using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "User", menuName = "Game/Reactive/User/User")]
public class ChannelUser : ScriptableObject
{
    public AsyncReactiveProperty<UserData> Current = new (new ());

    private void OnEnable() => Current = new(new());

    private void OnDisable() => Current.Dispose();
}