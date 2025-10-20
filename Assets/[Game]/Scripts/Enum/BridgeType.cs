[System.Serializable]
public enum BridgeType
{
    NONE,

    //From bridge
    //General
    INIT_USER, //{"user_id":userID} => temporary for supabase // user001

    //Home
    INIT_CHAT, //{"chat":"true", "mood":"mood_name"} // "false"

    //Quiz
    INIT_QUIZ, //[{"content":"content_string", "order": order_int, "status": "status_string"}]

    //Game
    INIT_GAME,//empty
    
    //To bridge
    UNITY_LOADED,//empty
    REQUEST_TOKEN, //empty
    REQUEST_SCENE, //{"name":"scene_name"} => PERIOD,DIARY,LECTURE,REPORT
    REQUEST_MINIGAME, //{"name":"game_name", "url":"game_url", "orientation": "portrait"}

    COMPLETE_MINIGAME,//empty
    COMPLETE_QUIZ,//{"pass":true/false} //adjusting

    ERROR,//{"message":message}
}