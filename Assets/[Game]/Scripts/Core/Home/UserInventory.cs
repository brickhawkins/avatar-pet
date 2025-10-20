using Newtonsoft.Json;

/*
 * "user_inventory_id": 4,
            "item": {
                "id": 5,
                "name": "SKIPTUTORIAL",
                "description": "",
                "price": 0,
                "effects": null
            }
*/

[System.Serializable]
public struct UserInventory
{
    [JsonProperty("user_inventory_id")]
    public int UserInventoryID;

    [JsonProperty("item")]
    public Item Item;
}