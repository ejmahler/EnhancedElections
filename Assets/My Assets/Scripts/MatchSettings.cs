using UnityEngine;
using System.Collections.Generic;

public class MatchSettings
{
    public readonly int Width;
    public readonly int Height;

    public readonly int NumDistricts;

    public readonly List<CityCell> Cells;

    public MatchSettings(int width, int height, int NumDistricts, List<CityCell> cells = null)
    {
        this.Width = width;
        this.Height = height;
        this.NumDistricts = NumDistricts;
        this.Cells = cells;
    }

    public struct CityCell
    {
        public readonly Constituent.Party Party;
        public readonly int DistrictId;

        public CityCell(Constituent.Party party, int district)
        {
            this.Party = party;
            this.DistrictId = district;
        }
    }
}
