using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniGame", menuName = "Game/Reactive/Master/MiniGame")]
public class ChannelMiniGame : ScriptableObject
{
    public AsyncReactiveProperty<MiniGame[]> Current = new(new MiniGame[0]);

    private void OnEnable() => Current = new(new MiniGame[0]);

    private void OnDisable() => Current.Dispose();
}