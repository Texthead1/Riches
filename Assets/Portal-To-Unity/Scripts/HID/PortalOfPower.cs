using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using libusbK;
using PimDeWitte.UnityMainThreadDispatcher;
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
        Sleeping,
        Ready
    }

    public class PortalOfPower
    {
        public static Action<PortalOfPower> OnAdded;
        public static Action<PortalOfPower> OnRemoved;
        public static Action<PortalOfPower> OnRawAdded;
        public static Action<PortalOfPower> OnRawRemoved;
        public static Action<PortalOfPower> OnPortalIOError;
        public static List<PortalOfPower> Instances = new List<PortalOfPower>();
        public static PortalOfPower WithHandle(KLST_DEVINFO_HANDLE handle) => Instances.FirstOrDefault(portal => portal.kHandle.Equals(handle));

        public Action<PortalOfPower, byte[]> OnInputReport;
        public Action<PortalFigure> OnFigureAdded;
        public Action<PortalFigure, FigureDepartInfo> OnFigureRemoved;
        public Action<PortalFigure> OnFinishedReadingFigure;
        public Action<PortalOfPower, byte[]> OnInterference;
        public UsbK kDevice;
        public KLST_DEVINFO_HANDLE kHandle;
        public PortalFigure[] Figures { get; private set; } = new PortalFigure[FIGURE_INDICIES_COUNT];
        public List<PortalFigure> FiguresInQueue = new List<PortalFigure>();
        public PortalFigure currentlyQueryingFigure;

        public byte[] ID { get; private set; } = new byte[4];
        public bool Active { get; private set; }
        public virtual bool IsDigital => false;

        private bool isReading;
        private CancellationTokenSource bootCTS;
        private CancellationTokenSource wirelessCTS = new CancellationTokenSource();
        private List<(char commandChar, DateTime timestamp, bool returned, byte[] extraInfo)> commandQueue = new List<(char, DateTime, bool returned, byte[] extraInfo)>();
        private readonly char[] priorityCommands = new char[] { 'A', 'M', 'Q', 'R', 'W' };
        
        internal PortalState State = PortalState.JustAdded;


        protected PortalOfPower()
        {
            for (byte i = 0; i < FIGURE_INDICIES_COUNT; i++)
                Figures[i] = new PortalFigure(this, i);
        }

        public PortalOfPower(KLST_DEVINFO_HANDLE handle)
        {
            kDevice = new UsbK(handle);
            kHandle = handle;

            for (byte i = 0; i < FIGURE_INDICIES_COUNT; i++)
                Figures[i] = new PortalFigure(this, i);
        }

        public string GetName() => PortalDatabase.NameFromID(ID);
        public PortalInfo GetPortalInfo() => PortalDatabase.PortalFromID(ID);

        internal async Task SetUpPortal()
        {
            int timeout = 0;
            bootCTS = new CancellationTokenSource();
            State = PortalState.SetUpForInterface;
            OnInputReport += SetUpPortalSub;

            while (true)
            {
                if (timeout == 10)
                {
                    OnPortalIOError?.Invoke(this);
                    OnInputReport -= SetUpPortalSub;
                    return;
                }
                COMMAND_ResetPortal();

                try
                {
                    await Task.Delay(80 + (timeout * 20), bootCTS.Token);
                }
                catch (OperationCanceledException) { break; }
                timeout++;
            }

            if (State == PortalState.Sleeping)
            {
                OnInputReport -= SetUpPortalSub;
                return;
            }

            timeout = 0;
            bootCTS = new CancellationTokenSource();

            while (true)
            {
                if (timeout == 5)
                {
                    OnPortalIOError?.Invoke(this);
                    OnInputReport -= SetUpPortalSub;
                    return;
                }
                COMMAND_SetAntenna(true);

                try
                {
                    await Task.Delay(80 + (timeout * 20), bootCTS.Token);
                }
                catch (OperationCanceledException) { break; }
                timeout++;
            }

            if (State == PortalState.Sleeping)
            {
                OnInputReport -= SetUpPortalSub;
                return;
            }

            OnInputReport -= SetUpPortalSub;
            Instances.Add(this);
            OnAdded?.Invoke(this);

            void SetUpPortalSub(PortalOfPower _, byte[] data)
            {
                if (data[0] == (byte)'R')
                {
                    if (State == PortalState.SetUpForInterface)
                    {
                        bootCTS.Cancel();
                        State = PortalState.CommunicatingWithAntenna;
                    }
                }
                else if (data[0] == (byte)'A')
                {
                    if (State == PortalState.CommunicatingWithAntenna)
                    {
                        bootCTS.Cancel();
                        State = PortalState.Ready;
                    }
                }
                else if (data[0] == (byte)'Z' && State >= PortalState.CommunicatingWithAntenna)
                {
                    bootCTS.Cancel();
                    State = PortalState.Sleeping;
                }
            }
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
            byte[] command = ConstructCommand
            (
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
            byte[] command = ConstructCommand
            (
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
            if (side == PortalLED.Trap)
                throw new ArgumentException("Traptanium Portal LED controlled via J command must either be left or right.");

            if (transitionTime == 0)
                PTUManager.LogWarning("Transition time set to 0. Use the L command instead if you wish to set a Traptanium LED to a given color with no transition.", LogPriority.Low);

            byte[] command = ConstructCommand
            (
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
            byte[] command = ConstructCommand
            (
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
            byte[] command = ConstructCommand
            (
                'M',
                (byte)(active ? 0x01 : 0x00)
            );
            PushTransfer(command, 2);
        }

        public void COMMAND_FigureQuery(byte index, byte block)
        {
            byte[] command = ConstructCommand
            (
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
            byte[] command = ConstructCommand
            (
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

            byte[] command = ConstructCommand
            (
                'W',
                index,
                block
            );
            Array.Copy(data, 0, command, 3, 0x10);
            PushTransfer(command, 19);
        }

        public void COMMAND_RequestWirelessDongleFirmwareVersion()
        {
            byte[] command = ConstructCommand((char)0xFA);
            PushTransfer(command, 1);
        }

        public async Task<byte[]> QueryFigureAsync(byte index, byte block)
        {
            if (!Figures[index].IsPresent())
                throw new FigureRemovedException("The requested figure does not exist on the Portal of Power.");

            bool query = true;
            byte timeout = 0;
            byte retryCount = 0;
            object tcsLock = new object();

            CancellationTokenSource cts = new CancellationTokenSource();
            TaskCompletionSource<byte[]> tcs = new TaskCompletionSource<byte[]>();

            void SetResult(TaskCompletionSource<byte[]> tcs, object lockObj, Action<TaskCompletionSource<byte[]>> action)
            {
                if (tcs.Task.IsCompleted)
                    return;

                lock (lockObj)
                {
                    if (!tcs.Task.IsCompleted)
                        action(tcs);
                }
                OnInputReport -= HandleReport;
                cts?.Cancel();
            }

            async void HandleReport(PortalOfPower portal, byte[] input)
            {
                try
                {
                    if (query && input[0] == (byte)'Q')
                    {
                        if ((input[1] & 0x10) != 0 && (input[1] & 0xF) == index && input[2] == block)
                        {
                            byte[] result = new byte[BLOCK_SIZE];
                            Array.Copy(input, 3, result, 0, BLOCK_SIZE);
                            SetResult(tcs, tcsLock, x => x.TrySetResult(result));
                        }
                        else if (retryCount < 2)
                        {
                            retryCount++;
                            query = false;
                            await Task.Delay(80, cts.Token);
                            COMMAND_RequestStatus();
                        }
                        else
                            SetResult(tcs, tcsLock, x => x.TrySetException(new FigureErrorException("The figure being queried could not be read successfully.")));
                    }

                    if (input[0] != (byte)'S')
                        return;

                    query = true;
                    ulong figurePresences = (ulong)(input[1] | input[2] << 0x08 | input[3] << 0x10 | input[4] << 0x18);
                    FigurePresence figurePresence = (FigurePresence)((figurePresences >> (index * 2)) & 0b11);

                    if (figurePresence == FigurePresence.Present || figurePresence == FigurePresence.JustArrived)
                    {
                        await Task.Delay(80, cts.Token);
                        COMMAND_FigureQuery(index, block);
                    }
                    else
                        SetResult(tcs, tcsLock, x => x.TrySetException(new FigureRemovedException("The figure being queried was removed.")));
                }
                catch (Exception ex)
                {
                    SetResult(tcs, tcsLock, x => x.TrySetException(ex));
                }
            }

            OnInputReport += HandleReport;

            while (!tcs.Task.IsCompleted && timeout < 4)
            {
                if (!Instances.Contains(this))
                {
                    SetResult(tcs, tcsLock, x => x.TrySetException(new PortalDisconnectedException("The parent Portal of Power was disconnected.")));
                    break;
                }

                COMMAND_FigureQuery(index, block);
                try
                {
                    await Task.Delay(800, cts.Token);
                }
                catch (TaskCanceledException) { break; }
                timeout++;
            }

            if (timeout >= 4)
                SetResult(tcs, tcsLock, x => x.TrySetException(new PortalIOException("No data was responded by the Portal of Power.")));

            try
            {
                return await tcs.Task;
            }
            finally { cts.Dispose(); }
        }

        public async Task<bool> WriteFigureAsync(byte index, byte block, byte[] data)
        {
            if (!Figures[index].IsPresent())
                throw new FigureRemovedException("The requested figure does not exist on the Portal of Power.");

            bool query = true;
            byte timeout = 0;
            byte retryCount = 0;
            object tcsLock = new object();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenSource cts = new CancellationTokenSource();

            void SetResult(TaskCompletionSource<bool> tcs, object lockObj, Action<TaskCompletionSource<bool>> action)
            {
                if (tcs.Task.IsCompleted)
                    return;

                lock (lockObj)
                {
                    if (!tcs.Task.IsCompleted)
                        action(tcs);
                }
                OnInputReport -= HandleReport;
                cts?.Cancel();
            }

            async void HandleReport(PortalOfPower portal, byte[] input)
            {
                try
                {
                    if (query && input[0] == (byte)'W')
                    {
                        if ((input[1] & 0x10) != 0 && (input[1] & 0xF) == index && input[2] == block)
                            SetResult(tcs, tcsLock, x => x.TrySetResult(true));
                        else if (retryCount < 5)
                        {
                            retryCount++;
                            query = false;
                            await Task.Delay(80, cts.Token);
                            COMMAND_RequestStatus();
                        }
                        else
                            SetResult(tcs, tcsLock, x => x.TrySetException(new FigureErrorException("The figure being written to could not be read successfully.")));
                    }

                    if (input[0] != (byte)'S')
                        return;

                    query = true;
                    ulong figurePresences = (ulong)(input[1] | input[2] << 0x08 | input[3] << 0x10 | input[4] << 0x18);
                    FigurePresence figurePresence = (FigurePresence)((figurePresences >> (index * 2)) & 0b11);

                    if (figurePresence == FigurePresence.Present || figurePresence == FigurePresence.JustArrived)
                    {
                        await Task.Delay(80, cts.Token);
                        COMMAND_FigureWrite(index, block, data);
                    }
                    else
                        SetResult(tcs, tcsLock, x => x.TrySetException(new FigureRemovedException("The figure being written to was removed.")));
                }
                catch (Exception ex)
                {
                    SetResult(tcs, tcsLock, x => x.TrySetException(ex));
                }
            }

            OnInputReport += HandleReport;

            while (!tcs.Task.IsCompleted && timeout < 4)
            {
                if (!Instances.Contains(this))
                {
                    SetResult(tcs, tcsLock, x => x.TrySetException(new PortalDisconnectedException("The parent Portal of Power was disconnected.")));
                    break;
                }

                COMMAND_FigureWrite(index, block, data);
                try
                {
                    await Task.Delay(450, cts.Token);
                }
                catch (TaskCanceledException) { break; }
                timeout++;
            }

            if (timeout >= 4)
                SetResult(tcs, tcsLock, x => x.TrySetException(new PortalIOException("No data was responded by the Portal of Power.")));

            try
            {
                return await tcs.Task;
            }
            finally { cts.Dispose(); }
        }

        public virtual void PlayAudio(AudioClip audioClip)
        {
            const int chunkSize = 16;
            int currentPosition = 0;
            int sampleCount = audioClip.samples;
            float[] samples = new float[chunkSize];
            byte[] data = new byte[chunkSize * 2];

            PTUManager.Log($"Started playing {audioClip.name} on Traptanium Portal", LogPriority.Low);
            while (currentPosition < sampleCount)
            {
                audioClip.GetData(samples, currentPosition);

                for (int num = 0; num < samples.Length; num++)
                {
                    if (currentPosition + num > sampleCount)
                        break;

                    ushort sample16 = (ushort)(Mathf.Clamp(samples[num] * 0.5f, -1f, 1f) * 32767f);
                    Array.Copy(BitConverter.GetBytes(sample16), 0, data, num * 2, 2);
                }

                if (!WriteRaw(data))
                {
                    PTUManager.LogWarning("Audio transmission terminated. Portal write error, most likely as a result of it being disconnected", LogPriority.Low);
                    return;
                }
                currentPosition += chunkSize;
            }
            PTUManager.Log($"Finished playing {audioClip.name} on Traptanium Portal", LogPriority.Low);
        }

        // investigate why there seems to be audio corruption with this method, but not the syncrhonous method
        // heck, this method functions flawlessly in a different project, so what's happening here?
        public async Task PlayTraptaniumAudio(AudioClip audioClip, float audioMult = 1)
        {
            const int CHUNK_SIZE = 16;
            const int CHUNK_COUNT = 8;
            const int TRANSFER_SIZE = CHUNK_SIZE * CHUNK_COUNT;
            int position = 0;
            int sampleCount = audioClip.samples;
            float[] samples = new float[TRANSFER_SIZE];
            byte[] sampleData = new byte[TRANSFER_SIZE * 2];

            PTUManager.Log($"Started playing {audioClip.name} on Traptanium Portal", LogPriority.Low);
            await UnityMainThreadDispatcher.Instance().EnqueueAsync(() => audioClip.GetData(samples, position));

            new Thread(async () =>
            {
                while (position < sampleCount)
                {
                    float[] nextSamples = new float[TRANSFER_SIZE];
                    
                    await UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
                    {
                        // Check that we're not exceeding past the amount of samples present in the file when grabbing future ones
                        if (sampleCount - (position + TRANSFER_SIZE) >= 0)
                            audioClip.GetData(nextSamples, position + TRANSFER_SIZE);
                    });

                    for (int i = 0; i < CHUNK_COUNT; i++)
                    {
                        int offset = i * CHUNK_SIZE;

                        for (int j = 0; j < CHUNK_SIZE; j++)
                        {
                            int sampleIndex = offset + j;
                            if (position + sampleIndex >= sampleCount)
                                break;
                            
                            ushort sample = (ushort)(Mathf.Clamp(samples[sampleIndex] * audioMult, -1f, 1f) * 32767f);
                            Array.Copy(BitConverter.GetBytes(sample), 0, sampleData, sampleIndex * 2, 2);
                        }

                        byte[] transferData = new byte[CHUNK_SIZE * 2];
                        Array.Copy(sampleData, offset * 2, transferData, 0, CHUNK_SIZE * 2);

                        if (!WriteRaw(transferData))
                            throw new IOException("Could not transfer AudioClip data successfully");
                    }
                    position += TRANSFER_SIZE;
                    samples = nextSamples;
                }
            }).Start();
            PTUManager.Log($"Finished playing {audioClip.name} on Traptanium Portal", LogPriority.Low);
        }

        // HID READ/WRITE RELATED FUNCTIONS

        public virtual bool PushTransfer(byte[] data, byte length = 0)
        {
            char commandChar = (char)data[0];
            PTUManager.LogWarning($"OUTPUT ({commandChar}): {BytesToHexString(data)}", LogPriority.Low);

            UpdateReportValidator(data, false);

            IntPtr dataBuffer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataBuffer, data.Length);
            bool success =  kDevice.ControlTransfer(SetupPacket(length), dataBuffer, length, out _, IntPtr.Zero);

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

        private bool UpdateReportValidator(byte[] input, bool detectInterference)
        {
            char commandChar = (char)input[0];
            DateTime update = DateTime.Now;

            List<(char commandChar, DateTime timestamp, bool returned, byte[] extraInfo)> matches = commandQueue.Where(x => x.commandChar == commandChar).ToList();
            (char commandChar, DateTime timestamp, bool returned, byte[] extraInfo) match = default;

            if (commandChar != 'Q' && commandChar != 'W')
                match = commandQueue.FirstOrDefault(x => x.commandChar == commandChar);
            else
            {
                foreach (var newMatch in matches)
                {
                    byte[] sequence = new byte[newMatch.extraInfo.Length];
                    Array.Copy(input, 1, sequence, 0, newMatch.extraInfo.Length);
                    if (newMatch.extraInfo.SequenceEqual(sequence))
                    {
                        match = newMatch;
                        break;
                    }
                    Debug.Log("OG: " + BytesToHexString(sequence));
                    Debug.Log("Found: " + BytesToHexString(newMatch.extraInfo));
                }
            }

            if (match != default)
                match.returned = true;
            
            // ngl the portals are so unpredictable, due to variable query times - especially long with invalid reads - and 3ds portals, that interference is difficult to detect while accomodating for all factors
            commandQueue.RemoveAll(x => (update - x.timestamp).TotalMilliseconds >= (x.returned ? 400 : 1200));

            if (priorityCommands.Contains(commandChar))
            {
                (char, DateTime timestamp, bool returned, byte[]) existingCommand = default;

                if (commandChar != 'Q' && commandChar != 'W')
                    existingCommand = commandQueue.FirstOrDefault(x => x.commandChar == commandChar);
                else
                {
                    foreach (var newMatch in matches)
                    {
                        byte[] sequence = new byte[newMatch.extraInfo.Length];
                        Array.Copy(input, 1, sequence, 0, newMatch.extraInfo.Length);
                        if (newMatch.extraInfo.SequenceEqual(sequence))
                        {
                            existingCommand = newMatch;
                            break;
                        }
                        // still need to properly test different figure/block interference of the same command type
                        Debug.Log("OG: " + BytesToHexString(sequence));
                        Debug.Log("Found: " + BytesToHexString(newMatch.extraInfo));
                    }
                }

                if (existingCommand != default)
                    existingCommand.timestamp = update;
                else
                {
                    if (commandChar == 'Q' || commandChar == 'W')
                        commandQueue.Add((commandChar, update, false, (commandChar == 'Q' || commandChar == 'W') ? new byte[2] { input[1], input[2] } : null));
                    
                    if (detectInterference)
                    {
                        OnInterference?.Invoke(this, input);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual async void StartReading()
        {
            isReading = true;
            byte[] buffer = new byte[REPORT_SIZE];

            while (isReading)
            {
                Task<bool> readTask = Task.Run(() => kDevice.ReadPipe(0x81, buffer, (uint)buffer.Length, out _, IntPtr.Zero));

                if (await readTask)
                    ReportRecieved(buffer);

                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public void StopReading() => isReading = false;

        public virtual bool Destroy()
        {
            bootCTS?.Cancel();
            bootCTS?.Dispose();
            wirelessCTS?.Cancel();
            wirelessCTS?.Dispose();

            if (Instances.Contains(this))
            {
                Instances.Remove(this);
                for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
                {
                    OnFigureRemoved?.Invoke(Figures[i], FigureDepartInfo.ParentPortalDisconnected);
                    Figures[i].Dispose();
                }
                return true;
            }
            return false;
        }

        public async void ReportRecieved(byte[] data)
        {
            char commandChar = (char)data[0];
            //if (UpdateReportValidator(data, true)) return;

            if (commandChar != 'S')
                PTUManager.Log($"INPUT ({commandChar}): {BytesToHexString(data)}", LogPriority.Low);

            if (data[0] == (byte)'Z')
            {
                if (State != PortalState.Sleeping)
                {
                    State = PortalState.Sleeping;
                    for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
                    {
                        OnFigureRemoved?.Invoke(Figures[i], FigureDepartInfo.ParentPortalDisconnected);
                        Figures[i].Reset();
                    }
                    Instances.Remove(this);
                    OnRemoved?.Invoke(this);
                }
                WakeQueue();
            }
            else if (data[0] != 0xFA && !Instances.Contains(this) && State == PortalState.Sleeping)
                await SetUpPortal();

            switch (commandChar)
            {
                case 'A':
                    Active = data[1] != 0x00;
                    break;
                
                case 'R':
                    if (State >= PortalState.Sleeping)
                    {
                        for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
                        {
                            OnFigureRemoved?.Invoke(Figures[i], FigureDepartInfo.PortalIndiciesReset);
                            Figures[i].Reset();
                        }
                        currentlyQueryingFigure = null;
                        FiguresInQueue.Clear();
                    }

                    // accomodate for weird wireless dongle "bug" (or whatever returning this id is meant to signify)
                    if (data[1] == 0x90 && data[2] == 0x00) break;

                    byte[] bytes = new byte[4];
                    Array.Copy(data, 1, bytes, 0, 4);
                    ID = TrimTrailingZeros(bytes);
                    break;
                
                case 'S':
                    if (State < PortalState.Sleeping) break;
                    ulong figurePresences = (ulong)(data[1] | data[2] << 0x08 | data[3] << 0x10 | data[4] << 0x18);

                    for (int i = 0; i < FIGURE_INDICIES_COUNT; i++)
                    {
                        FigurePresence presence = (FigurePresence)((figurePresences >> (i * 2)) & 0b11);
                        
                        switch (presence)
                        {
                            case FigurePresence.NotPresent:
                            case FigurePresence.JustDeparted:
                                if (Figures[i].IsPresent())
                                {
                                    Figures[i].Presence = presence;
                                    OnFigureRemoved?.Invoke(Figures[i], FigureDepartInfo.RemovedFromPortal);
                                }
                                break;

                            case FigurePresence.Present:
                            case FigurePresence.JustArrived:
                                if (!Figures[i].IsPresent())
                                {
                                    Figures[i].Presence = presence;
                                    OnFigureAdded?.Invoke(Figures[i]);
                                }
                                break;

                        }
                    }
                    break;
            }
            OnInputReport?.Invoke(this, data);
        }

        private async void WakeQueue()
        {
            wirelessCTS?.Cancel();
            wirelessCTS = new CancellationTokenSource();

            try
            {
                await Task.Delay(150, wirelessCTS.Token);
                if (State == PortalState.Sleeping)
                    await SetUpPortal();
            }
            catch (TaskCanceledException) {}
        }
    }
}