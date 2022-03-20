
namespace Twilight.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x14)]
    public class CMemBlock
    {
        [MarshalAs(UnmanagedType.I2)]
        public UInt16 magic;

        [MarshalAs(UnmanagedType.I1)]
        public byte flags;

        [MarshalAs(UnmanagedType.I1)]
        public byte groupId;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 blockSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 previousPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 nextPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknown;

        public static CMemBlock FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                CMemBlock result = (CMemBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CMemBlock));

                // Fix GC endianness
                result.magic = BinaryPrimitives.ReverseEndianness(result.magic);
                result.blockSize = BinaryPrimitives.ReverseEndianness(result.blockSize);
                result.previousPtr = BinaryPrimitives.ReverseEndianness(result.previousPtr);
                result.nextPtr = BinaryPrimitives.ReverseEndianness(result.nextPtr);
                result.unknown = BinaryPrimitives.ReverseEndianness(result.unknown);

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
