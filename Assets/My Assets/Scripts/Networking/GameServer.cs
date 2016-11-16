using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Networking.NetworkSystem;

public class GameServer : MonoBehaviour
{
    private const short PORT = 26837;
	public const short ReadyMessage = MsgType.Highest + 1;

    public event System.Action<string> debugLog;
    public event System.Action<string> errorLog;

    private NetworkServerSimple server;

    private Coroutine gameStartCoroutine;

    // Use this for initialization
    void Awake()
    {
        server = new NetworkServerSimple();
    }

	void Update() {
		server.Update ();
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
			LogDebug("Server listening for LAN connections on " + Network.player.ipAddress);
        }
        else
        {
            LogError("Server could not begin listening for connections");
            yield break;
        }

        //listen for the local player and the remote player to 
        NetworkConnection localConnection = null;
        NetworkConnection remoteConnection = null;

		server.RegisterHandler (ReadyMessage, (msg) => {
            if(msg.ReadMessage<ClientReadyMessage>().LocalClient)
            {
                localConnection = msg.conn;
                LogDebug("Host player connected");
            }
            else
            {
                LogDebug("Networked player connected, IP Address: " + msg.conn.address);
                remoteConnection = msg.conn;
            }
        });
        
        yield return new WaitUntil(() => localConnection != null && remoteConnection != null);

        LogDebug("Both players connected to server. Beginning match in 3 seconds");

        //2 players are connected, so stop listening for ready messages
        server.UnregisterHandler(ReadyMessage);
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
