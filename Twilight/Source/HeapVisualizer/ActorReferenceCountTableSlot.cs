
namespace Twilight.Source.HeapVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x24)]
    public class ActorReferenceCountTableSlot : INotifyPropertyChanged
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        private String name;

        [MarshalAs(UnmanagedType.I2)]
        private UInt16 referenceCount;

        [MarshalAs(UnmanagedType.I2)]
        private UInt16 padding;

        [MarshalAs(UnmanagedType.I4)]
        private Int32 mDMCommandPtr;

        [MarshalAs(UnmanagedType.I4)]
        private Int32 mArchivePtr;

        [MarshalAs(UnmanagedType.I4)]
        private Int32 heapPtr;

        [MarshalAs(UnmanagedType.I4)]
        private Int32 mDataHeapPtr;

        [MarshalAs(UnmanagedType.I4)]
        private Int32 mResPtrPtr;

        public static ActorReferenceCountTableSlot FromByteArray(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                ActorReferenceCountTableSlot result = (ActorReferenceCountTableSlot)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ActorReferenceCountTableSlot));

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

        public event PropertyChangedEventHandler PropertyChanged;

        public String Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged(nameof(this.Name));
                }
            }
        }

        public UInt16 ReferenceCount
        {
            get
            {
                return this.referenceCount;
            }

            set
            {
                if (this.referenceCount != value)
                {
                    this.referenceCount = value;
                    this.RaisePropertyChanged(nameof(this.ReferenceCount));
                }
            }
        }

        public UInt16 Padding
        {
            get
            {
                return this.padding;
            }

            set
            {
                if (this.padding != value)
                {
                    this.padding = value;
                    this.RaisePropertyChanged(nameof(this.Padding));
                }
            }
        }

        public Int32 MDMCommandPtr
        {
            get
            {
                return this.mDMCommandPtr;
            }

            set
            {
                if (this.mDMCommandPtr != value)
                {
                    this.mDMCommandPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDMCommandPtr));
                }
            }
        }

        public Int32 MArchivePtr
        {
            get
            {
                return this.mArchivePtr;
            }

            set
            {
                if (this.mArchivePtr != value)
                {
                    this.mArchivePtr = value;
                    this.RaisePropertyChanged(nameof(this.MArchivePtr));
                }
            }
        }

        public Int32 HeapPtr
        {
            get
            {
                return this.heapPtr;
            }

            set
            {
                if (this.heapPtr != value)
                {
                    this.heapPtr = value;
                    this.RaisePropertyChanged(nameof(this.HeapPtr));
                }
            }
        }

        public Int32 MDataHeapPtr
        {
            get
            {
                return this.mDataHeapPtr;
            }

            set
            {
                if (this.mDataHeapPtr != value)
                {
                    this.mDataHeapPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDataHeapPtr));
                }
            }
        }

        public Int32 MResPtrPtr
        {
            get
            {
                return this.mResPtrPtr;
            }

            set
            {
                if (this.mResPtrPtr != value)
                {
                    this.mResPtrPtr = value;
                    this.RaisePropertyChanged(nameof(this.MResPtrPtr));
                }
            }
        }

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace
