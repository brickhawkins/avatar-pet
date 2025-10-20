using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SendCode : MonoBehaviour
{
    public BridgeScript bridge;
    public string GameName;
    public string CodeWord;
   
    public void SendCodeToWeb()
    {


        Game NewGame = new Game()
        {
            game_name = GameName,
            code_word = CodeWord,

        };

        List<Game> listGame = new List<Game>();
        listGame.Add(NewGame);

        CodeWord newCode = new CodeWord()
        {
            games = listGame,
        };


        string json = JsonUtility.ToJson(newCode);

        bridge.SendMessageToPage(json);
        Application.Quit();
    }
}
