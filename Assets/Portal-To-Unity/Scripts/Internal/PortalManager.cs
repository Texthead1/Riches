using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PortalToUnity
{
    public static class PortalDatabase
    {
        private static List<PortalInfo> Portals = new List<PortalInfo>();
        
        public static void Initialize()
        {
            Portals.Clear();
            Portals = Resources.LoadAll<PortalInfo>("Portal-To-Unity/Portals").ToList();
        }

        public static PortalInfo PortalFromID(byte[] id) => Portals.FirstOrDefault(x => x.ID.SequenceEqual(id));

        public static string NameFromID(byte[] id) => IsRegisteredPortal(id) ? PortalFromID(id).Name : "Portal of Power";

        public static bool IsRegisteredPortal(byte[] id) => Portals.Count > 0 && Portals.Any(x => x.ID.SequenceEqual(id));
    }
}