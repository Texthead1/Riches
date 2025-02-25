using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static PortalToUnity.Global;

namespace PortalToUnity
{
    public class DigitalPortal : PortalOfPower
    {
        public override bool IsDigital => true;

        public DigitalPortal() : base()
        {
        }

        public override bool PushTransfer(byte[] data, byte length = 0)
        {
            return DigitalPortalManager.WritePortalPipe.TransferWrite(data);
        }

        public override bool WriteRaw(byte[] data) => false;

        public override void StartReading()
        {
        }

        public override void Destroy()
        {
            Instances.Remove(this);
        }
    }

    public static class DigitalPortalManager
    {
        public static InterProcessConnection ReadPortalPipe { get; private set; }
        public static InterProcessConnection WritePortalPipe { get; private set; }

        public static DigitalPortal DigitalPortal;

        public static async Task Initialize()
        {
            ReadPortalPipe = new InterProcessConnection("DPW_AR");
            WritePortalPipe = new InterProcessConnection("DPR_AW");

            ReadPortalPipe.OnConnected += Connected;
            ReadPortalPipe.OnDisconnected += Disconnected;
            ReadPortalPipe.OnResponse += ReportRecieved;
            ReadPortalPipe.Connect();
            WritePortalPipe.Connect();

#if UNITY_EDITOR
            EditorApplication.quitting += Quitting;
#endif
            Application.quitting += Quitting;
            
            await ReestablishPipes();
        }

        private static void Connected()
        {
            PortalOfPower portal = new DigitalPortal();
            DigitalPortal = (DigitalPortal)portal;
            PortalOfPower.OnAdded(portal);
        }

        private static void Disconnected()
        {
            Debug.Log("Digital Portal removed");
            if (DigitalPortal != null)
            {
                PortalOfPower.OnRemoved(DigitalPortal);
                DigitalPortal?.Destroy();
                DigitalPortal = null;
            }
        }

        private static void ReportRecieved(byte[] data)
        {
            DigitalPortal?.ReportRecieved(data);
        }

        private static void Quitting()
        {
            //if (DigitalPortal != null)
            //    Disconnected();
            ReadPortalPipe.Stop();
            WritePortalPipe.Stop();
        }

        private static async Task ReestablishPipes()
        {
            while (true)
            {
                await Task.Delay(200);

                if (!Application.isPlaying) return;
                if (DigitalPortal != null) continue;

                ReadPortalPipe.OnConnected -= Connected;
                ReadPortalPipe.OnDisconnected -= Disconnected;
                ReadPortalPipe.OnResponse -= ReportRecieved;
                ReadPortalPipe.Stop();
                WritePortalPipe.Stop();

                ReadPortalPipe = new InterProcessConnection("DPW_AR");
                WritePortalPipe = new InterProcessConnection("DPR_AW");

                ReadPortalPipe.OnConnected += Connected;
                ReadPortalPipe.OnDisconnected += Disconnected;
                ReadPortalPipe.OnResponse += ReportRecieved;
                ReadPortalPipe.Connect();
                WritePortalPipe.Connect();
            }
        }
    }
}