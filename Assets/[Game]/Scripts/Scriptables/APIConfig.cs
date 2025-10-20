using UnityEngine;

[CreateAssetMenu(fileName = "New API Config", menuName = "Game/API Config")]
public class APIConfig : ScriptableObject
{
    public string BaseURL;

    [Header("Config")]
    public string ConfigPath;

    [Header("Items")]
    public string ItemPath;

    [Header("Mini Games")]
    public string MiniGamesPath;

    [Header("AccessToken")]
    public string LoginPath;
    public string RefreshPath;

    [Header("User Status")]
    public string GetStatusPath;
    public string AddStatusPath;
    public string UpdateStatusPath;

    [Header("User Inventory")]
    public string GetInventoryPath;
    public string AddInventoryPath;
    public string RemoveInventoryPath;
}