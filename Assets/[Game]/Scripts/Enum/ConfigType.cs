[System.Serializable]
public enum ConfigType
{
    //Decrement
    HUNGER_PER_SECOND,
    HYGIENE_PER_SECOND,
    ENERGY_PER_SECOND,
    MOOD_PER_SECOND,

    //Increment
    HUNGER_PER_ACTION,
    HYGIENE_PER_ACTION,
    ENERGY_PER_ACTION,
    MOOD_PER_ACTION,

    //status
    MAX_STATUS_VALUE,

    //navigation
    ENABLE_PERIOD,
    ENABLE_DIARY,
    ENABLE_CHAT,
    ENABLE_LECTURE,
    ENABLE_REPORT,
}