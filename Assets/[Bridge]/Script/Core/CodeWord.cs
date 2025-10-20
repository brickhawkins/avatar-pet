using System.Collections.Generic;


[System.Serializable]
public class Game
{
    public string code_word;
    public string game_name;
}


[System.Serializable]
public class CodeWord
{
    public List<Game> games;
}


