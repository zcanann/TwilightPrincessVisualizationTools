
namespace Twilight.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x88)]
    public class JKRExpHeap
    {
        [MarshalAs(UnmanagedType.I4)]
        public UInt32 vTablePtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jkrHeapPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jsuPtrLink1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jsuPtrLink2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jsuPtrLink3;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 jsuPtrLink4;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public Byte[] osMutex;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 cMemBlockPtr1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 cMemBlockPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 heapSize;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public Byte[] unknownData;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 freeBlocksHeadPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 freeBlocksTailPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 usedBlocksHeadPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 usedBlocksTailPtr;

        public static JKRExpHeap FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                JKRExpHeap result = (JKRExpHeap)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(JKRExpHeap));

                // Fix GC endianness
                result.vTablePtr = BinaryPrimitives.ReverseEndianness(result.vTablePtr);
                result.jkrHeapPtr = BinaryPrimitives.ReverseEndianness(result.jkrHeapPtr);
                result.jsuPtrLink1 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink1);
                result.jsuPtrLink2 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink2);
                result.jsuPtrLink3 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink3);
                result.jsuPtrLink4 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink4);
                result.cMemBlockPtr1 = BinaryPrimitives.ReverseEndianness(result.cMemBlockPtr1);
                result.cMemBlockPtr2 = BinaryPrimitives.ReverseEndianness(result.cMemBlockPtr2);
                // result.unknownBuffer = BinaryPrimitives.ReverseEndianness(result.unknownBuffer);
                result.heapSize = BinaryPrimitives.ReverseEndianness(result.heapSize);
                result.freeBlocksHeadPtr = BinaryPrimitives.ReverseEndianness(result.freeBlocksHeadPtr);
                result.freeBlocksTailPtr = BinaryPrimitives.ReverseEndianness(result.freeBlocksTailPtr);
                result.usedBlocksHeadPtr = BinaryPrimitives.ReverseEndianness(result.usedBlocksHeadPtr);
                result.usedBlocksTailPtr = BinaryPrimitives.ReverseEndianness(result.usedBlocksTailPtr);

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
