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

    public enum FigureBlockQueryState
    {
        Success,
        FigureRemoved,
        Error
    }

    public enum RichesRecogniseState
    {
        Normal,
        Unsupported,
        Unknown,
        NotASkylander
    }

    public enum FigurePresence
    {
        NotPresent = 0b00,
        Present = 0b01,
        JustDeparted = 0b10,
        JustArrived = 0b11
    }

    public class PortalFigure : IDisposable
    {
        public PortalOfPower Parent { get; internal set; }
        public byte Index { get; }

        public unsafe SpyroTag_TagHeader* TagHeader { get; protected set; }
        public FigType TagBuffer;
        public FigurePresence Presence;
        public FigureReadingState ReadingState { get; }
        public RichesRecogniseState recogniseState;

        public unsafe PortalFigure(PortalOfPower parent, byte index)
        {
            Parent = parent;
            Index = index;
            Presence = FigurePresence.NotPresent;

            var allocSize = sizeof(SpyroTag_TagHeader);
            TagHeader = (SpyroTag_TagHeader*)Marshal.AllocHGlobal(allocSize);

            for (int i = 0; i < sizeof(SpyroTag_TagHeader); i++)
                ((byte*)TagHeader)[i] = 0;
        }

        public bool IsPresent() => Presence == FigurePresence.Present || Presence == FigurePresence.JustArrived;

        public async Task<byte[]> FetchBlock(byte block)
        {
            QueryReturnInfo info = await Parent.QueryFigureAsync(Index, block);

            if (info.ReadState == FigureBlockQueryState.FigureRemoved)
                throw new FigureRemovedException($"The figure being queried at block {block} was removed.");

            if (info.ReadState == FigureBlockQueryState.Error)
                throw new FigureErrorException($"The figure being queried at block {block} could not be read successfully.");

            if (block < 8 || IsAccessControlBlock(block))
                return info.Data;
            
            unsafe
            {
                fixed (byte* ptr = info.Data)
                    Cryptography.DecryptSpyroTagBlock(TagHeader, ptr, block);
            }
            return info.Data;
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