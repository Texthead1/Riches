using UnityEngine;

namespace PortalToUnity
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewPortalInfo", menuName = "Portal-To-Unity/Portal Info", order = 90)]
#endif
    public class PortalInfo : ScriptableObject
    {
        public string Name;
        public byte[] ID;
        public char[] SupportedCommands;
    }
}