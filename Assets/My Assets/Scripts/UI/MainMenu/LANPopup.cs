using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LANPopup : MainMenuPopup
{
    [SerializeField]
    private InputField _IpAddressField;

    [SerializeField]
    private Text _StatusField;

    private GameServer runningServer;
    private NetworkClient localClient;
    private bool connectionSuccess = false;

    public override void LaunchClicked()
    {
        GameObject serverObject = new GameObject("+MultiplayerServer");
        DontDestroyOnLoad(serverObject);
        runningServer = serverObject.AddComponent<GameServer>();

        runningServer.debugLog += WriteLog;
        runningServer.errorLog += WriteError;

        _StatusField.text = "";

		runningServer.PrepareLANGame(MatchSettings.MakeSettings(Utils.RandomPlayer(), Utils.RandomPlayer(), 21,14,11, seed: Random.Range(1,1000000)));
        
        localClient = runningServer.MakeLocalClient();
		localClient.RegisterHandler(MsgType.Connect, (msg) => {
			localClient.Send(GameServer.READY_MESSAGE, ClientReadyMessage.New(local: true));
		});

        localClient.RegisterHandler(GameServer.BEGIN_MATCH, (msg) =>
        {
            MakeConfigObject(msg.ReadMessage<MatchSettings>(), localClient);

            SceneManager.LoadScene("LANMode");

            connectionSuccess = true;
        });
    }

    public void ClientClicked()
    {
        NetworkClient client = GameServer.MakeRemoteClient(_IpAddressField.text);

		_StatusField.text = "";
		client.RegisterHandler(MsgType.Connect, (msg) => {
			WriteLog("Client connected to remote server at IP: " + msg.conn.address);
			client.Send(GameServer.READY_MESSAGE, ClientReadyMessage.New(local:false));
		});

        client.RegisterHandler(GameServer.BEGIN_MATCH, (msg) =>
        {
            MakeConfigObject(msg.ReadMessage<MatchSettings>(), client);

            SceneManager.LoadScene("LANMode");
        });
    }

    void OnDestroy()
    {
        if(!connectionSuccess && runningServer != null)
        {
            Destroy(runningServer.gameObject);
        }
    }

    private void WriteLog(string logMessage)
    {
        _StatusField.text += logMessage + "\n";
    }

    private void WriteError(string errorMessage)
    {
        _StatusField.text += string.Format("<color=\"red\">{0}</color>\n", errorMessage);
    }

    private void MakeConfigObject(MatchSettings settings, NetworkClient client)
    {
        GameObject obj = new GameObject("+MatchConfig");
        obj.tag = "MatchConfig";
        DontDestroyOnLoad(obj);

        MatchConfig config = obj.AddComponent<MatchConfig>();
        config.Settings = settings;
        config.Client = client;
    }
}
