using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour {

    private struct Point
    {
        public int x, y;

        public Point(int _x, int _y) { x = _x; y = _y; }
        public static Point operator +(Point lhs, Point rhs) {
            return new Point(lhs.x + rhs.x, lhs.y + rhs.y);
        }
    }

    public int width, height;
    public int numDistricts;
    public float populationFalloffDistance;

    public GameObject constituentPrefab;
    public GameObject districtPrefab;

    public List<District> Districts { get; private set; }
    public List<Constituent> Constituents { get; private set; }

    private int minDistrictSize, maxDistrictSize;

	void Awake () {
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
                locationDict.Add(new Point(x,y), constituent);
                Constituents.Add(constituent);
            }
        }
        
        //assign neighbors to each constistuent
        foreach (Point p in locationDict.Keys)
        {
            Constituent c = locationDict[p];
            locationDict.TryGetValue(p + new Point( 0, 1), out c.neighborTop);
            locationDict.TryGetValue(p + new Point( 0,-1), out c.neighborBottom);
            locationDict.TryGetValue(p + new Point(-1, 0), out c.neighborLeft);
            locationDict.TryGetValue(p + new Point( 1, 0), out c.neighborRight);
        }

        //partition the constituents into districts
        Districts = new List<District>();
        var partitionResults = PartitionConstituents(locationDict.Values.ToList(), numDistricts);
        int i = 0;
        foreach (var partition in partitionResults)
        {
            District d = MakeDistrict((++i).ToString());
            Districts.Add(d);

            foreach (var c in partition)
            {
                c.district = d;
            }
        }

        //go through the partitions again and make sure there are no disconnected pieces
        foreach (var partition in partitionResults)
        {
            var components = Utils.ConnectedComponents(partition, (c) => {
                return c.Neighbors.Where((n) => { return n != null && c.district == n.district; });
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
                        District newDistrict = c.district;
                        while (Object.ReferenceEquals(newDistrict, c.district))
                        {
                            newDistrict = Utils.ChooseRandom(c.Neighbors.Where((n) => { return n != null; }).ToList()).district;
                        }
                        c.district = newDistrict;
                    }
                }
            }
        }

        foreach (District d in Districts)
        {
			d.UpdateMemberData();
        }

		foreach (Constituent c in Constituents) {
			c.UpdateBorders();
		}

        float averageDistrictSize = (float)Constituents.Count((c) => { return c.party != Constituent.Party.None; }) / numDistricts;
        minDistrictSize = (int)System.Math.Round(averageDistrictSize * 0.75f);
        maxDistrictSize = (int)System.Math.Round(averageDistrictSize * 1.25f);
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
		pointProjection.Sort ();

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

    public bool IsValidMove(Constituent constituent, District newDistrict)
    {
		if (constituent.party != Constituent.Party.None)
		{
			//make sure the size of the old district will be within size constraints
			if (constituent.district.VotingMemberCount - 1 < minDistrictSize)
			{
				return false;
			}

			//make sure the size of the new district will be within constraints
            if (newDistrict.VotingMemberCount + 1 > maxDistrictSize)
			{
				return false;
			}
		}

        //check if this constituent is a cut vertex for the old district. if it is, return false
        if (constituent.district.ArticulationPoints.Contains(constituent))
        {
            return false;
        }

        //verify that this constituent is actually adjacent to the new district
		if(!newDistrict.NeighborConstituents.Contains(constituent))
		{
            return false;
        }

        //return true
        return true;
    }
}
