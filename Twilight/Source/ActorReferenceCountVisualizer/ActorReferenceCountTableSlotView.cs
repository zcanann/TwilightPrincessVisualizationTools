
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using GalaSoft.MvvmLight.Command;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Twilight.Engine.Common;
    using Twilight.Engine.Memory;

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
                Array.Resize(ref data, 12);

                if (this.Slot.name == null || !Enumerable.SequenceEqual(this.Slot.name, data))
                {
                    this.Slot.name = data;
                    this.RaisePropertyChanged(nameof(this.Name));
                    MemoryWriter.Instance.WriteBytes(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        data
                    );
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
                    MemoryWriter.Instance.Write<UInt16>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0xC + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x10 + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x14 + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x18 + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x1C + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x20 + (UInt64)this.Slot.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public void RefreshAllProperties()
        {
            this.RaisePropertyChanged(nameof(this.Name));
            this.RaisePropertyChanged(nameof(this.ReferenceCount));
            this.RaisePropertyChanged(nameof(this.Padding));
            this.RaisePropertyChanged(nameof(this.MDMCommandPtr));
            this.RaisePropertyChanged(nameof(this.MArchivePtr));
            this.RaisePropertyChanged(nameof(this.HeapPtr));
            this.RaisePropertyChanged(nameof(this.MDataHeapPtr));
            this.RaisePropertyChanged(nameof(this.MResPtrPtr));
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
