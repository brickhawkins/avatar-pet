using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game/Reactive/Master/Item")]
public class ChannelItem : ScriptableObject
{
    public AsyncReactiveProperty<Item[]> Current = new(new Item[0]);

    private void OnEnable() => Current = new(new Item[0]);

    private void OnDisable() => Current.Dispose();

    public Item GetItem(int id)
    {
        for (int i = 0; i < Current.Value.Length; i++)
            if (Current.Value[i].ID == id)
                return Current.Value[i];

        return new();
    }
}