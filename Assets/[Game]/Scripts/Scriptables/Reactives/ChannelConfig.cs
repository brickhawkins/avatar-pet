using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Game/Reactive/Master/Config")]
public class ChannelConfig : ScriptableObject
{
    public AsyncReactiveProperty<Config[]> Current = new(new Config[0]);

    private void OnEnable() => Current = new(new Config[0]);

    private void OnDisable() => Current.Dispose();

    public Config GetConfig(ConfigType type)
    {
        for (int i = 0; i < Current.Value.Length; i++)
            if(Current.Value[i].Type == type)
                return Current.Value[i];

        return new();
    }
}