using UnityEngine;
using UnityEngine.Networking;

public class MatchSettings: MessageBase
{
    public int Width;
    public int Height;

    public int NumDistricts;

    public CityCell[] Cells;

	public static MatchSettings MakeSettings(int width, int height, int NumDistricts, CityCell[] cells = null)
    {
		MatchSettings item = new MatchSettings ();

		item.Width = width;
		item.Height = height;
		item.NumDistricts = NumDistricts;
		item.Cells = cells;

		return item;
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
