using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static PortalToUnity.Global;

namespace PortalToUnity
{
    public enum FigureReadingState
    {
        NotRead,
        ReadingHeader,
        ReadHeader,
        ReadingMagicMoment,
        ReadMagicMoment,
        ReadingRemainingData,
        ReadRemainingData,
        Other,
        Finished
    }

    public enum FigurePresence
    {
        NotPresent = 0b00,
        Present = 0b01,
        JustDeparted = 0b10,
        JustArrived = 0b11
    }

    public enum FigureAcknowledgeState
    {
        Recognised,
        Unsupported,
        Unrecognised,
        NotASkylander
    }

    public enum FigureDepartInfo
    {
        RemovedFromPortal,
        PortalIndiciesReset,
        ParentPortalDisconnected
    }

    public class PortalFigure : IDisposable
    {
        public PortalOfPower Parent { get; internal set;}
        public byte Index { get; }

        public unsafe SpyroTag_TagHeader* TagHeader { get; protected set; }
        public FigType TagBuffer;

        public FigurePresence Presence;
        public FigureAcknowledgeState AcknowledgeState;
        public FigureReadingState ReadingState { get; private set; }

        bool disposed = false;

        public unsafe PortalFigure(PortalOfPower parent, byte index)
        {
            Parent = parent;
            Index = index;
            Reset();
        }

        public unsafe void Reset()
        {
            Presence = FigurePresence.NotPresent;

            int allocSize = sizeof(SpyroTag_TagHeader);
            TagHeader = (SpyroTag_TagHeader*)Marshal.AllocHGlobal(allocSize);

            for (int i = 0; i < sizeof(SpyroTag_TagHeader); i++)
                ((byte*)TagHeader)[i] = 0;

            TagBuffer = null;
            ReadingState = FigureReadingState.NotRead;
        }

        public bool IsPresent() => Presence == FigurePresence.Present || Presence == FigurePresence.JustArrived;

        public async Task<byte[]> FetchBlock(byte block)
        {
            try
            {
                byte[] data = await Parent.QueryFigureAsync(Index, block);
                
                if (block < 8 || IsAccessControlBlock(block))
                    return data;
            
                unsafe
                {
                    fixed (byte* ptr = data)
                        Cryptography.DecryptSpyroTagBlock(TagHeader, ptr, block);
                }
                return data;
            }
            catch (Exception ex)
            {
                PTUManager.LogException(ex, LogPriority.Low);
                throw;
            }
        }

        public async Task<bool> WriteBlock(byte block, byte[] data)
        {
            try
            {
                unsafe
                {
                    fixed (byte* dataPtr = data)
                        Cryptography.EncryptSpyroTagBlock(TagHeader, dataPtr, block);
                }
                return await Parent.WriteFigureAsync(Index, block, data);
            }
            catch (Exception ex)
            {
                PTUManager.LogException(ex, LogPriority.Low);
                throw;
            }
        }

        public unsafe void SetHeaderBlock(int block, byte* data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Pointer to block data cannot be null");

            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_TagHeader))
                throw new ArgumentOutOfRangeException(nameof(block), $"Block index is out of range");
            
            byte* destination = (byte*)TagHeader + offset;
            Buffer.MemoryCopy(data, destination, BLOCK_SIZE, BLOCK_SIZE);
        }

        public unsafe void SetHeaderBlockManaged(int block, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Block data cannot be null");

            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_TagHeader))
                throw new ArgumentOutOfRangeException(nameof(block), $"Block index is out of range");
            
            byte* destination = (byte*)TagHeader + offset;

            fixed (byte* dataPtr = data)
                Buffer.MemoryCopy(dataPtr, destination, BLOCK_SIZE, BLOCK_SIZE);
        }

        public async Task FetchTagHeader()
        {
            byte[] header0 = await FetchBlock(0);
            byte[] header1 = await FetchBlock(1);
            SetHeaderBlockManaged(0, header0);
            SetHeaderBlockManaged(1, header1);
        }

        public unsafe void Dispose()
        {
            if (TagHeader != null)
            {
                Marshal.FreeHGlobal((IntPtr)TagHeader);
                TagHeader = null;
            }
        }

        ~PortalFigure()
        {
            Dispose();
        }
    }
}