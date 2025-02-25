using UnityEngine;

namespace PortalToUnity
{
    internal class PTUManager : MonoBehaviour
    {
        void Awake()
        {
            SkylanderDatabase.Initialize();
            PortalDatabase.Initialize();
            Cryptography.CheckForSalt();
        }

        async void Start()
        {
            await DigitalPortalManager.Initialize();
        }
    }
}