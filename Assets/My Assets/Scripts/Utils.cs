using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Utils : MonoBehaviour {

    public static T ChooseRandom<T>(List<T> items)
    {
        int index = Random.Range(0, items.Count);
        return items[index];
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

    public static T ChooseRandom<T>(T[] items)
    {
        int index = Random.Range(0, items.Length);
        return items[index];
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
}
