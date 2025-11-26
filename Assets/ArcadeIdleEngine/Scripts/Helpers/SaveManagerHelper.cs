using System.Collections.Generic;
using System.Linq;
using ArcadeBridge.ArcadeIdleEngine.Data;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;

namespace ArcadeBridge.ArcadeIdleEngine.Helpers
{
	public static class SaveManagerHelper
	{
		public static List<Saveable> FindDuplicates(List<Saveable> inputList)
		{
			// Create a dictionary to store the count of each saveId
			Dictionary<string, int> countMap = new Dictionary<string, int>();

			// Iterate through the input list and count occurrences of each saveId
			foreach (Saveable saveable in inputList)
			{
				if (countMap.ContainsKey(saveable.SaveId))
				{
					// If saveId already exists in dictionary, increment count
					countMap[saveable.SaveId]++;
				}
				else
				{
					// If saveId is encountered for the first time, add it to dictionary with count 1
					countMap[saveable.SaveId] = 1;
				}
			}

			// Filter dictionary to include only Saveables with count > 1 (i.e., duplicates)
			List<Saveable> duplicates = inputList.Where(saveable => countMap[saveable.SaveId] > 1).ToList();

			return duplicates;
		}
	}
}
