using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libusbK;
using UnityEditor;
using UnityEngine;

namespace PortalToUnity
{
    public class libusbKConnection : MonoBehaviour
    {
        private HotK hotK;
        private KHOT_PARAMS khotParams = new KHOT_PARAMS();
        private static List<PortalOfPower> connectedPortals = new List<PortalOfPower>();

        public void Start()
        {
            khotParams.PatternMatch.DeviceInterfaceGUID = "*";
            khotParams.Flags = KHOT_FLAG.PLUG_ALL_ON_INIT;
            khotParams.OnHotPlug = PortalPlugStateChanged;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += EditorStateResponse;
#endif
            hotK = new HotK(ref khotParams);
        }

        private async void PortalPlugStateChanged(KHOT_HANDLE hotHandle, KLST_DEVINFO_HANDLE deviceInfo, KLST_SYNC_FLAG plugType)
        {
            if (!deviceInfo.IsPortalOfPower()) return;

            switch (plugType)
            {
                case KLST_SYNC_FLAG.ADDED:
                    await PortalAdded(deviceInfo);
                    break;

                case KLST_SYNC_FLAG.REMOVED:
                    PortalRemoved(deviceInfo);
                    break;
            }
        }

        private async Task PortalAdded(KLST_DEVINFO_HANDLE device)
        {
            PortalOfPower portal = new PortalOfPower(device);
            connectedPortals.Add(portal);
            PortalOfPower.OnRawAdded?.Invoke(portal);
            portal.StartReading();
            await portal.SetUpPortal();
        }

        private void PortalRemoved(KLST_DEVINFO_HANDLE device)
        {
            PortalOfPower portal = connectedPortals.FirstOrDefault(portal => portal.kHandle.Equals(device));
            portal.StopReading();
            bool exists = portal.Destroy();
            connectedPortals.Remove(portal);
            PortalOfPower.OnRawRemoved?.Invoke(portal);
            if (exists)
                PortalOfPower.OnRemoved?.Invoke(portal);
        }

#if UNITY_EDITOR
        public void EditorStateResponse(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;

            List<PortalOfPower> portals = new List<PortalOfPower>(connectedPortals);
            
            for (int i = 0; i < connectedPortals.Count; i++)
                PortalRemoved(portals[i].kHandle);
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorStateResponse;
        }
#endif
    }
}