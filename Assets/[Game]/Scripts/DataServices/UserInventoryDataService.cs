using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class UserInventoryDataService : MonoBehaviour
{
    [SerializeField] APIConfig APIConfig;
    [SerializeField] UserData userData;

    public async UniTask<UserInventory[]> GetInventory()
    {
        string url = APIConfig.BaseURL + APIConfig.GetInventoryPath;

        UserInventory[] userItems = new UserInventory[0];

        UnityWebRequest request = new(url, "GET")
        {
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Authorization", $"Bearer {userData.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                userItems = JsonConvert.DeserializeObject<UserInventory[]>(request.downloadHandler.text);
                Debug.Log($"User items parsed successfully. Count: {userItems.Length}");
            }
            catch (JsonException ex)
            {
                Debug.LogError($"Error parsing JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"Error fetching user item. Status: {request.responseCode}, Error: {request.error}");
        }

        return userItems;
    }

    public async UniTask<UserInventory> AddItem(string userID, int itemID)
    {
        string url = APIConfig.BaseURL + APIConfig.AddInventoryPath;

        string body = $"{{" +
            $"\"item_id\": \"{itemID}\"," +
            $"\"user_id\": \"{userID}\"}}";

        UserInventory newInventory = new();

        // Create the POST request
        UnityWebRequest request = new(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };

        // Set headers
        //request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", $"Bearer {userData.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=representation");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 201) // HTTP 201: Created
        {
            Debug.Log($"Item added successfully: {request.downloadHandler.text}");

            UserInventory[] inventories = JsonConvert.DeserializeObject<UserInventory[]>(request.downloadHandler.text);

            if (inventories.Length > 0)
            {
                newInventory = inventories[0];
                return newInventory;
            }
        }
        else
        {
            Debug.LogError($"Error creating user status. Status: {request.responseCode}, Error: {request.error}");
        }

        return newInventory;
    }

    public async UniTask RemoveItem(int userItemID)
    {
        string url = APIConfig.BaseURL + APIConfig.AddInventoryPath + "/" + userItemID;

        // Create the DELETE request
        UnityWebRequest request = new(url, "DELETE");

        // Set headers
        //request.SetRequestHeader("apikey", anonKey);
        request.SetRequestHeader("Authorization", $"Bearer {userData.AccessToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and await its completion
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success || request.responseCode == 204) // HTTP 204: No content
        {
            Debug.Log($"Item deleted successfully");
        }
        else
        {
            Debug.LogError($"Error deleting item. Status: {request.responseCode}, Error: {request.error}");
        }
    }
}