using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour
{

    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private int numDistricts;
    [SerializeField]
    private float populationFalloffDistance;

    [SerializeField]
    private GameObject constituentPrefab;
    [SerializeField]
    private GameObject districtPrefab;

    [SerializeField]
    private string setupString;

    public List<District> Districts { get; private set; }
    public List<Constituent> Constituents { get; private set; }

    public int MinDistrictSize { get; private set; }

    void Awake()
    {
        float baseX = -width / 2.0f + 0.5f;
        float baseY = -height / 2.0f + 0.5f;

        Dictionary<Point, Constituent> locationDict = new Dictionary<Point, Constituent>();

        //create each constituent
        Constituents = new List<Constituent>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(baseX + x, baseY + y, 0.0f);
                var constituent = MakeConstituent(position);
                locationDict.Add(new Point(x, y), constituent);
                Constituents.Add(constituent);
            }
        }

        //assign neighbors to each constistuent
        foreach (Point p in locationDict.Keys)
        {
            Constituent c = locationDict[p];
            locationDict.TryGetValue(p + new Point(0, 1), out c.neighborTop);
            locationDict.TryGetValue(p + new Point(0, -1), out c.neighborBottom);
            locationDict.TryGetValue(p + new Point(-1, 0), out c.neighborLeft);
            locationDict.TryGetValue(p + new Point(1, 0), out c.neighborRight);
        }

        if (setupString.Length > 0)
        {
            ParseCity(setupString);

            foreach (District d in Districts)
            {
                d.UpdateMemberData();
            }
        }
        else
        {
            PartitionCity(Constituents);

            foreach (District d in Districts)
            {
                d.UpdateMemberData();
            }

            //balance pass: make sure there are an equal number of red and blue constituents
            BalanceConstituentCounts();

            //balance pass: make sure there are an equal number of red and blue districts
            BalanceDistrictCounts();
        }

        float averageDistrictSize = (float)Constituents.Count((c) => { return c.party != Constituent.Party.None; }) / numDistricts;
        MinDistrictSize = (int)System.Math.Round(averageDistrictSize * 0.75f);

        foreach (Constituent c in Constituents)
        {
            c.UpdateBorders();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(SerializeCity());
        }
    }

    private void ParseCity(string cityString)
    {
        string[] splitData = setupString.Split('|');

        if (splitData.Length != Constituents.Count)
        {
            throw new System.Exception("Wrong setup string length");
        }

        SortedDictionary<string, District> districtMap = new SortedDictionary<string, District>();

        for (int i = 0; i < splitData.Length; i++)
        {
            var partitioned = splitData[i].Split('-');
            Constituents[i].party = Utils.ParseEnumString<Constituent.Party>(partitioned[0]);

            if (!districtMap.ContainsKey(partitioned[1]))
            {
                District newDistrict = MakeDistrict(partitioned[1]);
                districtMap.Add(partitioned[1], newDistrict);
            }

            Constituents[i].District = districtMap[partitioned[1]];
        }

        Districts = districtMap.Values.ToList();
    }

    private string SerializeCity()
    {
        return string.Join("|", Constituents.Select((c) =>
        {
            return c.party.ToString() + "-" + c.District.name;
        }).ToArray());
    }

    private void PartitionCity(List<Constituent> constituents)
    {
        //partition the constituents into districts
        Districts = new List<District>();
        var partitionResults = PartitionConstituents(constituents, numDistricts);
        int i = 0;
        foreach (var partition in partitionResults)
        {
            District d = MakeDistrict((++i).ToString());
            Districts.Add(d);

            foreach (var c in partition)
            {
                c.District = d;
            }
        }

        //go through the partitions again and make sure there are no disconnected pieces
        foreach (var partition in partitionResults)
        {
            var components = Utils.ConnectedComponents(partition, (c) =>
            {
                return c.Neighbors.Where((n) => { return n != null && c.District == n.District; });
            });

            //if the component count is more than 1, we have some disconnected pieces
            if (components.Count > 1)
            {
                //go through all components except the biggest
                var smallerComponents = components.OrderByDescending((component) => { return component.Count; }).Skip(1);
                foreach (var component in smallerComponents)
                {
                    //'component' is a disconnected piece. the simplest thing to do is just move each piece into one of the neighboring districts
                    foreach (var c in component)
                    {
                        District newDistrict = c.District;
                        while (Object.ReferenceEquals(newDistrict, c.District))
                        {
                            newDistrict = Utils.ChooseRandom(c.Neighbors.Where((n) => { return n != null; }).ToList()).District;
                        }
                        c.District = newDistrict;
                    }
                }
            }
        }
    }

    private Constituent MakeConstituent(Vector3 position)
    {
        GameObject obj = Instantiate(constituentPrefab, position, new Quaternion()) as GameObject;
        obj.transform.parent = transform;

        var constituent = obj.GetComponent<Constituent>();

        var length = position.magnitude;
        var adjusted = System.Math.Max(0.0f, length - populationFalloffDistance);

        //choose a party for this constituent
        var partyChances = new SortedList<Constituent.Party, float>();
        partyChances.Add(Constituent.Party.None, adjusted);
        partyChances.Add(Constituent.Party.Yellow, 0.1f);
        partyChances.Add(Constituent.Party.Blue, 1.0f);
        partyChances.Add(Constituent.Party.Red, 1.0f);
        constituent.party = Utils.ChooseWeightedRandom(partyChances);

        return constituent;
    }

    private District MakeDistrict(string name)
    {
        GameObject obj = Instantiate(districtPrefab, new Vector3(), new Quaternion()) as GameObject;
        obj.transform.parent = transform;

        var district = obj.GetComponent<District>();
        district.name = name;

        return district;
    }

    private void BalanceConstituentCounts()
    {
        var RedConstituents = Constituents.Where((c) => c.party == Constituent.Party.Red);
        var BlueConstituents = Constituents.Where((c) => c.party == Constituent.Party.Blue);

        var RedCount = RedConstituents.Count();
        var BlueCount = BlueConstituents.Count();

        HashSet<District> changedDistricts = new HashSet<District>();
        if (RedCount > BlueConstituents.Count())
        {
            //find the disparity, and convert half that many to blue
            int diff = RedCount - BlueCount;

            //if diff is odd, convert one red to brown
            if ((diff % 2) == 1)
            {
                Constituent c = Utils.ChooseRandom(RedConstituents.ToList());
                c.party = Constituent.Party.Yellow;
                changedDistricts.Add(c.District);
            }

            //choose enough constituents randomly to even things out, and convert them to the other party
            foreach (Constituent c in Utils.ChooseKRandom(RedConstituents.ToList(), diff / 2))
            {
                c.party = Constituent.Party.Blue;
                changedDistricts.Add(c.District);
            }
        }
        else if (RedCount < BlueCount)
        {
            //find the disparity, and convert half that many to red
            int diff = BlueCount - RedCount;

            //if diff is odd, convert one red to brown
            if ((diff % 2) == 1)
            {
                Constituent c = Utils.ChooseRandom(RedConstituents.ToList());
                c.party = Constituent.Party.Yellow;
                changedDistricts.Add(c.District);
            }

            //choose enough constituents randomly to even things out, and convert them to the other party

            foreach (Constituent c in Utils.ChooseKRandom(BlueConstituents.ToList(), diff / 2))
            {
                c.party = Constituent.Party.Red;
                changedDistricts.Add(c.District);
            }
        }

        //update the member counts of every district we changed
        foreach (District d in changedDistricts)
        {
            d.UpdateMemberData();
        }
    }

    private void BalanceDistrictCounts()
    {
        var redDistricts = Districts.Where((c) => { return c.CurrentMajority == Constituent.Party.Red; });
        var blueDistricts = Districts.Where((c) => { return c.CurrentMajority == Constituent.Party.Blue; });

        int redCount = redDistricts.Count();
        int blueCount = blueDistricts.Count();

        while (redCount != blueCount)
        {
            //find out which one we want to switch
            District districtToFlip;
            Constituent.Party originalParty, targetParty;

            //how many votes we need to sway in the other party's favor
            int amountToFlip;

            if (redCount > blueCount)
            {
                originalParty = Constituent.Party.Red;
                targetParty = Constituent.Party.Blue;
                districtToFlip = Utils.ChooseRandom(redDistricts.ToList());

                //enough votes to make it even, plus enough votes to give blue a small margin
                amountToFlip = (districtToFlip.VotesRed - districtToFlip.VotesBlue) / 2 + Random.Range(1, 4);
            }
            else
            {
                originalParty = Constituent.Party.Blue;
                targetParty = Constituent.Party.Red;
                districtToFlip = Utils.ChooseRandom(blueDistricts.ToList());

                //enough votes to make it even, plus enough votes to give red  a small margin
                amountToFlip = (districtToFlip.VotesBlue - districtToFlip.VotesRed) / 2 + Random.Range(1, 4);
            }

            //swap random consitutents in this district until it flips
            int numFlipped = 0;
            while (numFlipped < amountToFlip)
            {
                Constituent districtConstituent = districtToFlip.ConstituentsQuery.First((c) => c.party == originalParty);

                //select a random district from ALL other districts, and choose a random constituent in the target party
                District otherDistrict = Utils.ChooseRandom(Districts);
                if (otherDistrict == districtToFlip)
                    continue;
                Constituent otherConstituent = otherDistrict.ConstituentsQuery.FirstOrDefault((c) => c.party == targetParty);
                if (otherConstituent == null)
                    continue;

                //swap their parties
                districtConstituent.party = targetParty;
                otherConstituent.party = originalParty;

                //update the other district's member data
                otherDistrict.UpdateMemberData();

                numFlipped++;
            }

            //update district member data
            districtToFlip.UpdateMemberData();

            //update the counts after the change
            redCount = redDistricts.Count();
            blueCount = blueDistricts.Count();
        }
    }

    private static List<List<Constituent>> PartitionConstituents(List<Constituent> constituents, int numPartitions)
    {
        var result = new List<List<Constituent>>();
        if (numPartitions <= 1)
        {
            result.Add(constituents);
            return result;
        }

        //we don't want to consider empty cells
        var populatedConstituents = constituents.Where((c) => { return c.party != Constituent.Party.None; });

        //choose which axis to use, x or y
        float maxX = populatedConstituents.Max((c) => { return c.transform.position.x; });
        float minX = populatedConstituents.Min((c) => { return c.transform.position.x; });
        float maxY = populatedConstituents.Max((c) => { return c.transform.position.y; });
        float minY = populatedConstituents.Min((c) => { return c.transform.position.y; });

        float dx = maxX - minX;
        float dy = maxY - minY;

        //we want to bisect the "shorter" axis, so that we don't get long and skinny districts
        Vector2 axis;
        if (dx < dy)
        {
            //there's a higher variation in the x's than in the y's, so split along the y axis
            axis = new Vector3(0.0f, 1.0f, 0.0f);
        }
        else
        {
            //there's a higher variation in the y's than in the x's, so split along the x axis
            axis = new Vector3(1.0f, 0.0f, 0.0f);
        }

        //rotate the axis by a tiny bit, since the points are on a grid
        float maxRandom = 10.0f;
        float randomAngle = (Random.value - 0.5f) * maxRandom;
        axis = Quaternion.AngleAxis(randomAngle, Vector3.forward) * axis;

        //project every point onto that axis
        List<float> pointProjection = new List<float>();
        foreach (var c in populatedConstituents)
        {
            pointProjection.Add(Vector3.Dot(axis, c.transform.position));
        }
        pointProjection.Sort();

        //choose a pivot point based on numPartitions
        int smallerPartition = numPartitions / 2;
        float partitionRatio = ((float)smallerPartition) / numPartitions;
        int pivotIndex = (int)(partitionRatio * pointProjection.Count);

        float pivot = pointProjection[pivotIndex];

        //sort every constituent (even blank ones) below that pivot into one partition, above into another
        List<Constituent> belowPivot = new List<Constituent>();
        List<Constituent> abovePivot = new List<Constituent>();

        foreach (var c in constituents)
        {
            float projection = Vector3.Dot(axis, c.transform.position);
            if (projection < pivot)
            {
                belowPivot.Add(c);
            }
            else
            {
                abovePivot.Add(c);
            }
        }

        //recursively partition
        result.AddRange(PartitionConstituents(belowPivot, smallerPartition));
        result.AddRange(PartitionConstituents(abovePivot, numPartitions - smallerPartition));

        return result;
    }

    private struct Point
    {
        public int x, y;

        public Point(int _x, int _y) { x = _x; y = _y; }
        public static Point operator +(Point lhs, Point rhs)
        {
            return new Point(lhs.x + rhs.x, lhs.y + rhs.y);
        }
    }
}
