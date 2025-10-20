using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Netkit
{
    public enum HttpVerb { GET, POST, PUT, PATCH, DELETE }

    // ---- Auth plumbing ------------------------------------------------------
    public interface IAuthProvider
    {
        UniTask<string> GetAccessTokenAsync(CancellationToken ct);
        UniTask<bool> RefreshTokenAsync(CancellationToken ct);
    }

    [Serializable]
    public sealed class RestClientOptions
    {
        public string BaseUrl = "";
        public int TimeoutSeconds = 30;
        public int MaxRetries = 2;                    // total attempts = 1 + MaxRetries (not counting a token-refresh replay)
        public float RetryBaseDelaySeconds = 0.35f;   // backoff base
        public int MaxConcurrentRequests = 6;         // 0 or less = unlimited
        public Dictionary<string, string> DefaultHeaders = new();
        public bool LogRequests = false;
        public bool LogResponses = false;

        // Auth
        public IAuthProvider AuthProvider = null;     // if set, Authorization header is added per request
        public string AuthHeaderName = "Authorization";
        public string AuthScheme = "Bearer ";        // prefix applied before token
        public bool AutoRefreshToken = true;          // on HTTP 401: refresh and replay once

        // Global event hooks
        public Action<long, string> OnError;          // statusCode, body
        public Action<long, string> OnSuccess;        // statusCode, body
    }

    public sealed class RestClient
    {
        private readonly RestClientOptions _opt;
        private readonly System.Threading.SemaphoreSlim _semaphore;

        public RestClient(RestClientOptions options)
        {
            _opt = options ?? new RestClientOptions();
            _semaphore = (_opt.MaxConcurrentRequests > 0)
                ? new System.Threading.SemaphoreSlim(_opt.MaxConcurrentRequests, _opt.MaxConcurrentRequests)
                : null;
        }

        // ===================== Public API (JSON) =============================
        public UniTask<T> GetJson<T>(string path, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                     Action<long, string> onSuccess = null, Action<long, string> onError = null)
            => SendJson<T>(HttpVerb.GET, path, default(object), headers, ct, onSuccess, onError);

        public UniTask<TResponse> PostJson<TRequest, TResponse>(string path, TRequest bodyObj, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                     Action<long, string> onSuccess = null, Action<long, string> onError = null)
            => SendJson<TResponse>(HttpVerb.POST, path, bodyObj, headers, ct, onSuccess, onError);

        public UniTask<TResponse> PutJson<TRequest, TResponse>(string path, TRequest bodyObj, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                     Action<long, string> onSuccess = null, Action<long, string> onError = null)
            => SendJson<TResponse>(HttpVerb.PUT, path, bodyObj, headers, ct, onSuccess, onError);

        public UniTask<T> PatchJson<TRequest, T>(string path, TRequest bodyObj, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                     Action<long, string> onSuccess = null, Action<long, string> onError = null)
            => SendJson<T>(HttpVerb.PATCH, path, bodyObj, headers, ct, onSuccess, onError);

        public UniTask Delete(string path, Dictionary<string, string> headers = null, CancellationToken ct = default,
                              Action<long, string> onSuccess = null, Action<long, string> onError = null)
            => SendRaw(HttpVerb.DELETE, path, null, headers, ct, onSuccess, onError);

        // Optional raw JSON helpers
        public UniTask<TRes> PostJsonRaw<TRes>(string path, string json, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                               Action<long, string> onSuccess = null, Action<long, string> onError = null)
        {
            var bodyBytes = json != null ? Encoding.UTF8.GetBytes(json) : null;
            return SendJsonCore<TRes>(HttpVerb.POST, path, bodyBytes, headers, ct, onSuccess, onError);
        }
        public UniTask<TRes> PatchJsonRaw<TRes>(string path, string json, Dictionary<string, string> headers = null, CancellationToken ct = default,
                                                Action<long, string> onSuccess = null, Action<long, string> onError = null)
        {
            var bodyBytes = json != null ? Encoding.UTF8.GetBytes(json) : null;
            return SendJsonCore<TRes>(HttpVerb.PATCH, path, bodyBytes, headers, ct, onSuccess, onError);
        }

        // ===================== Internals (JSON) ==============================
        private UniTask<T> SendJson<T>(HttpVerb verb, string path, object bodyObj, Dictionary<string, string> headers, CancellationToken ct,
                                       Action<long, string> onSuccess, Action<long, string> onError)
        {
            byte[] bodyBytes = null;
            if (bodyObj != null)
            {
                string json = JsonConvert.SerializeObject(bodyObj);
                bodyBytes = Encoding.UTF8.GetBytes(json);
            }
            return SendJsonCore<T>(verb, path, bodyBytes, headers, ct, onSuccess, onError);
        }

        private async UniTask<T> SendJsonCore<T>(HttpVerb verb, string path, byte[] bodyBytes, Dictionary<string, string> headers, CancellationToken ct,
                                                 Action<long, string> onSuccess, Action<long, string> onError)
        {
            var data = await SendWithRetries(
                buildWithToken: (token) => BuildRequest(verb, path, bodyBytes, headers, token),
                ct: ct,
                onSuccess: onSuccess,
                onError: onError
            );

            var text = data.Text;
            if (typeof(T) == typeof(string))
                return (T)(object)text;

            try
            {
                var obj = JsonConvert.DeserializeObject<T>(text);
                if (obj == null)
                    throw new RestException((int)data.Code, "JSON parse produced null", text);
                return obj;
            }
            catch (Exception ex)
            {
                throw new RestException((int)data.Code, $"JSON parse failed: {ex.Message}", text);
            }
        }

        private async UniTask SendRaw(HttpVerb verb, string path, byte[] bodyBytes, Dictionary<string, string> headers, CancellationToken ct,
                                      Action<long, string> onSuccess, Action<long, string> onError)
        {
            await SendWithRetries(
                buildWithToken: (token) => BuildRequest(verb, path, bodyBytes, headers, token),
                ct: ct,
                onSuccess: onSuccess,
                onError: onError
            );
        }

        // ===================== Request construction ==========================
        private UnityWebRequest BuildRequest(HttpVerb verb, string path, byte[] bodyBytes, Dictionary<string, string> headers, string bearerToken)
        {
            var url = ComposeUrl(path);
            UnityWebRequest req;

            switch (verb)
            {
                case HttpVerb.GET:
                    req = UnityWebRequest.Get(url);
                    break;
                case HttpVerb.DELETE:
                    req = UnityWebRequest.Delete(url);
                    break;
                case HttpVerb.POST:
                case HttpVerb.PUT:
                case HttpVerb.PATCH:
                    req = new UnityWebRequest(url, verb == HttpVerb.POST ? UnityWebRequest.kHttpVerbPOST : (verb == HttpVerb.PUT ? UnityWebRequest.kHttpVerbPUT : "PATCH"))
                    {
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    if (bodyBytes != null)
                    {
                        req.uploadHandler = new UploadHandlerRaw(bodyBytes);
                        req.SetRequestHeader("Content-Type", "application/json");
                    }
                    else
                    {
                        req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
                    }
                    break;
                default:
                    req = UnityWebRequest.Get(url);
                    break;
            }

            // default headers
            if (_opt.DefaultHeaders != null)
            {
                foreach (var kv in _opt.DefaultHeaders)
                    req.SetRequestHeader(kv.Key, kv.Value);
            }
            // per-call headers
            if (headers != null)
            {
                foreach (var kv in headers)
                    req.SetRequestHeader(kv.Key, kv.Value);
            }
            // auth header (late-binding)
            if (!string.IsNullOrEmpty(bearerToken))
            {
                req.SetRequestHeader(_opt.AuthHeaderName, _opt.AuthScheme + bearerToken);
            }

            if (_opt.TimeoutSeconds > 0)
                req.timeout = _opt.TimeoutSeconds; // browsers may ignore; we add UniTask Timeout too

            if (_opt.LogRequests)
            {
                var msg = $"HTTP {verb} {url}";
                if (bodyBytes != null)
                    msg += $"\nBody: {Encoding.UTF8.GetString(bodyBytes)}";
                Debug.Log(msg);
            }

            return req;
        }

        private string ComposeUrl(string path)
        {
            if (string.IsNullOrEmpty(_opt.BaseUrl)) return path;
            if (string.IsNullOrEmpty(path)) return _opt.BaseUrl;
            if (_opt.BaseUrl.EndsWith("/") && path.StartsWith("/")) return _opt.BaseUrl + path.Substring(1);
            if (!_opt.BaseUrl.EndsWith("/") && !path.StartsWith("/")) return _opt.BaseUrl + "/" + path;
            return _opt.BaseUrl + path;
        }

        // ===================== Transport & resilience ========================
        private struct ResponseBlob
        {
            public long Code;
            public byte[] Bytes;
            public string Text => Bytes != null ? Encoding.UTF8.GetString(Bytes) : string.Empty;
        }

        private async UniTask<ResponseBlob> SendWithRetries(
            Func<string, UnityWebRequest> buildWithToken,
            CancellationToken ct,
            Action<long, string> onSuccess,
            Action<long, string> onError)
        {
            if (_semaphore != null) await _semaphore.WaitAsync(ct);
            try
            {
                var attempts = 0;
                var maxAttempts = Mathf.Max(1, 1 + _opt.MaxRetries);
                var baseDelay = Mathf.Max(0.01f, _opt.RetryBaseDelaySeconds);

                // token lifecycle vars
                string token = null;
                bool triedRefreshForThisRequest = false;

                while (true)
                {
                    attempts++;

                    // acquire or re-acquire token each attempt
                    if (_opt.AuthProvider != null)
                    {
                        token = await _opt.AuthProvider.GetAccessTokenAsync(ct);
                    }

                    using var req = buildWithToken(token);

                    try
                    {
                        var op = req.SendWebRequest();
                        var timeout = _opt.TimeoutSeconds > 0 ? TimeSpan.FromSeconds(_opt.TimeoutSeconds) : (TimeSpan?)null;

                        if (timeout.HasValue)
                            await op.ToUniTask(cancellationToken: ct).Timeout(timeout.Value);
                        else
                            await op.ToUniTask(cancellationToken: ct);

                        var code = req.responseCode;
                        var ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
                        var bytes = req.downloadHandler?.data;
                        var blob = new ResponseBlob { Code = code, Bytes = bytes };

                        if (ok)
                        {
                            _opt.OnSuccess?.Invoke(code, blob.Text);
                            onSuccess?.Invoke(code, blob.Text);
                            if (_opt.LogResponses)
                                Debug.Log($"HTTP {(int)code} {req.url}\n{blob.Text}");
                            return blob;
                        }
                        else
                        {
                            // If unauthorized, optionally refresh once and replay without consuming a retry attempt.
                            if (_opt.AutoRefreshToken && _opt.AuthProvider != null && code == 401 && !triedRefreshForThisRequest)
                            {
                                triedRefreshForThisRequest = true;
                                bool refreshed = await _opt.AuthProvider.RefreshTokenAsync(ct);
                                if (refreshed)
                                {
                                    // Immediately retry with fresh token; do not backoff or increment attempts further here.
                                    continue;
                                }
                            }

                            _opt.OnError?.Invoke(code, blob.Text);
                            onError?.Invoke(code, blob.Text);
                            throw new RestException((int)code, req.error, blob.Text);
                        }
                    }
                    catch (TimeoutException tex)
                    {
                        if (attempts >= maxAttempts)
                        {
                            _opt.OnError?.Invoke(408, tex.Message);
                            onError?.Invoke(408, tex.Message);
                            throw new RestException(408, "Request Timeout", tex.Message);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (RestException rex)
                    {
                        // transient? backoff & retry
                        if (attempts >= maxAttempts || !IsTransient(rex.StatusCode))
                        {
                            _opt.OnError?.Invoke(rex.StatusCode, rex.Body);
                            onError?.Invoke(rex.StatusCode, rex.Body);
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (attempts >= maxAttempts)
                        {
                            _opt.OnError?.Invoke(-1, ex.Message);
                            onError?.Invoke(-1, ex.Message);
                            throw new RestException(-1, "Transport Error", ex.Message);
                        }
                    }

                    // backoff (no backoff for the immediate 401-refresh replay; that goes through 'continue;')
                    var delay = baseDelay * Mathf.Pow(2f, attempts - 1);
                    var jitter = UnityEngine.Random.Range(0f, baseDelay);
                    await UniTask.Delay(TimeSpan.FromSeconds(delay + jitter), cancellationToken: ct);
                }
            }
            finally
            {
                if (_semaphore != null) _semaphore.Release();
            }
        }

        private static bool IsTransient(int status)
        {
            if (status == 408 || status == 425 || status == 429) return true;
            return status >= 500 && status <= 599;
        }
    }

    public sealed class RestException : Exception
    {
        public int StatusCode { get; }
        public string Body { get; }
        public RestException(int status, string message, string body = null) : base(message)
        {
            StatusCode = status;
            Body = body;
        }
        public override string ToString() => $"HTTP {StatusCode}: {Message}\n{Body}";
    }
}

// ===================== Examples =====================
// 1) A minimal token provider
// public sealed class MyAuthProvider : Netkit.IAuthProvider
// {
//     private string _accessToken;
//     private string _refreshToken;
//     private readonly Netkit.RestClient _authClient; // can be a separate client without AuthProvider
//
//     public MyAuthProvider(Netkit.RestClient authClient, string access, string refresh)
//     { _authClient = authClient; _accessToken = access; _refreshToken = refresh; }
//
//     public UniTask<string> GetAccessTokenAsync(CancellationToken ct)
//         => UniTask.FromResult(_accessToken);
//
//     public async UniTask<bool> RefreshTokenAsync(CancellationToken ct)
//     {
//         // Example refresh flow; adjust to your backend
//         var body = new { refreshToken = _refreshToken };
//         try
//         {
//             var res = await _authClient.PostJson<object, RefreshResponse>("/auth/refresh", body, ct: ct);
//             _accessToken = res.accessToken;
//             _refreshToken = res.refreshToken ?? _refreshToken; // keep old if server doesn't return new
//             return true;
//         }
//         catch
//         {
//             return false;
//         }
//     }
//
//     [Serializable] public class RefreshResponse { public string accessToken; public string refreshToken; }
// }
//
// 2) Construct the main client with auto-refresh
// var authless = new Netkit.RestClient(new Netkit.RestClientOptions
// {
//     BaseUrl = "https://api.example.com",
//     TimeoutSeconds = 15,
// });
// var provider = new MyAuthProvider(authless, access: "<init-access>", refresh: "<init-refresh>");
// var client = new Netkit.RestClient(new Netkit.RestClientOptions
// {
//     BaseUrl = "https://api.example.com",
//     DefaultHeaders = new() { { "Accept", "application/json" } },
//     AuthProvider = provider,         // adds Authorization: Bearer <token>
//     AutoRefreshToken = true,        // on 401, refresh & replay once
//     MaxRetries = 2,
//     MaxConcurrentRequests = 4,
//     OnError = (code, body) => Debug.LogError($"ERR {code}: {body}"),
//     OnSuccess = (code, body) => Debug.Log($"OK {code}")
// });
//
// 3) Use it as usual; no need to recreate the client when token changes
// [Serializable] class KV { public string key; public string value; }
// [Serializable] class ApiResponse { public string status; public string message; }
//
// async UniTask UpdateUser(Netkit.RestClient c)
// {
//     var payload = new KV { key = "health", value = "42" };
//     var res = await c.PatchJson<KV, ApiResponse>("/v1/update", payload);
//     Debug.Log($"{res.status}: {res.message}");
// }
