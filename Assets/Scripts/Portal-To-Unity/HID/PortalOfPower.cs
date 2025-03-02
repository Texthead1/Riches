using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using libusbK;
using UnityEngine;
using static PortalToUnity.Global;

namespace PortalToUnity
{
    public enum PortalLED
    {
        Left = 0x00,
        Trap = 0x01,
        Right = 0x02
    }

    public enum PortalState
    {
        JustAdded,
        SetUpForInterface,
        CommunicatingWithAntenna,
        Standby,
        Ready
    }

    public struct QueryReturnInfo
    {
        public FigureBlockQueryState ReadState;
        public byte[] Data;

        public QueryReturnInfo(FigureBlockQueryState readState, byte[] data)
        {
            ReadState = readState;
            Data = data;
        }
    }

    public class PortalOfPower
    {
        public static Action<PortalOfPower> OnAdded;
        public static Action<PortalOfPower> OnRemoved;

        public static List<PortalOfPower> Instances = new List<PortalOfPower>();
        public static PortalOfPower WithHandle(KLST_DEVINFO_HANDLE handle) => Instances.FirstOrDefault(portal => portal.kHandle.Equals(handle));

        public UsbK kDevice;
        public KLST_DEVINFO_HANDLE kHandle;

        public byte[] ID { get; private set; }
        public bool Active { get; private set; }
        public bool IsAwake { get; private set; }
        public virtual bool IsDigital => false;

        public PortalFigure[] Figures { get; private set; } = new PortalFigure[FIGURE_INDICIES_COUNT];

        public Action<PortalOfPower, byte[]> OnInputReport;
        public Action<PortalFigure> OnFigureAdded;
        public Action<PortalFigure> OnFigureRemoved;
        public Action<PortalFigure> OnFinishedReadingFigure;

        public List<PortalFigure> FiguresInQueue = new List<PortalFigure>();
        public PortalFigure currentlyQueryingFigure;
        public bool readFigures = false;
        public PortalState State = PortalState.JustAdded;
        public CancellationTokenSource bootCTS;

        protected PortalOfPower()
        {
            for (byte i = 0; i < FIGURE_INDICIES_COUNT; i++)
                Figures[i] = new PortalFigure(this, i);
            Instances.Add(this);
        }

        public PortalOfPower(KLST_DEVINFO_HANDLE handle)
        {
            kDevice = new UsbK(handle);
            kHandle = handle;

            for (byte i = 0; i < FIGURE_INDICIES_COUNT; i++)
                Figures[i] = new PortalFigure(this, i);

            Instances.Add(this);
        }

        private byte[] ConstructCommand(char commandChar, params byte[] commandArgs)
        {
            byte[] command = new byte[REPORT_SIZE];
            command[0] = (byte)commandChar;
            Array.Copy(commandArgs, 0, command, 1, commandArgs.Length);
            return command;
        }

        public void COMMAND_SetAntenna(bool active)
        {
            byte[] command = ConstructCommand(
                'A',
                (byte)(active ? 0x01 : 0x00)
            );

            PushTransfer(command, 2);
        }

        public void COMMAND_RestoreBasicColorCycle()
        {
            byte[] command = ConstructCommand('B');
            PushTransfer(command, 1);
        }

        public void COMMAND_SetLEDColor(byte r, byte g, byte b)
        {
            byte[] command = ConstructCommand(
                'C',
                r,
                g,
                b
            );

            PushTransfer(command, 4);
        }

        public void COMMAND_SetLEDColor(Color32 color) => COMMAND_SetLEDColor(color.r, color.g, color.b);

        public void COMMAND_SetTraptaniumLEDColor(PortalLED side, byte r, byte g, byte b, short transitionTime)
        {
            byte[] command = ConstructCommand(
                'J',
                (byte)side,
                r,
                g,
                b,
                (byte)(transitionTime & 0xFF),
                (byte)(transitionTime >> 0x08)
            );

            PushTransfer(command, 7);
        }

        public void COMMAND_SetTraptaniumLEDColor(PortalLED side, Color32 color, short transitionTime) => COMMAND_SetTraptaniumLEDColor(side, color.r, color.g, color.b, transitionTime);

        public void COMMAND_SetTraptaniumLight(PortalLED led, byte r, byte g = 0, byte b = 0, byte unknown = 0)
        {
            byte[] command = ConstructCommand(
                'L',
                (byte)led,
                r,
                g,
                b,
                unknown
            );

            PushTransfer(command, 6);
        }

        public void COMMAND_SetTraptaniumLight(PortalLED led, Color32 color, byte unknown = 0) => COMMAND_SetTraptaniumLight(led, color.r, color.g, color.b, unknown);

        public void COMMAND_SetTraptaniumSpeaker(bool active)
        {
            byte[] command = ConstructCommand(
                'M',
                (byte)(active ? 0x01 : 0x00)
            );

            PushTransfer(command, 2);
        }

        public void COMMAND_FigureQuery(byte index, byte block)
        {
            byte[] command = ConstructCommand(
                'Q',
                index,
                block
            );

            PushTransfer(command, 3);
        }

        public void COMMAND_ResetPortal()
        {
            byte[] command = ConstructCommand('R');
            PushTransfer(command, 1);
        }

        public void COMMAND_RequestStatus()
        {
            byte[] command = ConstructCommand('S');
            PushTransfer(command, 1);
        }

        public void COMMAND_SetLightAudioVibrancyTolerance(byte tolerance, byte unk0 = 0, byte unk1 = 0)
        {
            byte[] command = ConstructCommand(
                'V',
                unk0,
                unk1,
                tolerance
            );

            PushTransfer(command, 4);
        }

        public void COMMAND_FigureWrite(byte index, byte block, byte[] data)
        {
            if (data.Length != BLOCK_SIZE)
                throw new ArgumentException("Data array must be 16 bytes long.");

            byte[] command = ConstructCommand(
                'W',
                index,
                block
            );
            Array.Copy(data, 0, command, 3, 0x10);

            PushTransfer(command, 19);
        }

        public void RequestWirelessDongleSerial()
        {
            byte[] command = ConstructCommand((char)0xFA);

            PushTransfer(command, 1);
        }

        public async Task<QueryReturnInfo> QueryFigureAsync(byte index, byte block)
        {
            bool askForQuery = true;
            TaskCompletionSource<QueryReturnInfo> tcs = new TaskCompletionSource<QueryReturnInfo>();
            byte retryCount = 0;
            byte timeout = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            async void reportHandler(PortalOfPower portal, byte[] returnedData)
            {
                if (!Figures[index].IsPresent() || !Instances.Contains(this))
                {
                    tcs.SetResult(new QueryReturnInfo(FigureBlockQueryState.FigureRemoved, new byte[0x10]));
                    OnInputReport -= reportHandler;
                    cts.Cancel();
                    return;
                }

                if (askForQuery)
                {
                    if (returnedData[0] == (byte)'Q')
                    {
                        if ((returnedData[1] & 0x10) != 0 && (returnedData[1] & 0xF) == index && returnedData[2] == block)
                        {
                            byte[] result = new byte[BLOCK_SIZE];
                            Array.Copy(returnedData, 3, result, 0, BLOCK_SIZE);
                            tcs.SetResult(new QueryReturnInfo(FigureBlockQueryState.Success, result));
                            OnInputReport -= reportHandler;
                            cts.Cancel();
                            return;
                        }
                        if (retryCount >= 5)
                        {
                            tcs.SetResult(new QueryReturnInfo(FigureBlockQueryState.Error, new byte[0x10]));
                            OnInputReport -= reportHandler;
                            cts.Cancel();
                        }
                        retryCount++;
                        await Task.Delay(80);
                        COMMAND_RequestStatus();
                        askForQuery = false;
                    }
                    return;
                }

                if (returnedData[0] == (byte)'S')
                {
                    ulong figurePresences = (ulong)(returnedData[1] | returnedData[2] << 0x08 | returnedData[3] << 0x10 | returnedData[4] << 0x18);
                    FigurePresence figurePresence = (FigurePresence)((figurePresences >> (index * 2)) & 0b11);

                    if (figurePresence == FigurePresence.Present || figurePresence == FigurePresence.JustArrived)
                    {
                        askForQuery = true;
                        await Task.Delay(80);
                        COMMAND_FigureQuery(index, block);
                    }
                    else
                    {
                        tcs.SetResult(new QueryReturnInfo(FigureBlockQueryState.FigureRemoved, new byte[0x10]));
                        OnInputReport -= reportHandler;
                        cts.Cancel();
                        return;
                    }
                }
            }

            OnInputReport += reportHandler;

            while (!tcs.Task.IsCompleted && timeout < 4 && Instances.Contains(this) && Figures[index].IsPresent())
            {
                COMMAND_FigureQuery(index, block);
                try
                {
                    await Task.Delay(350, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                timeout++;
            }

            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult(new QueryReturnInfo(FigureBlockQueryState.Error, new byte[0x10]));
                OnInputReport -= reportHandler;
            }
            return await tcs.Task;
        }

        public async Task<bool> WriteFigureAsync(byte index, byte block, byte[] data)
        {
            bool askForQuery = true;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            byte retryCount = 0;
            byte timeout = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            async void reportHandler(PortalOfPower portal, byte[] returnedData)
            {
                if (!Figures[index].IsPresent() || !Instances.Contains(this))
                {
                    tcs.SetResult(false);
                    OnInputReport -= reportHandler;
                    cts.Cancel();
                    return;
                }

                if (askForQuery)
                {
                    if (returnedData[0] == (byte)'W')
                    {
                        if ((returnedData[1] & 0x10) != 0 && (returnedData[1] & 0xF) == index && returnedData[2] == block)
                        {
                            tcs.SetResult(true);
                            OnInputReport -= reportHandler;
                            cts.Cancel();
                            return;
                        }
                        if (retryCount >= 5)
                        {
                            tcs.SetResult(false);
                            OnInputReport -= reportHandler;
                            cts.Cancel();
                        }
                        retryCount++;
                        await Task.Delay(80);
                        COMMAND_RequestStatus();
                        askForQuery = false;
                    }
                    return;
                }

                if (returnedData[0] == (byte)'S')
                {
                    ulong figurePresences = (ulong)(returnedData[1] | returnedData[2] << 0x08 | returnedData[3] << 0x10 | returnedData[4] << 0x18);
                    FigurePresence figurePresence = (FigurePresence)((figurePresences >> (index * 2)) & 0b11);

                    if (figurePresence == FigurePresence.Present || figurePresence == FigurePresence.JustArrived)
                    {
                        askForQuery = true;
                        await Task.Delay(80);
                        COMMAND_FigureWrite(index, block, data);
                    }
                    else
                    {
                        tcs.SetResult(false);
                        OnInputReport -= reportHandler;
                        cts.Cancel();
                        return;
                    }
                }
            }

            OnInputReport += reportHandler;

            while (!tcs.Task.IsCompleted && timeout < 4 && Instances.Contains(this) && Figures[index].IsPresent())
            {
                COMMAND_FigureWrite(index, block, data);
                try
                {
                    await Task.Delay(350, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                timeout++;
            }

            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult(false);
                OnInputReport -= reportHandler;
            }
            return await tcs.Task;
        }

        // HID READ/WRITE RELATED FUNCTIONS

        public virtual bool PushTransfer(byte[] data, byte length = 0)
        {
            IntPtr dataBuffer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataBuffer, data.Length);
            bool success = kDevice.ControlTransfer(SetupPacket(length), dataBuffer, length, out _, IntPtr.Zero);
            Marshal.FreeHGlobal(dataBuffer);
            return success;
        }

        public virtual bool WriteRaw(byte[] data) => kDevice.WritePipe(2, data, (uint)data.Length, out _, IntPtr.Zero);

        private WINUSB_SETUP_PACKET SetupPacket(byte length) => new WINUSB_SETUP_PACKET
        {
            RequestType = 0x21,
            Request = 0x09,
            Value = 0x0200,
            Index = 0x0000,
            Length = (ushort)(0x0008 + length)
        };

        private bool isReading;

        public virtual async void StartReading()
        {
            isReading = true;
            byte[] buffer = new byte[REPORT_SIZE];

            while (isReading)
            {
                bool success = false;
                await Task.Run(() => success = kDevice.ReadPipe(0x81, buffer, (uint)buffer.Length, out _, IntPtr.Zero));

                if (success)
                {
                    try
                    {
                        ReportRecieved(buffer);
                    }
                    catch (Exception)
                    {
                    }
                }

                await Task.Delay(8);
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public void StopReading() => isReading = false;

        public virtual void Destroy()
        {
            Instances.Remove(this);

            for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
            {
                Figures[i].Parent = null;
                Figures[i].Dispose();
                Figures[i] = null;
                OnFigureRemoved?.Invoke(Figures[i]);
            }
        }

        public void ReportRecieved(byte[] data)
        {
            switch ((char)data[0])
            {
                case 'A':
                    Active = data[1] != 0x00;
                    break;
                
                case 'R':
                    currentlyQueryingFigure = null;
                    FiguresInQueue.Clear();
                    byte[] bytes = new byte[4];
                    Array.Copy(data, 1, bytes, 0, 4);
                    ID = TrimTrailingZeros(bytes);
                    break;
                
                case 'S':
                    if (State < PortalState.Standby) break;

                    ulong figurePresences = (ulong)(data[1] | data[2] << 0x08 | data[3] << 0x10 | data[4] << 0x18);

                    for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
                    {
                        FigurePresence presence = (FigurePresence)((figurePresences >> (i * 2)) & 0b11);
                        
                        switch (presence)
                        {
                            case FigurePresence.NotPresent:
                            case FigurePresence.JustDeparted:
                                if (Figures[i].IsPresent())
                                    FigureRemoved();
                                break;

                            case FigurePresence.Present:
                            case FigurePresence.JustArrived:
                                if (!Figures[i].IsPresent())
                                    FigureAdded();
                                break;

                        }

                        void FigureAdded()
                        {
                            Figures[i].Presence = presence;
                            OnFigureAdded?.Invoke(Figures[i]);
                        }

                        void FigureRemoved()
                        {
                            Figures[i].Presence = presence;
                            OnFigureRemoved?.Invoke(Figures[i]);
                        }
                    }
                    break;
            }
            OnInputReport?.Invoke(this, data);
        }
    }
}