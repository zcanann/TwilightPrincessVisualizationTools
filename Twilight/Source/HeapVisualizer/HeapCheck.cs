
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public Byte[] unknownBuffer;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 heapSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 usedCount;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 totalUsedSize;

        public static HeapCheck FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                HeapCheck result = (HeapCheck)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HeapCheck));

                // Fix GC endianness
                result.mNamePointer = BinaryPrimitives.ReverseEndianness(result.mNamePointer);
                result.jNamePointer = BinaryPrimitives.ReverseEndianness(result.jNamePointer);
                result.heapPointer = BinaryPrimitives.ReverseEndianness(result.heapPointer);
                result.maxTotalUsedSize = BinaryPrimitives.ReverseEndianness(result.maxTotalUsedSize);
                result.maxTotalFreeSize = BinaryPrimitives.ReverseEndianness(result.maxTotalFreeSize);
                // result.unknownBuffer = BinaryPrimitives.ReverseEndianness(result.unknownBuffer);
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
