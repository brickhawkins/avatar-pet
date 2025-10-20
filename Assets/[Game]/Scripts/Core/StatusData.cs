[System.Serializable]
public struct StatusData
{
    public StatusType Type;
    
    public StatusModifier Increment;
    public StatusModifier Decrement;
}

[System.Serializable]
public struct StatusModifier
{
    public ConfigType Type;
    public int Value;
}