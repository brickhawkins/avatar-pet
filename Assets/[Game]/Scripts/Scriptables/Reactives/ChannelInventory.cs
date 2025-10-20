using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "User Inventory", menuName = "Game/Reactive/User/Inventory")]
public class ChannelInventory : ScriptableObject
{
    public AsyncReactiveProperty<UserInventory[]> Current = new(new UserInventory[0]);

    private void OnEnable() => Current = new(new UserInventory[0]);

    private void OnDisable() => Current.Dispose();
}