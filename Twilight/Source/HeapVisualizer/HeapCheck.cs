
namespace Twilight.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x28)]
    public class HeapCheck
    {
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 mNamePointer;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jNamePointer;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 heapPointer;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 maxTotalUsedSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 maxTotalFreeSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 heapSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 usedCount;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 totalUsedSize;

        public static HeapCheck FromByteArray(byte[] bytes)
        {
            // Convert Wii structure to GC. jNamePointer field is missing, so we shift all the bytes down and leave that as nullptr.
            if (bytes.Length == 0x24)
            {
                byte[] newBytes = new byte[0x28];

                Array.Copy(bytes, newBytes, 0x4);
                Array.Copy(bytes, 0x4, newBytes, 0x8, 0x20);

                bytes = newBytes;
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                HeapCheck result = Marshal.PtrToStructure<HeapCheck>(handle.AddrOfPinnedObject());

                // Fix GC endianness
                result.mNamePointer = BinaryPrimitives.ReverseEndianness(result.mNamePointer);
                result.heapPointer = BinaryPrimitives.ReverseEndianness(result.heapPointer);
                result.maxTotalUsedSize = BinaryPrimitives.ReverseEndianness(result.maxTotalUsedSize);
                result.maxTotalFreeSize = BinaryPrimitives.ReverseEndianness(result.maxTotalFreeSize);
                result.unknownPtr1 = BinaryPrimitives.ReverseEndianness(result.unknownPtr1);
                result.unknownPtr2 = BinaryPrimitives.ReverseEndianness(result.unknownPtr2);
                result.heapSize = BinaryPrimitives.ReverseEndianness(result.heapSize);
                result.usedCount = BinaryPrimitives.ReverseEndianness(result.usedCount);
                result.totalUsedSize = BinaryPrimitives.ReverseEndianness(result.totalUsedSize);

                return result;
            }
            finally
            {
                handle.Free();
            }
        }
    }
    //// End class
}
//// End namespace
