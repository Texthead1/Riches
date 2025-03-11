using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static PortalToUnity.Global;

namespace PortalToUnity
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x140)]
    public unsafe struct SpyroTag_CreationCrystal
    {
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x40)]
        public struct MagicMoment
        {
            [FieldOffset(0x00)] public ushort experience2011;
            [FieldOffset(0x02)] public byte experience2011High;
            [FieldOffset(0x03)] public ushort money;
            [FieldOffset(0x05)] public uint cumulativeTime;
            [FieldOffset(0x09)] public byte areaSequence;
            [FieldOffset(0x0A)] public ushort crcCYOS;
            [FieldOffset(0x0C)] public ushort crcType2;
            [FieldOffset(0x0E)] public ushort crcType1;
            [FieldOffset(0x10)] public ushort flags;
            [FieldOffset(0x12)] public byte flagsHigh;
            [FieldOffset(0x13)] public byte platforms2011;
            [FieldOffset(0x14)] public ushort hat2011;                      // ?
            [FieldOffset(0x16)] public byte regionCountIdentifier;          // ?
            [FieldOffset(0x17)] public byte platforms2013;
            [FieldOffset(0x19)] public byte cyosElement;
            [FieldOffset(0x20)] public fixed ushort nickname[16];
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x100)]
        public struct RemainingData
        {
            [FieldOffset(0x00)] public TagTimeDate lastUsed;
            [FieldOffset(0x06)] public uint heroicChallenges2011;           // ?
            [FieldOffset(0x0A)] public ushort heroPoints;                   // ?
            [FieldOffset(0x0C)] public fixed byte lastUsageID[3];
            [FieldOffset(0x0F)] public byte ownerCount;
            [FieldOffset(0x10)] public TagTimeDate firstUsed;
            [FieldOffset(0x20)] public fixed byte unknownUsageIdentifier[15];
            [FieldOffset(0x33)] public ushort experience2012;
            [FieldOffset(0x36)] public ushort flags2;
            [FieldOffset(0x38)] public uint experience2013;
            [FieldOffset(0x77)] public byte AuraClass;


            public unsafe byte GetBattleClass() => (byte)(AuraClass & 0xF);
            public unsafe void SetBattleClass(BattleClass battleClass) => AuraClass = (byte)((AuraClass & 0xF0) | (((byte)battleClass) & 0xF));

        }

        [FieldOffset(0x00)] public MagicMoment magicMoment;
        [FieldOffset(0x40)] public RemainingData remainingData;
    }

    public class FigType_CreationCrystal : FigType
    {
        public unsafe SpyroTag_CreationCrystal* SpyroTag { get; protected set; }
        public byte DataArea = 0;

#region FigType Info
        public unsafe FigType_CreationCrystal(PortalFigure portalFigure) : base(portalFigure)
        {
            int size = sizeof(SpyroTag_CreationCrystal);
            SpyroTag = (SpyroTag_CreationCrystal*)Marshal.AllocHGlobal(size);

            for (int i = 0; i < size; i++)
                ((byte*)SpyroTag)[i] = 0;
        }

        public override unsafe byte* GetBlock(int block)
        {
            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_CreationCrystal))
                throw new ArgumentOutOfRangeException(nameof(block), "Block index is out of range");
            
            return (byte*)SpyroTag + offset;
        }

        public override unsafe byte[] GetBlockManaged(int block)
        {
            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_CreationCrystal))
                throw new ArgumentOutOfRangeException(nameof(block), "Block index is out of range");

            byte[] data = new byte[BLOCK_SIZE];

            fixed (byte* dataPtr = data)
                Buffer.MemoryCopy((byte*)SpyroTag + offset, dataPtr, BLOCK_SIZE, BLOCK_SIZE);
            
            return data;
        }

        public override unsafe void SetBlock(int block, byte* data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Pointer to block data cannot be null");

            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_CreationCrystal))
                throw new ArgumentOutOfRangeException(nameof(block), "Block index is out of range");
            
            byte* destination = (byte*)SpyroTag + offset;
            Buffer.MemoryCopy(data, destination, BLOCK_SIZE, BLOCK_SIZE);
        }

        public override unsafe void SetBlockManaged(int block, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Pointer to block data cannot be null");

            int offset = block * BLOCK_SIZE;

            if (offset < 0 || offset >= sizeof(SpyroTag_CreationCrystal))
                throw new ArgumentOutOfRangeException(nameof(block), "Block index is out of range");
            
            fixed (byte* dataPtr = data)
            {
                byte* destination = (byte*)SpyroTag + offset;
                Buffer.MemoryCopy(dataPtr, destination, BLOCK_SIZE, BLOCK_SIZE);
            }
        }

        public override unsafe void Dispose()
        {
            if (SpyroTag != null)
            {
                Marshal.FreeHGlobal((IntPtr)SpyroTag);
                SpyroTag = null;
            }
        }
#endregion

        public async Task FetchMagicMoment()
        {
            byte[] block0 = await portalFigure.FetchBlock(DATA_REGION0_OFFSET);
            byte[] block1 = await portalFigure.FetchBlock(DATA_REGION1_OFFSET);

            if ((byte)(block0[0x09] + 1) == block1[0x09])
            {
                DataArea = 1;
                SetBlockManaged(0x00, block1);
            }
            else
            {
                if ((byte)(block1[0x09] + 1) != block0[0x09])
                    PTUManager.LogWarning("Data areas out of sync. First area chosen by default", LogPriority.High);

                DataArea = 0;
                SetBlockManaged(0x00, block0);
            }

            int size = BlockSizeOf<SpyroTag_CreationCrystal.MagicMoment>();
            int areaOffset = (DataArea == 1) ? DATA_REGION1_OFFSET : DATA_REGION0_OFFSET;
            int offset = 0;

            for (int i = 1; i < size; i++)
            {
                if (IsAccessControlBlock((byte)(i + areaOffset + offset)))
                    offset++;

                SetBlockManaged(i, await portalFigure.FetchBlock((byte)(i + areaOffset + offset)));
            }
        }

        public async Task FetchRemainingData()
        {
            int size = BlockSizeOf<SpyroTag_CreationCrystal.RemainingData>();
            int areaOffset = (DataArea == 1) ? DATA_REGION1_OFFSET : DATA_REGION0_OFFSET;
            int offset = 0x05;

            for (int i = 0; i < size; i++)
            {
                if (IsAccessControlBlock((byte)(i + areaOffset + offset)))
                    offset++;

                SetBlockManaged(i + 0x04, await portalFigure.FetchBlock((byte)(i + areaOffset + offset)));
            }
        }

        private unsafe void SetUpRegionHeader()
        {
            SpyroTag->magicMoment.crcCYOS = CalculateCRCCYOS();
            SpyroTag->magicMoment.crcType2 = CalculateCRCType2();
            SpyroTag->magicMoment.areaSequence++;
            SpyroTag->magicMoment.crcType1 = CalculateCRCType1();
        }

        private unsafe ushort CalculateCRCCYOS()
        {
            Checksum crcCYOS = new Checksum(sizeof(SpyroTag_CreationCrystal.RemainingData), (IntPtr)(&SpyroTag->magicMoment.crcCYOS));
            return crcCYOS.Calculate((IntPtr)(&SpyroTag->remainingData));
        }

        private unsafe ushort CalculateCRCType2()
        {
            Checksum crcType2 = new Checksum(BLOCK_SIZE * 3, (IntPtr)(&SpyroTag->magicMoment.crcType2));
            return crcType2.Calculate((IntPtr)(&SpyroTag->magicMoment) + BLOCK_SIZE);
        }

        private unsafe ushort CalculateCRCType1()
        {
            Checksum crcType1 = new Checksum(BLOCK_SIZE, (IntPtr)(&SpyroTag->magicMoment.crcType1), 0x0500);
            return crcType1.Calculate((IntPtr)(&SpyroTag->magicMoment));
        }

        public async Task SetMagicMoment()
        {
            SetUpRegionHeader();
            DataArea = (byte)((DataArea + 1) & 1);
            int size = BlockSizeOf<SpyroTag_CreationCrystal.MagicMoment>();
            int areaOffset = (DataArea == 1) ? DATA_REGION1_OFFSET : DATA_REGION0_OFFSET;
            int offset = 0;

            for (int i = 0; i < size; i++)
            {
                byte[] data = GetBlockManaged(i);

                if (IsAccessControlBlock((byte)(i + areaOffset + offset)))
                    offset++;
                
                byte targetBlock = (byte)(i + areaOffset + offset);

                unsafe
                {
                    fixed (byte* dataPtr = data)
                        Cryptography.EncryptSpyroTagBlock(portalFigure.TagHeader, dataPtr, targetBlock);
                }
                await portalFigure.Parent.WriteFigureAsync(portalFigure.Index, targetBlock, data);
            }
        }

        public async Task SetRemainingData()
        {
            int size = BlockSizeOf<SpyroTag_CreationCrystal.RemainingData>();
            int areaOffset = (DataArea == 1) ? DATA_REGION1_OFFSET : DATA_REGION0_OFFSET;
            int offset = 0x05;

            for (int i = 0; i < size; i++)
            {
                byte[] data = GetBlockManaged(i + 0x04);

                if (IsAccessControlBlock((byte)(i + areaOffset + offset)))
                    offset++;
                
                byte targetBlock = (byte)(i + areaOffset + offset);

                unsafe
                {
                    fixed (byte* dataPtr = data)
                        Cryptography.EncryptSpyroTagBlock(portalFigure.TagHeader, dataPtr, targetBlock);
                }
                await portalFigure.Parent.WriteFigureAsync(portalFigure.Index, targetBlock, data);
            }
        }
    }
}