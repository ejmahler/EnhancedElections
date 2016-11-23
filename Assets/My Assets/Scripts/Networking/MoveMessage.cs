using UnityEngine.Networking;

public class MoveMessage : MessageBase {
    public Point location;
    public string newDistrictName;
    public bool undo;

    public MoveMessage() { }
    public MoveMessage(Point location, string newDistrictName, bool undo)
    {
        this.location = location;
        this.newDistrictName = newDistrictName;
        this.undo = undo;
    }
}
