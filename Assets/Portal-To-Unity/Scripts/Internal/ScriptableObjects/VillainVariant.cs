using UnityEngine;

namespace PortalToUnity
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewVillainVariant", menuName = "Portal-To-Unity/Villain Variant", order = 41)]
#endif
    public class VillainVariant : ScriptableObject
    {
        public string Name;
        public string NameOverride;
    }
}