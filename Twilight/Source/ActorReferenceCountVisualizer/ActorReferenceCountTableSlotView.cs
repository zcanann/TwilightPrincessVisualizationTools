
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public class ActorReferenceCountTableSlotView : INotifyPropertyChanged
    {
        public ActorReferenceCountTableSlotView(ActorReferenceCountTableSlot slot)
        {
            this.Slot = slot;
        }

        public ActorReferenceCountTableSlot Slot { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public String Name
        {
            get
            {
                return this.Slot.name != null ? Encoding.ASCII.GetString(this.Slot.name) : "";
            }

            set
            {
                byte[] data = Encoding.ASCII.GetBytes(value);
                if (!Array.Equals(this.Slot.name, data))
                {
                    this.Slot.name = data;
                    this.RaisePropertyChanged(nameof(this.Name));
                }
            }
        }

        public UInt16 ReferenceCount
        {
            get
            {
                return this.Slot.referenceCount;
            }

            set
            {
                if (this.Slot.referenceCount != value)
                {
                    this.Slot.referenceCount = value;
                    this.RaisePropertyChanged(nameof(this.ReferenceCount));
                }
            }
        }

        public UInt16 Padding
        {
            get
            {
                return this.Slot.padding;
            }

            set
            {
                if (this.Slot.padding != value)
                {
                    this.Slot.padding = value;
                    this.RaisePropertyChanged(nameof(this.Padding));
                }
            }
        }

        public UInt32 MDMCommandPtr
        {
            get
            {
                return this.Slot.mDMCommandPtr;
            }

            set
            {
                if (this.Slot.mDMCommandPtr != value)
                {
                    this.Slot.mDMCommandPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDMCommandPtr));
                }
            }
        }

        public UInt32 MArchivePtr
        {
            get
            {
                return this.Slot.mArchivePtr;
            }

            set
            {
                if (this.Slot.mArchivePtr != value)
                {
                    this.Slot.mArchivePtr = value;
                    this.RaisePropertyChanged(nameof(this.MArchivePtr));
                }
            }
        }

        public UInt32 HeapPtr
        {
            get
            {
                return this.Slot.heapPtr;
            }

            set
            {
                if (this.Slot.heapPtr != value)
                {
                    this.Slot.heapPtr = value;
                    this.RaisePropertyChanged(nameof(this.HeapPtr));
                }
            }
        }

        public UInt32 MDataHeapPtr
        {
            get
            {
                return this.Slot.mDataHeapPtr;
            }

            set
            {
                if (this.Slot.mDataHeapPtr != value)
                {
                    this.Slot.mDataHeapPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDataHeapPtr));
                }
            }
        }

        public UInt32 MResPtrPtr
        {
            get
            {
                return this.Slot.mResPtrPtr;
            }

            set
            {
                if (this.Slot.mResPtrPtr != value)
                {
                    this.Slot.mResPtrPtr = value;
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
