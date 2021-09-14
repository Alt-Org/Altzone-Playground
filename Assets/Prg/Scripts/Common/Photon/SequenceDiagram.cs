using Photon.Pun;
using System.Diagnostics;

public enum SD
{
    CONNECT,
    DISCONNECT,
    JOIN_LOBBY,
    LEAVE_LOBBY,
    CREATE_ROOM,
    CLOSE_ROOM,
    JOIN_ROOM,
    ENTER_ROOM,
    LEAVE_ROOM,
    MASTER_CLIENT,
}

public static class SequenceDiagram
{
    private static bool isSendEmitted;

    [Conditional("USE_SEQUENCE")]
    public static void send(string sender, SD message)
    {
        var line = $"SEQUENCE send -> {sender} : {message}";
#if FORCE_LOG || DEVELOPMENT_BUILD
        Debug.Log(line);
#else
        UnityEngine.Debug.Log(line);
#endif
        isSendEmitted = true;
    }

    [Conditional("USE_SEQUENCE")]
    public static void receive(string receiver, SD message)
    {
        string info = string.Empty;
        if (!isSendEmitted)
        {
            var frame = new StackFrame(2);
            var method = frame.GetMethod();
            info = $" <-- {method.GetFullName()}";
        }
        var line = $"SEQUENCE recv <- {receiver} : {message} [{PhotonNetwork.NetworkClientState}]{info}";
#if FORCE_LOG || DEVELOPMENT_BUILD
        Debug.Log(line);
#else
        UnityEngine.Debug.Log(line);
#endif
        isSendEmitted = false;
    }

    [Conditional("USE_SEQUENCE")]
    public static void status(string sender, SD message)
    {
        string info = string.Empty;
        if (PhotonNetwork.InRoom)
        {
            info = $" (#{PhotonNetwork.CurrentRoom.PlayerCount})";
        }
        var line = $"SEQUENCE status : {sender} : {message} [{PhotonNetwork.NetworkClientState}]{info}";
#if FORCE_LOG || DEVELOPMENT_BUILD
        Debug.Log(line);
#else
        UnityEngine.Debug.Log(line);
#endif
    }
}