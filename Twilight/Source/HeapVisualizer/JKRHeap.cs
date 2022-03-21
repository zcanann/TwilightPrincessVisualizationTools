
namespace Twilight.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x6C)]
    public class JKRHeap
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
        public UInt32 startPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 endPtr;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 heapSize;

        [MarshalAs(UnmanagedType.I1)]
        public byte debugFill;

        [MarshalAs(UnmanagedType.I1)]
        public byte checkMemoryFilled;

        [MarshalAs(UnmanagedType.I1)]
        public byte allocationMode;

        [MarshalAs(UnmanagedType.I1)]
        public byte groupId;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeListPtr1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeListPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeListPtr3;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeLinkPtr1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeLinkPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeLinkPtr3;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 childTreeLinkPtr4;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 disposerPtr1;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 disposerPtr2;

        [MarshalAs(UnmanagedType.I4)]
        public UInt32 disposerPtr3;

        [MarshalAs(UnmanagedType.I1)]
        public byte errorFlag;

        [MarshalAs(UnmanagedType.I1)]
        public byte initFlag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public Byte[] padding;

        public static JKRHeap FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                JKRHeap result = Marshal.PtrToStructure<JKRHeap>(handle.AddrOfPinnedObject());

                // Fix GC endianness
                result.vTablePtr = BinaryPrimitives.ReverseEndianness(result.vTablePtr);
                result.jkrHeapPtr = BinaryPrimitives.ReverseEndianness(result.jkrHeapPtr);
                result.jsuPtrLink1 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink1);
                result.jsuPtrLink2 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink2);
                result.jsuPtrLink3 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink3);
                result.jsuPtrLink4 = BinaryPrimitives.ReverseEndianness(result.jsuPtrLink4);
                result.startPtr = BinaryPrimitives.ReverseEndianness(result.startPtr);
                result.endPtr = BinaryPrimitives.ReverseEndianness(result.endPtr);
                result.heapSize = BinaryPrimitives.ReverseEndianness(result.heapSize);
                result.childTreeListPtr1 = BinaryPrimitives.ReverseEndianness(result.childTreeListPtr1);
                result.childTreeListPtr2 = BinaryPrimitives.ReverseEndianness(result.childTreeListPtr2);
                result.childTreeListPtr3 = BinaryPrimitives.ReverseEndianness(result.childTreeListPtr3);
                result.childTreeLinkPtr1 = BinaryPrimitives.ReverseEndianness(result.childTreeLinkPtr1);
                result.childTreeLinkPtr2 = BinaryPrimitives.ReverseEndianness(result.childTreeLinkPtr2);
                result.childTreeLinkPtr3 = BinaryPrimitives.ReverseEndianness(result.childTreeLinkPtr3);
                result.childTreeLinkPtr4 = BinaryPrimitives.ReverseEndianness(result.childTreeLinkPtr4);
                result.disposerPtr1 = BinaryPrimitives.ReverseEndianness(result.disposerPtr1);
                result.disposerPtr2 = BinaryPrimitives.ReverseEndianness(result.disposerPtr2);
                result.disposerPtr3 = BinaryPrimitives.ReverseEndianness(result.disposerPtr3);

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
