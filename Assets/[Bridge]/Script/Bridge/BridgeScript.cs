using UnityEngine;

/// <summary>
/// Bridge used to communicate with a page
/// </summary>
public class BridgeScript : MonoBehaviour
{
    public delegate void EventBridge(string message);
    public event EventBridge OnReceiveMessage;

    /// <summary>
    /// Receives the message from a page.
    /// </summary>
    /// <param name="message">Message.</param>
    public void ReceiveMessageFromPage(string message)
    {
        OnReceiveMessage?.Invoke(message);
    }

    /// <summary>
    /// Sends the message to a page.
    /// </summary>
    /// <param name="message">Message.</param>
    public void SendMessageToPage(string message)
    {
        // Sends the message through JS plugin
        WebGLPluginJS.SendMessageToPage(message);
    }
}
