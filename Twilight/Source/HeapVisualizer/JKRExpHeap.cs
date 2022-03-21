
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

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownData1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownData2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownData3;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownData4;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr1; // Offset 76

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr3;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr4;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr5;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownPtr6;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 unknownData5;

        [MarshalAs(UnmanagedType.I1)]
        public byte unknownBool1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Byte[] padding1;

        [MarshalAs(UnmanagedType.I1)]
        public byte unknownBool2;

        [MarshalAs(UnmanagedType.I1)]
        public byte currentGroupId;

        [MarshalAs(UnmanagedType.I1)]
        public byte unknownBool3;

        [MarshalAs(UnmanagedType.I1)]
        public byte padding2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 dataPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 dataSize;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 freeBlocksHeadPtr; // Offset 120

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
                JKRExpHeap result = Marshal.PtrToStructure<JKRExpHeap>(handle.AddrOfPinnedObject());

                // Fix GC endianness
                result.vTablePtr = BinaryPrimitives.ReverseEndianness(result.vTablePtr);
                result.jkrHeapPtr = BinaryPrimitives.ReverseEndianness(result.jkrHeapPtr);
                result.jsuPtrLink1 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink1);
                result.jsuPtrLink2 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink2);
                result.jsuPtrLink3 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink3);
                result.jsuPtrLink4 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink4);
                result.cMemBlockPtr1 = BinaryPrimitives.ReverseEndianness(result.cMemBlockPtr1);
                result.cMemBlockPtr2 = BinaryPrimitives.ReverseEndianness(result.cMemBlockPtr2);
                result.heapSize = BinaryPrimitives.ReverseEndianness(result.heapSize);
                result.unknownData1 = BinaryPrimitives.ReverseEndianness(result.unknownData1);
                result.unknownData2 = BinaryPrimitives.ReverseEndianness(result.unknownData2);
                result.unknownData3 = BinaryPrimitives.ReverseEndianness(result.unknownData3);
                result.unknownData4 = BinaryPrimitives.ReverseEndianness(result.unknownData4);
                result.unknownPtr1 = BinaryPrimitives.ReverseEndianness(result.unknownPtr1);
                result.unknownPtr2 = BinaryPrimitives.ReverseEndianness(result.unknownPtr2);
                result.unknownPtr3 = BinaryPrimitives.ReverseEndianness(result.unknownPtr3);
                result.unknownPtr4 = BinaryPrimitives.ReverseEndianness(result.unknownPtr4);
                result.unknownPtr5 = BinaryPrimitives.ReverseEndianness(result.unknownPtr5);
                result.unknownPtr6 = BinaryPrimitives.ReverseEndianness(result.unknownPtr6);
                result.unknownData5 = BinaryPrimitives.ReverseEndianness(result.unknownData5);
                result.dataPtr = BinaryPrimitives.ReverseEndianness(result.dataPtr);
                result.dataSize = BinaryPrimitives.ReverseEndianness(result.dataSize);
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
