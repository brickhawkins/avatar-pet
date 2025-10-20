using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Status", menuName = "Game/Reactive/User/Status")]
public class ChannelStatus : ScriptableObject
{
    public AsyncReactiveProperty<UserStatus[]> CurrentStatus = new(new UserStatus[4]);
    public AsyncReactiveProperty<UserStatus[]> CurrentTime = new(new UserStatus[4]);

    private void OnEnable()
    {
        CurrentStatus = new(new UserStatus[4]);
        CurrentTime = new(new UserStatus[4]);
    }

    private void OnDisable()
    {
        CurrentStatus.Dispose();
        CurrentTime.Dispose();
    }

    public bool GetStatus(StatusType type, out UserStatus status)
    {
        status = new();

        for (int i = 0; i < CurrentStatus.Value.Length; i++)
        {
            if (CurrentStatus.Value[i].Key != type) continue;

            status = CurrentStatus.Value[i];
            return true;
        }
        
        return false;
    }

    public bool GetStatusTime(StatusType type, out UserStatus status)
    {
        status = new();

        for (int i = 0; i < CurrentTime.Value.Length; i++)
        {
            if (CurrentTime.Value[i].Key != type) continue;

            status = CurrentTime.Value[i];
            return true;
        }

        return false;
    }
}