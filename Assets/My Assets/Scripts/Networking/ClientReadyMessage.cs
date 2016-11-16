using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ClientReadyMessage : MessageBase {
	public bool LocalClient;
	public static ClientReadyMessage New(bool local) {
        ClientReadyMessage msg = new ClientReadyMessage();
		msg.LocalClient = local;
        return msg;
	}
}
