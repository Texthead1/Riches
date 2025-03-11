using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using libusbK;

namespace PortalToUnity
{
    public static class Global
    {
        public const int PORTAL_VENDOR_ID = 0x1430;
        public const int PORTAL_PRODUCT_ID = 0x0150;
        public const int PORTAL_PRODUCT_ID_XBOX360 = 0x1F17;
        public const int PORTAL_PRODUCT_ID_XBOXONE = 0x09AA;
        public const int REPORT_SIZE = 0x20;
        public const int FIGURE_INDICIES_COUNT = 0x10;

        public const int SALT_LENGTH = 0x35;
        public const int REGION_COUNT = 2;
        public const int BLOCK_SIZE = 0x10;
        public const int BLOCK_COUNT = 0x40;
        public const int TAG_HEADER_SIZE = BLOCK_SIZE * 2;
        public const int KEY_SIZE = TAG_HEADER_SIZE + 1 + SALT_LENGTH;
        public const int SECTOR_PERMISSION_0 = 0x690F0F0F;
        public const int SECTOR_PERMISSION_FULL = 0x69080F7F;
        public const int DATA_REGION0_OFFSET = 0x08;
        public const int DATA_REGION1_OFFSET = 0x24;

        public static bool IsPortalOfPower(this KLST_DEVINFO_HANDLE info) => info.Common.Vid == PORTAL_VENDOR_ID && info.Common.Pid == PORTAL_PRODUCT_ID;
        public static string BytesToHexString(byte[] data) => string.Join(" ", data.Select(x => x.ToString("X2")));

        public static byte[] TrimTrailingZeros(byte[] data)
        {
            int lastIndex = Array.FindLastIndex(data, x => x != 0);

            if (lastIndex == -1)
                return new byte[0];

            byte[] trimmedArray = new byte[lastIndex + 1];
            Array.Copy(data, trimmedArray, lastIndex + 1);
            return trimmedArray;
        }

        public static byte[] AddZeroPadding(byte[] array, int amount)
        {
            if (amount == 0)
                return array;
            
            int length = array.Length;

            byte[] newArray = new byte[length + amount];
            Array.Copy(array, 0, newArray, amount, length);
            return newArray;
        }

        public static int BlockSizeOf<T>() where T : struct => Marshal.SizeOf<T>() / BLOCK_SIZE;

        public static bool IsAccessControlBlock(byte block) => (block % 4) == 3;

        public static string SpyroTagToHexString<T>(T spyroTag) where T : struct
        {
            int size = Marshal.SizeOf(spyroTag);
            byte[] bytes = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(spyroTag, ptr, true);
                Marshal.Copy(ptr, bytes, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            StringBuilder hexString = new StringBuilder(size * 3);

            for (int i = 0; i < size; i++)
            {
                if (i > 0 && i % BLOCK_SIZE == 0)
                    hexString.AppendLine();

                hexString.Append($"{bytes[i]:X2} ");
            }
            return hexString.ToString();
        }
    }
}