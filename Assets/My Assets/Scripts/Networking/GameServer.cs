using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Networking.NetworkSystem;

public class GameServer : MonoBehaviour
{
    private const short PORT = 26837;

    public event System.Action<string> debugLog;
    public event System.Action<string> errorLog;

    private NetworkServerSimple server;

    private Coroutine gameStartCoroutine;

    // Use this for initialization
    void Awake()
    {
        server = new NetworkServerSimple();
    }

    public void PrepareLANGame(MatchSettings localSettings)
    {
        gameStartCoroutine = StartCoroutine(WaitForGameBegin(localSettings));
    }

    public void CancelLANGame()
    {
        server.Stop();
    }

    public NetworkClient MakeLocalClient()
    {
        NetworkClient localClient = new NetworkClient();
        localClient.RegisterHandler(MsgType.Connect, (msg) => { LogDebug("Local client has connected to server"); });
        localClient.RegisterHandler(MsgType.Error,
            (msg) => { LogError(((NetworkError)msg.ReadMessage<ErrorMessage>().errorCode).ToString()); }
            );

        localClient.Connect("localhost", PORT);

        return localClient;
    }

    private IEnumerator WaitForGameBegin(MatchSettings localSettings)
    {
        if(server.Listen(PORT))
        {
            LogDebug("Server listening for LAN connections");
        }
        else
        {
            LogError("Server could not begin listening for connections");
            yield break;
        }
    }

    private void LogDebug(string msg)
    {
        if (debugLog != null) debugLog(msg);
    }

    private void LogError(string errorMsg)
    {
        if (errorLog != null) errorLog(errorMsg);
    }

    public static NetworkClient MakeRemoteClient(string remoteIp)
    {
        NetworkClient client = new NetworkClient();
        client.Connect(remoteIp, PORT);
        return client;
    }
}
