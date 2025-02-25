using System;
using System.Collections.Generic;
using UnityEngine;

namespace PortalToUnity
{
    public enum Element
    {
        None = 0,
        Earth = 1,
        Water = 2,
        Air = 3,
        Fire = 4,
        Life = 5,
        Undead = 6,
        Magic = 7,
        Tech = 8,
        Light = 9,
        Dark = 10,
        Kaos = 11
    }

    public enum SkyType
    {
        Skylander,
        Giant,
        SWAPForceBottom,
        SWAPForceTop,
        TrapMaster,
        SuperCharger,
        Sensei,
        MagicItem,
        ExpansionPack,
        BattlePiece,
        Mini,
        Trap,
        Vehicle,
        RacingPack,
        VillainDriver,
        VillainVehicle,
        CreationCrystal,
        Imaginator
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

    [Serializable]
    public struct VariantID
    {
        public SkylandersGame YearCode;
        public bool IsRepose, IsAltDeco, IsLightCore, IsSuperCharger;
        public int DecoID;

        public VariantID(ushort variantID)
        {
            YearCode = (SkylandersGame)((variantID >> 0xC) & 0xF);
            IsRepose = ((variantID >> 0xB) & 0x1) != 0;
            IsAltDeco = ((variantID >> 0xA) & 0x1) != 0;
            IsLightCore = ((variantID >> 0x9) & 0x1) != 0;
            IsSuperCharger = ((variantID >> 0x8) & 0x1) != 0;
            DecoID = (int)(DecoID)(variantID & 0xFF);
        }

        public VariantID(SkylandersGame yearCode, bool isRepose, bool isAltDeco, bool isLightCore, bool isSuperCharger, byte decoID)
        {
            YearCode = yearCode;
            IsRepose = isRepose;
            IsAltDeco = isAltDeco;
            IsLightCore = isLightCore;
            IsSuperCharger = isSuperCharger;
            DecoID = (int)(DecoID)decoID;
        }

        public readonly ushort Encode() => (ushort)((byte)DecoID | ((IsSuperCharger ? 1 : 0) << 0x8) | ((IsLightCore ? 1 : 0) << 0x9) | ((IsAltDeco ? 1 : 0) << 0xA) | ((IsRepose ? 1 : 0) << 0xB) | ((int)YearCode << 0xC));

        public override readonly string ToString() => $"YearCode: {YearCode}, IsRepose: {IsRepose}, IsAltDeco: {IsAltDeco}, IsLightCore: {IsLightCore}, IsSuperCharger: {IsSuperCharger}, DecoID: {(int)DecoID}, VariantShort: {Encode()}";
    }

    public static class SkylanderDatabase
    {
        private static Dictionary<ToyCode, Skylander> Skylanders = new Dictionary<ToyCode, Skylander>();

        public static void Initialize()
        {
            Skylanders.Clear();
            Skylander[] skylanders = Resources.LoadAll<Skylander>("Portal-To-Unity/Figures");
            foreach (var skylander in skylanders)
                AddSkylander(skylander);
        }

        public static void AddSkylander(Skylander skylander)
        {
            if (!Skylanders.ContainsKey(skylander.CharacterID))
                Skylanders.Add(skylander.CharacterID, skylander);
            else
                Debug.LogWarning($"Skylander with ToyCode {skylander.CharacterID} already exists in the database");
        }

        public static Skylander GetSkylander(ToyCode characterID)
        {
            if (Skylanders.TryGetValue(characterID, out Skylander skylander))
                return skylander;

            Debug.LogWarning($"Skylander with ToyCode {characterID} not found in the database");
            return null;
        }

        public static IEnumerable<Skylander> GetAllSkylanders() => Skylanders.Values;
    }
}