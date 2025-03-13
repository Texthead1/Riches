using UnityEngine;

namespace PortalToUnity
{
    public enum LEDType
    {
        None = -1,
        FullColor = 0,
        Enhanced = 1,
        BlueOnly = 3
    }
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NewPortalInfo", menuName = "Portal-To-Unity/Portal Info", order = 60)]
#endif
    public class PortalInfo : ScriptableObject
    {
        public string Name;
        public byte[] ID;
        public LEDType LEDType;
        public char[] SupportedCommands;
    }
}