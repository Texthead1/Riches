using System;
using System.Collections.Generic;
using libusbK;
using UnityEditor;
using UnityEngine;

namespace PortalToUnity
{
    public class libusbkConnection : MonoBehaviour
    {
        private HotK hotK;
        private KHOT_PARAMS khotParams = new KHOT_PARAMS();

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

        private void PortalPlugStateChanged(KHOT_HANDLE hotHandle, KLST_DEVINFO_HANDLE deviceInfo, KLST_SYNC_FLAG plugType)
        {
            if (!deviceInfo.IsPortalOfPower()) return;

            switch (plugType)
            {
                case KLST_SYNC_FLAG.ADDED:
                    PortalAdded(deviceInfo);
                    break;

                case KLST_SYNC_FLAG.REMOVED:
                    PortalRemoved(deviceInfo);
                    break;
            }
        }

        private void PortalAdded(KLST_DEVINFO_HANDLE device)
        {
            PortalOfPower portal = new PortalOfPower(device);
            portal.StartReading();
            PortalOfPower.OnAdded?.Invoke(portal);
        }

        private void PortalRemoved(KLST_DEVINFO_HANDLE device)
        {
            PortalOfPower portal = PortalOfPower.WithHandle(device);
            portal.StopReading();
            PortalOfPower.OnRemoved?.Invoke(portal);
            portal.Destroy();
        }

#if UNITY_EDITOR
        public void EditorStateResponse(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;

            var portals = new List<PortalOfPower>(PortalOfPower.Instances);
            
            for (int i = 0; i < PortalOfPower.Instances.Count; i++)
                PortalRemoved(portals[i].kHandle);
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorStateResponse;
        }
#endif
    }
}