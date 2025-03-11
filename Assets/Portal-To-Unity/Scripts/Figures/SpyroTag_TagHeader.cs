using System.Runtime.InteropServices;

namespace PortalToUnity
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x20)]
    public readonly unsafe struct SpyroTag_TagHeader
    {
        [FieldOffset(0x00)] public readonly uint serial;
        [FieldOffset(0x04)] public readonly byte bcc;
        [FieldOffset(0x05)] public readonly byte sak;
        [FieldOffset(0x06)] public readonly ushort atqa;
        [FieldOffset(0x08)] public readonly ulong manufacturerData;
        [FieldOffset(0x10)] public readonly ushort toyType;
        [FieldOffset(0x12)] public readonly byte toyTypeHigh;
        [FieldOffset(0x13)] public readonly byte errorByte;
        [FieldOffset(0x14)] public readonly ulong tradingCardID;
        [FieldOffset(0x1C)] public readonly ushort subType;
        [FieldOffset(0x1E)] public readonly ushort crcType0;
    }
}