
namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Buffers.Binary;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.DataStructures;
    using Twilight.Engine.Memory;
    using Twilight.Source.Main;

    public class RawActorSlotsTableEntry : INotifyPropertyChanged
    {

        public String Name
        {
            get
            {
                return this.name ?? "";
            }

            set
            {
                byte[] data = Encoding.ASCII.GetBytes(value);
                Array.Resize(ref data, 12);

                if (this.name == null && name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged(nameof(this.Name));
                    MemoryWriter.Instance.WriteBytes(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x0,
                        data
                    );
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
                    MemoryWriter.Instance.Write<UInt16>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0xC,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
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

        public UInt32 MDMCommandPtr
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x10,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MArchivePtr
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x14,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 HeapPtr
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x18,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MDataHeapPtr
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x1C,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public UInt32 MResPtrPtr
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
                    MemoryWriter.Instance.Write<UInt32>(
                        SessionManager.Session.OpenedProcess,
                        GetSlotAddress() + (UInt64)this.ActorSlotIndex * 0x24 + 0x20,
                        BinaryPrimitives.ReverseEndianness(value)
                    );
                }
            }
        }

        public String name; // 0x0

        public UInt16 referenceCount; // 0xC

        public UInt16 padding; // 0xE

        public UInt32 mDMCommandPtr; // 0x10

        public UInt32 mArchivePtr; // 0x14

        public UInt32 heapPtr; // 0x18

        public UInt32 mDataHeapPtr; // 0x1C

        public UInt32 mResPtrPtr; // 0x20

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

        UInt64 GetSlotAddress()
        {
            UInt64 gameCubeMemoryBase = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            switch (MainViewModel.GetInstance().DetectedVersion)
            {
                default:
                case EDetectedVersion.GC_En: return gameCubeMemoryBase + ActorReferenceCountTableConstants.ActorReferenceTableBaseGcEn;
                case EDetectedVersion.Wii_En_1_0: return gameCubeMemoryBase + ActorReferenceCountTableConstants.ActorReferenceTableBaseWii_1_0;
                case EDetectedVersion.Wii_En_1_2: return gameCubeMemoryBase + ActorReferenceCountTableConstants.ActorReferenceTableBaseWii_1_2;
            }
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

                // Note: assignments below bypass setters to avoid write-backs to memory
                unsafe
                {
                    fixed (byte* byteRef = &bytes[index * StructSize])
                    {
                        this.rawActorSlots[index].name = Encoding.ASCII.GetString(byteRef, 0xC);
                    }
                }

                this.rawActorSlots[index].referenceCount = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes, index * StructSize + 0xC));
                this.rawActorSlots[index].padding = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes, index * StructSize + 0xE));
                this.rawActorSlots[index].mDMCommandPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x10));
                this.rawActorSlots[index].mArchivePtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x14));
                this.rawActorSlots[index].heapPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x18));

                this.rawActorSlots[index].mDataHeapPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x1C));
                this.rawActorSlots[index].mResPtrPtr = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes, index * StructSize + 0x20));
                this.rawActorSlots[index].ActorSlotIndex = index;

                this.rawActorSlots[index].Refresh();
            }
        }
    }
    //// End class
}
//// End namespace
