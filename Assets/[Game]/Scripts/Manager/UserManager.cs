using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Newtonsoft.Json;
using System;
using System.Threading;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [SerializeField] ChannelBridge bridge;
    [SerializeField] ChannelUser user;
    [SerializeField] ChannelConfig channelConfig;
    [SerializeField] ChannelInventory inventory;
    [SerializeField] ChannelStatus status;

    [SerializeField] UserDataService userDataService;
    [SerializeField] UserInventoryDataService userInventoryDataService;
    [SerializeField] UserStatusDataService userStatusDataService;

    private readonly CancellationTokenSource cts = new();
    private bool isProcessingUserFlow;

    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private async void Start()
    {
        await UniTask.WaitForEndOfFrame();

        user.Current.Subscribe(CheckUserData).AddTo(cts.Token);
        bridge.Current.Subscribe(CheckBridgeData).AddTo(cts.Token);
    }

    private void CheckBridgeData(BridgeData data)
    {
        if (data.Key != BridgeType.INIT_USER) return;

        Logger.Log($"Init User from Web: {data.Value}");

        user.Current.Value = JsonConvert.DeserializeObject<UserData>(data.Value);
    }

    private void CheckUserData(UserData data)
    {
        if (data.AccessToken == string.Empty) return;

        Logger.Log($"User Logged In: {data.UserName}");

        ProcessUserFlow().Forget();
    }

    public void Login()
    {
        userDataService.Login().Forget();
        Logger.Log($"Force Login Requested");
    }

    private async UniTask ProcessUserFlow()
    {
        if (isProcessingUserFlow)
            return;

        isProcessingUserFlow = true;

        try
        {
            if (await WaitForMasterData())
            {
                await CheckInventoryData();
            }
        }
        finally
        {
            isProcessingUserFlow = false;
        }
    }

    private async UniTask<bool> WaitForMasterData()
    {
        if (channelConfig == null)
        {
            Debug.LogError("UserManager: ChannelConfig reference is missing.");
            return false;
        }

        if (channelConfig.Current.Value.Length > 0)
            return true;

        Logger.Log("Waiting for master data to be ready...");

        try
        {
            await channelConfig.Current
                .Where(configs => configs.Length > 0)
                .FirstAsync(cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            Debug.LogWarning("UserManager: Waiting for master data was cancelled.");
            return false;
        }
    }

    private async UniTask CheckInventoryData()
    {
        Logger.Log($"Checking Inventory Data...");
        inventory.Current.Value = await userInventoryDataService.GetInventory();

        if (inventory.Current.Value.Length == 0)
        {
            Logger.Log($"No Inventory Found, Start Tutorial");
            //Start Tutorial
            //await tutorial finish
            //await CheckStatusData();
            return;
        }
        else
        {
            foreach (var item in inventory.Current.Value)
            {
                if (item.Item.Name == "SKIPTUTORIAL")
                {
                    Logger.Log($"Skip Tutorial Item Found, Load Home");
                    await CheckStatusData();
                    return;
                }
                else
                {
                    Logger.Log($"No Skip Tutorial Item Found, Start Tutorial");
                    //Start Tutorial
                    //await tutorial finish
                    //await CheckStatusData();
                    return;
                }
            }
        }
    }

    private async UniTask CheckStatusData()
    {
        await userStatusDataService.GetUserStatus();

    }
}
