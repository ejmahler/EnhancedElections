using UnityEngine;
using UnityEngine.Networking;

public class MatchSettings: MessageBase
{
    public TurnManager.Player ThisPlayer;
    public TurnManager.Player FirstPlayer;

    public int Width;
    public int Height;

    public int NumDistricts;

    public CityCell[] Cells;

    public int RandomSeed;


	public static MatchSettings MakeSettings(TurnManager.Player player, TurnManager.Player firstPlayer, int width, int height, int NumDistricts, CityCell[] cells = null, int seed = -1)
    {
        Debug.Log("Making settings with cells");
		MatchSettings item = new MatchSettings ();

        item.ThisPlayer = player;
        item.FirstPlayer = player;
        item.Width = width;
		item.Height = height;
		item.NumDistricts = NumDistricts;
		item.Cells = cells;
        item.RandomSeed = seed;

		return item;
    }

    public MatchSettings()
    {

    }

    public MatchSettings(MatchSettings other)
    {
        this.ThisPlayer = other.ThisPlayer;
        this.FirstPlayer = other.FirstPlayer;
        this.Width = other.Width;
        this.Height = other.Height;
        this.NumDistricts = other.NumDistricts;
        this.Cells = other.Cells;
        this.RandomSeed = other.RandomSeed;
    }

	[System.Serializable]
    public struct CityCell
    {
        public Constituent.Party Party;
        public int DistrictId;

        public CityCell(Constituent.Party party, int district)
        {
            this.Party = party;
            this.DistrictId = district;
        }
    }
}
