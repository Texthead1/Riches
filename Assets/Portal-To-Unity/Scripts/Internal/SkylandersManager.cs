using System.Collections.Generic;
using UnityEngine;

namespace PortalToUnity
{
    public static class SkylanderDatabase
    {
        private static Dictionary<ToyCode, Skylander> Skylanders = new Dictionary<ToyCode, Skylander>();

        public static void Initialize()
        {
            Skylanders.Clear();
            Skylander[] skylanders = Resources.LoadAll<Skylander>("Portal-To-Unity/Figures");
            foreach (Skylander skylander in skylanders)
                AddSkylander(skylander);
        }

        public static void AddSkylander(Skylander skylander)
        {
            if (!Skylanders.ContainsKey(skylander.CharacterID))
                Skylanders.Add(skylander.CharacterID, skylander);
            else
                PTUManager.LogWarning($"Skylander with ToyCode {skylander.CharacterID} already exists in the database", LogPriority.Low);
        }

        public static bool GetSkylander(ToyCode characterID, out Skylander skylander)
        {
            if (Skylanders.TryGetValue(characterID, out skylander))
                return true;

            PTUManager.LogWarning($"Skylander with ToyCode {characterID} not found in the database", LogPriority.Low);
            return false;
        }

        public static IEnumerable<Skylander> GetAllSkylanders() => Skylanders.Values;
    }

    // Common subtext names used by Skylanders
    internal static class Tags
    {
        internal const string S = "Special";                        // For registered chase variants
        internal const string R = "Rare";                           // For certain registered chase variants
        internal const string V = "Variant";                        // Normally this would be "Special" in VV engine and blank in TFB
        internal const string S1 = "Series 1";
        internal const string S2 = "Series 2";
        internal const string S3 = "Series 3";
        internal const string S4 = "Series 4";                      // Used exclusively by Tidal Wave Gill Grunt
        internal const string LC = "LightCore";
        internal const string EE = "Eon's Elite";
        internal const string EV = "Exclusive Event Edition";       // Used exclusively by E3 Hot Streak
        internal const string VV = "Vicarious Visions";             // Used exclusively by (Gear Head) VVind-Up
        internal static bool IsReposeTag(string tag) => tag == S2 || tag == S3 || tag == S4;
        internal static bool IsSpecialTag(string tag) => tag == S || tag == R;
    }
}