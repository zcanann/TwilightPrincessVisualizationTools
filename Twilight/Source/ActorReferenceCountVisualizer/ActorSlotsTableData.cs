
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using Twilight.Engine.Common.DataStructures;

    /*
     
        public String Name
        {
            get
            {
                return this.SlotData.name != null ? Encoding.ASCII.GetString(this.SlotData.name) : "";
            }

            set
            {
                byte[] data = Encoding.ASCII.GetBytes(value);
                Array.Resize(ref data, 12);

                if (this.SlotData.name == null || !Enumerable.SequenceEqual(this.SlotData.name, data))
                {
                    this.SlotData.name = data;
                    this.RaisePropertyChanged(nameof(this.Name));
                    MemoryWriter.Instance.WriteBytes(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        data
                    );
                }
            }
        }

        public UInt16 ReferenceCount
        {
            get
            {
                return this.SlotData.referenceCount;
            }

            set
            {
                if (this.SlotData.referenceCount != value)
                {
                    this.SlotData.referenceCount = value;
                    this.RaisePropertyChanged(nameof(this.ReferenceCount));
                    MemoryWriter.Instance.Write<UInt16>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0xC + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt16 Padding
        {
            get
            {
                return this.SlotData.padding;
            }

            set
            {
                if (this.SlotData.padding != value)
                {
                    this.SlotData.padding = value;
                    this.RaisePropertyChanged(nameof(this.Padding));
                }
            }
        }

        public UInt32 MDMCommandPtr
        {
            get
            {
                return this.SlotData.mDMCommandPtr;
            }

            set
            {
                if (this.SlotData.mDMCommandPtr != value)
                {
                    this.SlotData.mDMCommandPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDMCommandPtr));
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x10 + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MArchivePtr
        {
            get
            {
                return this.SlotData.mArchivePtr;
            }

            set
            {
                if (this.SlotData.mArchivePtr != value)
                {
                    this.SlotData.mArchivePtr = value;
                    this.RaisePropertyChanged(nameof(this.MArchivePtr));
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x14 + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 HeapPtr
        {
            get
            {
                return this.SlotData.heapPtr;
            }

            set
            {
                if (this.SlotData.heapPtr != value)
                {
                    this.SlotData.heapPtr = value;
                    this.RaisePropertyChanged(nameof(this.HeapPtr));
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x18 + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MDataHeapPtr
        {
            get
            {
                return this.SlotData.mDataHeapPtr;
            }

            set
            {
                if (this.SlotData.mDataHeapPtr != value)
                {
                    this.SlotData.mDataHeapPtr = value;
                    this.RaisePropertyChanged(nameof(this.MDataHeapPtr));
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x1C + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MResPtrPtr
        {
            get
            {
                return this.SlotData.mResPtrPtr;
            }

            set
            {
                if (this.SlotData.mResPtrPtr != value)
                {
                    this.SlotData.mResPtrPtr = value;
                    this.RaisePropertyChanged(nameof(this.MResPtrPtr));
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize() + 0x20 + (UInt64)this.SlotData.ActorSlotIndex * 0x24, EmulatorType.Dolphin),
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
     */

    public class RawActorSlotsTableEntry : INotifyPropertyChanged
    {
        public String Name { get; set; } // 0x0

        public UInt16 ReferenceCount { get; set; } // 0xC

        public UInt16 Padding { get; set; } // 0xE

        public UInt32 MDMCommandPtr { get; set; } // 0x10

        public UInt32 MArchivePtr { get; set; } // 0x14

        public UInt32 HeapPtr { get; set; } // 0x18

        public UInt32 MDataHeapPtr { get; set; } // 0x1C

        public UInt32 MResPtrPtr { get; set; } // 0x20

        public Int32 ActorSlotIndex { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            this.RaisePropertyChanged(nameof(this.Name));
            this.RaisePropertyChanged(nameof(this.ReferenceCount));
            this.RaisePropertyChanged(nameof(this.Padding));
            this.RaisePropertyChanged(nameof(this.MDMCommandPtr));
            this.RaisePropertyChanged(nameof(this.MArchivePtr));
            this.RaisePropertyChanged(nameof(this.HeapPtr));
            this.RaisePropertyChanged(nameof(this.MDataHeapPtr));
            this.RaisePropertyChanged(nameof(this.MResPtrPtr));
            this.RaisePropertyChanged(nameof(this.MResPtrPtr));
            this.RaisePropertyChanged(nameof(this.ActorSlotIndex));
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

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x24 * 128)]
    public class ActorTableDataSerializable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24 * 128)]
        public Byte[] rawActorTableSlots;
    }

    public class ActorSlotsTableData
    {
        public ActorSlotsTableData()
        {
            this.SerializableData = new ActorTableDataSerializable();
        }

        public ActorTableDataSerializable SerializableData { get; set; }

        public FullyObservableCollection<RawActorSlotsTableEntry> rawActorSlots = new FullyObservableCollection<RawActorSlotsTableEntry>();

        const Int32 StructSize = 0x24;

        public static void Deserialize(ActorSlotsTableData entry, Byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                if (handle.IsAllocated)
                {
                    if (entry.SerializableData == null)
                    {
                        entry.SerializableData = new ActorTableDataSerializable();
                    }

                    Marshal.PtrToStructure<ActorTableDataSerializable>(handle.AddrOfPinnedObject(), entry.SerializableData);
                }
            }
            finally
            {
                handle.Free();
            }
        }

        public void Refresh(Byte[] bytes)
        {
            for (Int32 index = 0; index < bytes.Length / StructSize; index++)
            {
                if (index >= this.rawActorSlots.Count || this.rawActorSlots[index] == null)
                {
                    this.rawActorSlots.Add(new RawActorSlotsTableEntry());
                }

                unsafe
                {
                    fixed (byte* byteRef = &bytes[index * StructSize])
                    {
                        this.rawActorSlots[index].Name = Encoding.ASCII.GetString(byteRef, 0xC);
                    }
                }

                this.rawActorSlots[index].ReferenceCount = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes, index * StructSize + 0xC));
                this.rawActorSlots[index].Padding = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes, index * StructSize + 0xE));
                this.rawActorSlots[index].MDMCommandPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x10));
                this.rawActorSlots[index].MArchivePtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x14));
                this.rawActorSlots[index].HeapPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x18));

                this.rawActorSlots[index].MDataHeapPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x1C));
                this.rawActorSlots[index].MResPtrPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x20));
                this.rawActorSlots[index].ActorSlotIndex = index;

                this.rawActorSlots[index].Refresh();
            }
        }
    }
    //// End class
}
//// End namespace
