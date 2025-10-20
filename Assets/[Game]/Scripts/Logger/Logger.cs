#nullable enable
using System;
using UnityEngine;

public static class Logger
{
    public static event Action<string>? OnLog;

    public static void Log(string message)
    {
        Debug.Log(message);

        try
        {
            OnLog?.Invoke(message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Logger OnLog handler exception: {ex}");
        }
    }
}
