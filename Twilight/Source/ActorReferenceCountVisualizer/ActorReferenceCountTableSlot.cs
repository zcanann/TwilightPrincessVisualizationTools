
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x24)]
    public class ActorReferenceCountTableSlot
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] // 0x0
        public byte[] name;

        [MarshalAs(UnmanagedType.I2)] // 0xC
        public UInt16 referenceCount;

        [MarshalAs(UnmanagedType.I2)] // 0xE
        public UInt16 padding;

        [MarshalAs(UnmanagedType.I4)] // 0x10
        public UInt32 mDMCommandPtr;

        [MarshalAs(UnmanagedType.I4)] // 0x14
        public UInt32 mArchivePtr;

        [MarshalAs(UnmanagedType.I4)] // 0x18
        public UInt32 heapPtr;

        [MarshalAs(UnmanagedType.I4)] // 0x1C
        public UInt32 mDataHeapPtr;

        [MarshalAs(UnmanagedType.I4)] // 0x20
        public UInt32 mResPtrPtr;

        public Int32 ActorSlotIndex { get; set; }

        public static ActorReferenceCountTableSlot FromByteArray(byte[] bytes, int actorSlotIndex)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            if (handle.IsAllocated)
            {
                try
                {
                    ActorReferenceCountTableSlot result = Marshal.PtrToStructure<ActorReferenceCountTableSlot>(handle.AddrOfPinnedObject());

                    result.ActorSlotIndex = actorSlotIndex;

                    // Fix GC endianness
                    result.referenceCount = BinaryPrimitives.ReverseEndianness(result.referenceCount);
                    result.padding = BinaryPrimitives.ReverseEndianness(result.padding);
                    result.mDMCommandPtr = BinaryPrimitives.ReverseEndianness(result.mDMCommandPtr);
                    result.mArchivePtr = BinaryPrimitives.ReverseEndianness(result.mArchivePtr);
                    result.heapPtr = BinaryPrimitives.ReverseEndianness(result.heapPtr);
                    result.mDataHeapPtr = BinaryPrimitives.ReverseEndianness(result.mDataHeapPtr);
                    result.mResPtrPtr = BinaryPrimitives.ReverseEndianness(result.mResPtrPtr);

                    return result;
                }
                finally
                {
                    handle.Free();
                }
            }

            return null;
        }
    }
    //// End class
}
//// End namespace
