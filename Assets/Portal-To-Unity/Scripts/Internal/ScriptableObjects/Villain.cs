using UnityEngine;

namespace PortalToUnity
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewVillain", menuName = "Portal-To-Unity/Villain", order = 40)]
#endif
    public class Villain : ScriptableObject
    {
        public string Name;
        public VillainID VillainID;
        public Element Element;
        public VillainVariant Variant;
    }
}