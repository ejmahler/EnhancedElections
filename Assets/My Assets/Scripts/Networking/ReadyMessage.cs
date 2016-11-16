using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ReadyMessage : MessageBase {
	public bool LocalClient;
	public ReadyMessage(bool local) {
		LocalClient = local;
	}
}
