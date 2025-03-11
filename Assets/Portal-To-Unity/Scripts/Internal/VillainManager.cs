using System.Collections.Generic;
using UnityEngine;

namespace PortalToUnity
{
    public static class VillainDatabase
    {
        private static Dictionary<VillainID, Villain> Villains = new Dictionary<VillainID, Villain>();

        public static void Initialize()
        {
            Villains.Clear();
            Villain[] villains = Resources.LoadAll<Villain>("Portal-To-Unity/Villains");
            foreach (Villain villain in villains)
                AddVillain(villain);
        }

        public static void AddVillain(Villain villain)
        {
            if (!Villains.ContainsKey(villain.VillainID))
                Villains.Add(villain.VillainID, villain);
            else
                PTUManager.LogWarning($"Villain with ID {villain.VillainID} already exists in the database", LogPriority.Low);
        }

        public static bool GetVillain(VillainID villainID, out Villain villain)
        {
            if (Villains.TryGetValue(villainID, out villain))
                return true;

            PTUManager.LogWarning($"Villain with ID {villainID} not found in the database", LogPriority.Low);
            return false;
        }

        public static IEnumerable<Villain> GetAllVillains() => Villains.Values;
    }
}