using System.Collections.Generic;
using UnityEngine;

namespace PortalToUnity
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewSkylander", menuName = "Portal-To-Unity/Skylander", order = 70)]
#endif
    public class Skylander : ScriptableObject
    {
        public string Name;
        public ToyCode CharacterID;
        public SkyType Type;
        public Element Element;
        public string Prefix;
        public List<SkylanderVariant> Variants;
    }
}