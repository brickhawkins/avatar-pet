using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;

public class MasterDataManager : MonoBehaviour
{
    [SerializeField] ChannelUser channelUser;
    [SerializeField] ChannelConfig channelConfig;
    [SerializeField] ChannelItem channelItem;
    [SerializeField] ChannelMiniGame channelMiniGame;

    [SerializeField] ConfigDataService configDataService;
    [SerializeField] ItemDataService itemDataService;
    [SerializeField] MiniGameDataService miniGameDataService;

    private readonly CancellationTokenSource cts = new();
    private bool isCollecting;

    private async void Start()
    {
        await UniTask.WaitForEndOfFrame();

        if (channelUser == null)
        {
            Debug.LogError("MasterDataManager: ChannelUser reference is missing.");
            return;
        }

        channelUser.Current.Subscribe(HandleUserUpdated).AddTo(cts.Token);
    }

    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
    }

    private void HandleUserUpdated(UserData data)
    {
        if (string.IsNullOrEmpty(data.AccessToken))
            return;

        CollectMasterData().Forget();
    }

    public async UniTask CollectMasterData()
    {
        if (isCollecting)
            return;

        isCollecting = true;

        try
        {
            Logger.Log("Collecting master data");

            var configs = await configDataService.GetConfig();
            channelConfig.Current.Value = configs;

            var items = await itemDataService.GetItem();
            channelItem.Current.Value = items;

            var miniGames = await miniGameDataService.GetMiniGame();
            channelMiniGame.Current.Value = miniGames;

            Logger.Log("Master data collection completed");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to collect master data: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            isCollecting = false;
        }
    }
}
