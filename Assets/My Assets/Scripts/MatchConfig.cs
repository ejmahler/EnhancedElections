using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class MatchConfig : MonoBehaviour
{
    private MatchSettings _settings;
    public MatchSettings Settings
    {
        get
        {
            if(_settings == null)
            {
                if (_SetupString == null || _SetupString.Length == 0)
                {
					_settings = MatchSettings.MakeSettings(Utils.RandomPlayer(), Utils.RandomPlayer(), _Width, _Height, _NumDistricts);
                }
                else
                {
					_settings = MatchSettings.MakeSettings(Utils.RandomPlayer(), Utils.RandomPlayer(), _Width, _Height, _NumDistricts, ParseString(_SetupString));
                }
            }
            return _settings;
        }
        set
        {
            _settings = value;
        }
    }

    public NetworkClient Client { get; set; }

    [SerializeField]
    private string _SetupString;

    [SerializeField]
    private int _Width;

    [SerializeField]
    private int _Height;

    [SerializeField]
    private int _NumDistricts;

    private MatchSettings.CityCell[] ParseString(string cityString)
    {
        string[] splitData = _SetupString.Split('|');

        if (splitData.Length != _Width * _Height)
        {
            throw new System.Exception("Wrong setup string length");
        }

        MatchSettings.CityCell[] results = new MatchSettings.CityCell[splitData.Length];

        for (int i = 0; i < splitData.Length; i++)
        {
            var partitioned = splitData[i].Split('-');
            Constituent.Party party = Utils.ParseEnumString<Constituent.Party>(partitioned[0]);
            int district = int.Parse(partitioned[1]);

			results[i] = new MatchSettings.CityCell(party, district);
        }

        return results;
    }
}
