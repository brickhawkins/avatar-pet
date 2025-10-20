using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// UI Panel for sending message from Unity to a page
/// </summary>
public class UIFromUnity : MonoBehaviour
{
    // Reference to the input field
    [SerializeField]
    private InputField inputMessage;

    // Reference to the bridge
    [SerializeField]
    private BridgeScript bridge;

    /// <summary>
    /// Method for sending entered message to the bridge
    /// </summary>
    public void SendMessageToPage()
    {
        // Get a message
        var message = inputMessage.text;
        // Clear field
        //inputMessage.text = "";

        // Send a message to the brigde
        bridge.SendMessageToPage(message);
    }
}
