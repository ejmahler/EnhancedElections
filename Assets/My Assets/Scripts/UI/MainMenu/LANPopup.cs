using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

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
        runningServer = serverObject.AddComponent<GameServer>();

        runningServer.debugLog += WriteLog;
        runningServer.errorLog += WriteError;

        _StatusField.text = "";

		runningServer.PrepareLANGame(MatchSettings.MakeSettings(2,3,4));

        localClient = runningServer.MakeLocalClient();
		localClient.RegisterHandler(MsgType.Connect, (msg) => {
			WriteLog("Local client connected");
			Debug.Log("connected: " + localClient.isConnected);
			Debug.Log("server ip: " + localClient.serverIp);
			localClient.Send(GameServer.ReadyMessage, new ReadyMessage(local:true));
		});
    }

    public void ClientClicked()
    {
        NetworkClient client = GameServer.MakeRemoteClient(_IpAddressField.text);

		_StatusField.text = "";
		client.RegisterHandler(MsgType.Connect, (msg) => {
			WriteLog("Client connected to remote server");
			client.Send(GameServer.ReadyMessage, new ReadyMessage(local:false));
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
}
