using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Bridge", menuName = "Game/Reactive/Bridge")]
public class ChannelBridge : ScriptableObject
{
    public AsyncReactiveProperty<BridgeData> Current = new(new());

    private void OnEnable() => Current = new(new());

    private void OnDisable() => Current.Dispose();
}