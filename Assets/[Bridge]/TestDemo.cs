using TMPro;
using UnityEngine;

[System.Serializable]
public class DataTest
{
    public string gamename;
    public int level;
    public int finalscore;
    public string method;
}

public class TestDemo : MonoBehaviour
{
    public TMP_Text TxtFromPage;
    public BridgeScript bridge;
    
    public void SendPOST()
    {
        DataTest NewData = new DataTest()
        {
            gamename = "flappybird",
            level = 1,
            finalscore = 10,
            method = "POST"
        };


        string json = JsonUtility.ToJson(NewData);
        bridge.SendMessageToPage(json);
    }

    public void SendGET()
    {
        DataTest NewData = new DataTest()
        {
            gamename = "",
            level = 0,
            finalscore = 0,
            method = "GET"
        };


        string json = JsonUtility.ToJson(NewData);
        bridge.SendMessageToPage(json);
    }


    public void ClearFromPage()
    {
        TxtFromPage.text = string.Empty;
    }
}
