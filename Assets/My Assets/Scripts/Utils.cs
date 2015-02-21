using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Utils : MonoBehaviour {

    public static T ChooseRandom<T>(List<T> items)
    {
        int index = Random.Range(0, items.Count);
        return items[index];
    }

    public static T ChooseRandom<T>(T[] items)
    {
        int index = Random.Range(0, items.Length);
        return items[index];
    }

    public static T ParseEnumString<T>(string enumString)
    {
        return (T)System.Enum.Parse(typeof(T), enumString);
    }


    public static T ChooseWeightedRandom<T>(SortedList<T, float> items)
    {

        List<float> cumulativeWeights = new List<float>();
        foreach (var item in items)
        {
            cumulativeWeights.Add(cumulativeWeights.LastOrDefault() + item.Value);
        }

        //currentSum now contains the max value
        float randomValue = Random.value * cumulativeWeights.Last();

        int index = cumulativeWeights.BinarySearch(randomValue);
        if (index < 0) index = ~index;

        return items.Keys[index];
    }

	public static List<HashSet<T>> ConnectedComponents<T>(IEnumerable<T> items, System.Func<T, IEnumerable<T>> neighborFunc)
    {
        HashSet<T> remainingSet = new HashSet<T>(items);

		List<HashSet<T>> result = new List<HashSet<T>>();

        while (remainingSet.Count > 0)
        {
            Queue<T> openSet = new Queue<T>(remainingSet.Take(1));
            HashSet<T> closedSet = new HashSet<T>();

            while (openSet.Count > 0)
            {
                T currentItem = openSet.Dequeue();
                if (!closedSet.Contains(currentItem))
                {
                    closedSet.Add(currentItem);

                    foreach (T neighbor in neighborFunc(currentItem))
                    {
                        if (!closedSet.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor);
                        }
                    }
                }
            }

            result.Add(closedSet);

            foreach (T item in closedSet)
            {
                remainingSet.Remove(item);
            }

        }
        return result;
    }

	private class CutVertexSearchData<T> where T: Object
	{
		public T parent;
		public int num;
		public int low;

		public CutVertexSearchData(T _parent, int initialNum) 
		{
			parent = _parent;
			num = initialNum;
			low = initialNum;
		}
	}

	public static HashSet<T> FindArticulationPoints<T>(T rootVertex, System.Func<T, IEnumerable<T>> neighborFunc) where T: Object
	{
		//use the algorithm found here http://www.eecs.wsu.edu/~holder/courses/CptS223/spr08/slides/graphapps.pdf
		//returns a list of all vertices that would disconnect the graph if they were removed

		Dictionary<T, CutVertexSearchData<T>> searchData = new Dictionary<T, CutVertexSearchData<T>> ();
		HashSet<T> results = new HashSet<T> ();

		int counter = 0;

		//we declare it to be null first so that it exist in local scope, so that it can call itself recursively
		System.Action<T, T> FindArticulationPoints = null;
		FindArticulationPoints = (vertex, parent) =>
		{
			searchData.Add (vertex, new CutVertexSearchData<T>(parent, counter++));

			//go through every neighbor if this vertex
			foreach(T neighbor in neighborFunc(vertex))
			{
				//if this neighbor has not already been visited, then it's a forward edge
				if(!searchData.ContainsKey(neighbor))
				{
					//recursively run the algorithm on this neighbor
					FindArticulationPoints(neighbor, vertex);

					if(searchData[neighbor].low >= searchData[vertex].num)
					{
						//this vertex is an articulation point
						results.Add (vertex);
					}
					searchData[vertex].low = System.Math.Min(searchData[vertex].low, searchData[neighbor].low);
				}
				else if(parent != neighbor) //this neighbor has already been visited and isn't the parent, so this is a back-edge
				{
					searchData[vertex].low = System.Math.Min(searchData[vertex].low, searchData[neighbor].num);
				}
			}
		};

		//run the algorithm to find all articulation points, starting with the provided root
		FindArticulationPoints (rootVertex, null);

		//"results" now contains all articulation points with one exception - it contains the root
		//we need to check if the root has only one child. if it does, it's not an articulation point

		//we're not using linq here because of a mono bug
		//if (searchData.Values.Count ((entry) => { return entry.parent == rootVertex; }) > 1)
		int rootChildren = 0;
		foreach(var item in searchData.Values)
		{
			if(item.parent == rootVertex)
				rootChildren++;
		}
		if(rootChildren == 1)
			results.Remove (rootVertex);

		return results;
	}
}
