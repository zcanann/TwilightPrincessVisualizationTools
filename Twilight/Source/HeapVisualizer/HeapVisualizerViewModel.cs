namespace Twilight.Source.HeapVisualizer
{
    using GalaSoft.MvvmLight.Command;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.Logging;
    using Twilight.Engine.Memory;
    using Twilight.Source.ActorReferenceCountVisualizer;
    using Twilight.Source.Docking;
    using Twilight.Source.Main;

    /// <summary>
    /// View model for the Heap Visualizer.
    /// </summary>
    public class HeapVisualizerViewModel : ToolViewModel
    {
        private static readonly UInt32 HeapTableBaseGc = 0x803A2EF4;
        private static readonly UInt32 HeapTableBaseWii1_0 = 0x803F5850;
        private static readonly UInt32 HeapTableBaseWii1_2 = 0x803F5850;

        private static readonly List<Tuple<UInt32, UInt32>> HeapsGc = new List<Tuple<UInt32, UInt32>>
        {
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // Root
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // System
            Tuple.Create<UInt32, UInt32>(0x80502400, 0x80a2ae0c),   // Zelda
            Tuple.Create<UInt32, UInt32>(0x81399ad0, 0x817e7ad0),   // Game
            Tuple.Create<UInt32, UInt32>(0x80a3d6b0, 0x8131cab0),   // Archive
            Tuple.Create<UInt32, UInt32>(0x8131cac0, 0x81399ac0),   // J2D
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // HostIO
            Tuple.Create<UInt32, UInt32>(0x80a3c6a0, 0x80a3d6a0),   // Command
        };

        private static readonly List<Tuple<UInt32, UInt32>> HeapsWii1_0 = new List<Tuple<UInt32, UInt32>>
        {
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // Root
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // System
            Tuple.Create<UInt32, UInt32>(0x80607820, 0x80f06f8c),   // Zelda
            Tuple.Create<UInt32, UInt32>(0x81099840, 0x817e7840),   // Game
            Tuple.Create<UInt32, UInt32>(0x911000a0, 0x92020ca0),   // Archive
            Tuple.Create<UInt32, UInt32>(0x92020cb0, 0x920dc4b0),   // J2D
            Tuple.Create<UInt32, UInt32>(0x80f18820, 0x80f19820),   // Command
            Tuple.Create<UInt32, UInt32>(0x80f19830, 0x81099830),   // Dynamic Link
        };

        private static readonly List<Tuple<UInt32, UInt32>> HeapsWii1_2 = new List<Tuple<UInt32, UInt32>>
        {
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // Root
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // System
            Tuple.Create<UInt32, UInt32>(0x805ed8a0, 0x80f06d4c),   // Zelda
            Tuple.Create<UInt32, UInt32>(0x81099600, 0x817e7600),   // Game
            Tuple.Create<UInt32, UInt32>(0x911000a0, 0x92020ca0),   // Archive
            Tuple.Create<UInt32, UInt32>(0x92020cb0, 0x920dc4b0),   // J2D
            Tuple.Create<UInt32, UInt32>(0x00000000, 0x00000000),   // Command
            Tuple.Create<UInt32, UInt32>(0x80f195f0, 0x810995f0),   // Dynamic Link
        };
        private static readonly Int32 HeapCount = 8;

        private static readonly Int32 HeapImageWidth = 8192;
        private static readonly Int32 HeapImageHeight = 1;
        private static readonly Int32 DPI = 72;

        public enum HeapVisualizationOption
        {
            CMem,
            NonZeroMemory,
        }

        /// <summary>
        /// Singleton instance of the <see cref="HeapVisualizerViewModel" /> class.
        /// </summary>
        private static HeapVisualizerViewModel heapVisualizerViewModelInstance = new HeapVisualizerViewModel();

        private HeapCheck[] cachedHeapInfo = new HeapCheck[HeapCount];

        /// <summary>
        /// Prevents a default instance of the <see cref="HeapVisualizerViewModel" /> class from being created.
        /// </summary>
        private HeapVisualizerViewModel() : base("Heap Visualizer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.HeapBitmaps = new List<WriteableBitmap>();
            this.HeapNames = new List<String>();
            this.BlockUsage = new List<String>();
            this.MemoryUsage = new List<String>();
            this.HeapBitmapBuffers = new List<Byte[]>();

            try
            {
                for (int heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                {
                    WriteableBitmap HeapBitmap = new WriteableBitmap(HeapImageWidth, HeapImageHeight, DPI, DPI, PixelFormats.Bgr24, null);
                    Byte[] HeapBuffer = new Byte[HeapBitmap.BackBufferStride * HeapImageHeight];

                    this.HeapBitmaps.Add(HeapBitmap);
                    this.HeapBitmapBuffers.Add(HeapBuffer);
                    this.HeapNames.Add("<unknown>");
                    this.BlockUsage.Add("-");
                    this.MemoryUsage.Add("-");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error initializing visualization bitmap", ex);
            }

            Application.Current.Exit += this.OnAppExit;

            this.RunUpdateLoop();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            this.CanUpdate = false;
        }

        /// <summary>
        /// Gets the list of heap visualization bitmaps.
        /// </summary>
        public List<WriteableBitmap> HeapBitmaps { get; private set; }

        /// <summary>
        /// Gets the list of heap visualization options.
        /// </summary>
        public IEnumerable<HeapVisualizationOption> HeapVisualizationOptionList
        {
            get
            {
                return Enum.GetValues(typeof(HeapVisualizationOption)).Cast<HeapVisualizationOption>();
            }
        }

        /// <summary>
        /// Gets or sets the selected heap visualization option.
        /// </summary>
        public HeapVisualizationOption SelectedHeapVisualizationOption { get; set; }

        public ICommand DisallowComboboxPreviewItemCommand
        {
            get
            {
                return new RelayCommand<ComboBox>((combobox) =>
                {
                    if (combobox != null)
                    {
                        combobox.SelectedIndex = -1;
                        combobox.SelectedItem = null;
                    }
                });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap0
        {
            get
            {
                return Properties.Settings.Default.ShowHeap0;
            }

            set
            {
                Properties.Settings.Default.ShowHeap0 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap0));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap1
        {
            get
            {
                return Properties.Settings.Default.ShowHeap1;
            }

            set
            {
                Properties.Settings.Default.ShowHeap1 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap1));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap2
        {
            get
            {
                return Properties.Settings.Default.ShowHeap2;
            }

            set
            {
                Properties.Settings.Default.ShowHeap2 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap2));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap3
        {
            get
            {
                return Properties.Settings.Default.ShowHeap3;
            }

            set
            {
                Properties.Settings.Default.ShowHeap3 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap3));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap4
        {
            get
            {
                return Properties.Settings.Default.ShowHeap4;
            }

            set
            {
                Properties.Settings.Default.ShowHeap4 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap4));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap5
        {
            get
            {
                return Properties.Settings.Default.ShowHeap5;
            }

            set
            {
                Properties.Settings.Default.ShowHeap5 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap5));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap6
        {
            get
            {
                return Properties.Settings.Default.ShowHeap6;
            }

            set
            {
                Properties.Settings.Default.ShowHeap6 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap6));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the corresponding heap.
        /// </summary>
        public Boolean ShowHeap7
        {
            get
            {
                return Properties.Settings.Default.ShowHeap0;
            }

            set
            {
                Properties.Settings.Default.ShowHeap7 = value;
                Properties.Settings.Default.Save();
                this.RaisePropertyChanged(nameof(this.ShowHeap7));
            }
        }

        /// <summary>
        /// Gets the list of heap usage display strings.
        /// </summary>
        public List<String> BlockUsage { get; private set; }

        /// <summary>
        /// Gets the list of heap usage display strings.
        /// </summary>
        public List<String> MemoryUsage { get; private set; }

        /// <summary>
        /// Gets the list of buffers used to update the visualization bitmaps.
        /// </summary>
        public List<Byte[]> HeapBitmapBuffers { get; private set; }

        /// <summary>
        /// Gets the list of heap names.
        /// </summary>
        public List<String> HeapNames { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the heap visualizer update loop can run.
        /// </summary>
        private bool CanUpdate { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="HeapVisualizerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static HeapVisualizerViewModel GetInstance()
        {
            return HeapVisualizerViewModel.heapVisualizerViewModelInstance;
        }

        public UInt32 GetHeapTableBase()
        {
            switch(MainViewModel.GetInstance().DetectedVersion)
            {
                default:
                case EDetectedVersion.GC_En: return HeapTableBaseGc;
                case EDetectedVersion.Wii_En_1_0: return HeapTableBaseWii1_0;
            }
        }

        public Int32 GetHeapCheckSize()
        {
            switch (MainViewModel.GetInstance().DetectedVersion)
            {
                default:
                case EDetectedVersion.GC_En: return typeof(HeapCheck).StructLayoutAttribute.Size;
                case EDetectedVersion.Wii_En_1_0: return typeof(HeapCheck).StructLayoutAttribute.Size - 4;
            }
        }

        public UInt32 GetHeapStart(Int32 heapIndex)
        {
            heapIndex = Math.Clamp(heapIndex, 0, HeapCount - 1);

            switch (MainViewModel.GetInstance().DetectedVersion)
            {
                default:
                case EDetectedVersion.GC_En: return HeapsGc[heapIndex].Item1;
                case EDetectedVersion.Wii_En_1_0: return HeapsWii1_0[heapIndex].Item1;
            }
        }

        public UInt32 GetHeapSize(Int32 heapIndex)
        {
            heapIndex = Math.Clamp(heapIndex, 0, HeapCount - 1);

            switch (MainViewModel.GetInstance().DetectedVersion)
            {
                default:
                case EDetectedVersion.GC_En: return HeapsGc[heapIndex].Item2 - this.GetHeapStart(heapIndex);
                case EDetectedVersion.Wii_En_1_0: return HeapsWii1_0[heapIndex].Item2 - this.GetHeapStart(heapIndex);
            }
        }

        /// <summary>
        /// Begin the update loop for visualizing the heap.
        /// </summary>
        private void RunUpdateLoop()
        {
            this.CanUpdate = true;

            Task.Run(async () =>
            {
                while (this.CanUpdate)
                {
                    if (this.IsVisible)
                    {
                        try
                        {
                            if (SessionManager.Session.OpenedProcess != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    UpdateHeaps();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(LogLevel.Error, "Error updating the Heap Visualizer", ex);
                        }
                    }

                    await Task.Delay(250);
                }
            });
        }

        private void UpdateHeaps()
        {
            // Clear out old visual data
            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
            {
                Array.Clear(this.HeapBitmapBuffers[heapIndex], 0, this.HeapBitmapBuffers[heapIndex].Length);
            }

            this.ReadAndCacheHeaps();

            switch (this.SelectedHeapVisualizationOption)
            {
                case HeapVisualizationOption.CMem:
                    this.BuildHeapVisualizationsFromCMem();
                    // this.BuildActorReferenceVisualizations();
                    break;
                case HeapVisualizationOption.NonZeroMemory:
                    this.BuildHeapVisualizationsFromNonZeroMemory();
                    break;
            }

            for (Int32 heapIndex = 0; heapIndex < this.HeapBitmaps.Count; heapIndex++)
            {
                Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
                this.HeapBitmaps[heapIndex].WritePixels(
                    new Int32Rect(0, 0, HeapImageWidth, HeapImageHeight),
                    this.HeapBitmapBuffers[heapIndex],
                    this.HeapBitmaps[heapIndex].PixelWidth * bytesPerPixel,
                    0
                );
            }
        }

        private void ReadAndCacheHeaps()
        {
            UInt64 gameCubeMemoryBase = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            Int32 heapCheckSize = this.GetHeapCheckSize();
            bool success;
            byte[] heapTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                gameCubeMemoryBase + (GetHeapTableBase() - 0x80000000),
                sizeof(UInt32) * HeapCount,
                out success);

            if (!success)
            {
                return;
            }

            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
            {
                byte[] heapPointerBytes = new byte[4];
                Array.Copy(heapTable, heapIndex * sizeof(UInt32), heapPointerBytes, 0, sizeof(UInt32));

                UInt32 heapPointer = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(heapPointerBytes));

                byte[] heapCheckBytes = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    gameCubeMemoryBase + (heapPointer - 0x80000000),
                    heapCheckSize,
                    out success);

                if (!success)
                {
                    continue;
                }

                HeapCheck heapCheck = HeapCheck.FromByteArray(heapCheckBytes);
                this.cachedHeapInfo[heapIndex] = HeapCheck.FromByteArray(heapCheckBytes);

                const Int32 maxNameSize = 12;
                byte[] heapNameBytes = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    gameCubeMemoryBase + (heapCheck.mNamePointer - 0x80000000),
                    maxNameSize,
                    out success);

                Int32 nameLength = Array.IndexOf(heapNameBytes, (byte)0);
                String newName = Encoding.UTF8.GetString(heapNameBytes, 0, nameLength <= 0 ? maxNameSize : nameLength);

                if (newName != this.HeapNames[heapIndex])
                {
                    this.HeapNames[heapIndex] = newName;
                    this.RaisePropertyChanged(nameof(this.HeapNames));
                }
            }
        }

        private void BuildHeapVisualizationsFromNonZeroMemory()
        {
            UInt64 gameCubeMemoryBase = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
            {
                HeapCheck heap = this.cachedHeapInfo[heapIndex];

                // Arbitrary size cutoff
                if (heap == null || heap.heapSize > 1073741824)
                {
                    continue;
                }

                bool success;
                byte[] fullHeapData = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    gameCubeMemoryBase + (heap.heapPointer - 0x80000000),
                    (Int32)heap.heapSize,
                    out success);

                if (success)
                {
                    this.ColorHeapSlotMemory(fullHeapData, heapIndex, Color.FromRgb(255, 0, 0));
                }
            }
        }

        /*
        private byte[] slotData = new byte[ActorReferenceCountTableConstants.ActorSlotStructSize];

        /// <summary>
        /// Experimental idea that fails since we only have heap pointers, but not heap sizes
        /// </summary>
        private void BuildActorReferenceVisualizations()
        {
            // Read the entire actor reference counting table
            bool success;
            byte[] actorReferenceCountTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceCountTableConstants.GetActorReferenceTableSize(), EmulatorType.Dolphin),
                ActorReferenceCountTableConstants.ActorSlotStructSize * ActorReferenceCountTableConstants.ActorReferenceCountTableMaxEntries,
                out success);

            // Update new data / visual data
            if (!success)
            {
                return;
            }

            for (int actorSlotIndex = 0; actorSlotIndex < ActorReferenceCountTableConstants.ActorReferenceCountTableMaxEntries; actorSlotIndex++)
            {
                Array.Copy(actorReferenceCountTable, actorSlotIndex * ActorReferenceCountTableConstants.ActorSlotStructSize, slotData, 0, ActorReferenceCountTableConstants.ActorSlotStructSize);
                ActorReferenceCountTableSlot result = ActorReferenceCountTableSlot.FromByteArray(slotData, actorSlotIndex);

                if (result == null)
                {
                    continue;
                }

                Stack<UInt32> heapRefs = new Stack<UInt32>();

                heapRefs.Push(result.heapPtr);
                heapRefs.Push(result.mArchivePtr);
                heapRefs.Push(result.mResPtrPtr);

                while(heapRefs.Count > 0)
                {
                    UInt32 pointer = heapRefs.Pop();

                    if (pointer <= 0)
                    {
                        continue;
                    }

                    for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                    {
                        if (this.cachedHeapInfo[heapIndex] != null && pointer >= this.cachedHeapInfo[heapIndex].heapPointer && pointer < this.cachedHeapInfo[heapIndex].heapPointer + this.cachedHeapInfo[heapIndex].heapSize)
                        {
                            const Int32 drawSize = 4096;
                            this.ColorHeapSlotMemory(pointer, drawSize, this.GetHeapStart(heapIndex), this.GetHeapSize(heapIndex), heapIndex, Color.FromRgb(0, 255, 0), true);
                        }
                    }
                }
            }
        }
        */

        /// <summary>
        /// Experimental, no idea what I am doing here.
        /// </summary>
        private void BuildHeapVisualizationsFromJkrHeaps()
        {
            UInt64 gameCubeMemoryBase = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
            {
                HeapCheck heap = this.cachedHeapInfo[heapIndex];

                if (heap == null)
                {
                    continue;
                }

                bool success;
                byte[] jkrExpHeapData = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    gameCubeMemoryBase + (heap.heapPointer - 0x80000000),
                    typeof(JKRExpHeap).StructLayoutAttribute.Size,
                    out success);

                if (!success)
                {
                    continue;
                }

                JKRExpHeap jkrExpHeap = JKRExpHeap.FromByteArray(jkrExpHeapData);
                Queue<UInt32> heapPointerQueue = new Queue<UInt32>();

                heapPointerQueue.Enqueue(jkrExpHeap.jkrHeapPtr);

                while (heapPointerQueue.Count > 0)
                {
                    UInt32 nextStackPointer = heapPointerQueue.Dequeue();

                    if (nextStackPointer <= 0)
                    {
                        continue;
                    }

                    byte[] jkrHeapData = MemoryReader.Instance.ReadBytes(
                        SessionManager.Session.OpenedProcess,
                        gameCubeMemoryBase + (nextStackPointer - 0x80000000),
                        typeof(JKRHeap).StructLayoutAttribute.Size,
                        out success);

                    if (success)
                    {
                        JKRHeap jkrHeap = JKRHeap.FromByteArray(jkrHeapData);

                        this.ColorHeapSlotMemory(jkrHeap.startPtr, jkrHeap.endPtr - jkrHeap.startPtr, heap.heapPointer, heap.heapSize, heapIndex, Color.FromRgb(255, 0, 0));

                        heapPointerQueue.Enqueue(jkrHeap.childTreeLinkPtr1);
                        heapPointerQueue.Enqueue(jkrHeap.childTreeLinkPtr2);
                        heapPointerQueue.Enqueue(jkrHeap.childTreeLinkPtr3);
                        heapPointerQueue.Enqueue(jkrHeap.childTreeLinkPtr4);

                        heapPointerQueue.Enqueue(jkrHeap.childTreeListPtr1);
                        heapPointerQueue.Enqueue(jkrHeap.childTreeListPtr2);
                        heapPointerQueue.Enqueue(jkrHeap.childTreeListPtr3);

                        heapPointerQueue.Enqueue(jkrHeap.jsuPtrLink1);
                        heapPointerQueue.Enqueue(jkrHeap.jsuPtrLink2);
                        heapPointerQueue.Enqueue(jkrHeap.jsuPtrLink3);
                        heapPointerQueue.Enqueue(jkrHeap.jsuPtrLink4);
                    }
                }
            }
        }

        private void BuildHeapVisualizationsFromCMem()
        {
            UInt64 gameCubeMemoryBase = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
            {
                HeapCheck heap = this.cachedHeapInfo[heapIndex];

                if (heap == null)
                {
                    continue;
                }

                bool success;
                byte[] jkrExpHeapData = MemoryReader.Instance.ReadBytes(
                    SessionManager.Session.OpenedProcess,
                    gameCubeMemoryBase + (heap.heapPointer - 0x80000000),
                    typeof(JKRExpHeap).StructLayoutAttribute.Size,
                    out success);

                if (success)
                {
                    JKRExpHeap jkrExpHeap = JKRExpHeap.FromByteArray(jkrExpHeapData);

                    UInt32 usedMemory = 0;
                    UInt32 freeMemory = 0;
                    UInt32 usedBlocks = 0;
                    UInt32 freeBlocks = 0;

                    UInt32 nextCMemBlockPtr = jkrExpHeap.usedBlocksHeadPtr;
                    HashSet<UInt32> CycleDetect = new HashSet<UInt32>();
                    while (nextCMemBlockPtr > 0)
                    {
                        if (CycleDetect.Contains(nextCMemBlockPtr))
                        {
                            break;
                        }

                        CycleDetect.Add(nextCMemBlockPtr);

                        byte[] cMemBlockData = MemoryReader.Instance.ReadBytes(
                            SessionManager.Session.OpenedProcess,
                            gameCubeMemoryBase + (nextCMemBlockPtr - 0x80000000),
                            typeof(CMemBlock).StructLayoutAttribute.Size,
                            out success);

                        if (success)
                        {
                            CMemBlock cMemBlock = CMemBlock.FromByteArray(cMemBlockData);
                            usedBlocks++;
                            usedMemory += cMemBlock.blockSize;
                            nextCMemBlockPtr = cMemBlock.nextPtr;

                            // Color used memory
                            this.ColorHeapSlotMemory(nextCMemBlockPtr, cMemBlock.blockSize, this.GetHeapStart(heapIndex), this.GetHeapSize(heapIndex), heapIndex, Color.FromRgb(255, 0, 0));
                        }
                    }

                    CycleDetect.Clear();
                    nextCMemBlockPtr = jkrExpHeap.freeBlocksHeadPtr;
                    while (nextCMemBlockPtr > 0)
                    {
                        if (CycleDetect.Contains(nextCMemBlockPtr))
                        {
                            break;
                        }

                        CycleDetect.Add(nextCMemBlockPtr);

                        byte[] cMemBlockData = MemoryReader.Instance.ReadBytes(
                            SessionManager.Session.OpenedProcess,
                            gameCubeMemoryBase + (nextCMemBlockPtr - 0x80000000),
                            typeof(CMemBlock).StructLayoutAttribute.Size,
                            out success);

                        if (success)
                        {
                            CMemBlock cMemBlock = CMemBlock.FromByteArray(cMemBlockData);
                            freeBlocks++;
                            freeMemory += cMemBlock.blockSize;
                            nextCMemBlockPtr = cMemBlock.nextPtr;

                            this.ColorHeapSlotMemory(nextCMemBlockPtr, cMemBlock.blockSize, this.GetHeapStart(heapIndex), this.GetHeapSize(heapIndex), heapIndex, Color.FromRgb(0, 0, 255));
                        }
                    }

                    String blockUsage = String.Format("Blocks: {0} / {1}", usedBlocks, usedBlocks + freeBlocks);
                    String memoryUsage;

                    if (usedMemory + freeMemory < 65535)
                    {
                        memoryUsage = String.Format("Mem (b): {0} / {1} (b)", usedMemory, this.GetHeapSize(heapIndex)); // usedMemory + freeMemory
                    }
                    else
                    {
                        memoryUsage = String.Format("Mem: {0} / {1} (kb)", usedMemory / 1024, this.GetHeapSize(heapIndex) / 1024);
                    }


                    if (blockUsage != this.BlockUsage[heapIndex])
                    {
                        this.BlockUsage[heapIndex] = blockUsage;
                        this.RaisePropertyChanged(nameof(this.BlockUsage));
                    }

                    if (memoryUsage != this.MemoryUsage[heapIndex])
                    {
                        this.MemoryUsage[heapIndex] = memoryUsage;
                        this.RaisePropertyChanged(nameof(this.MemoryUsage));
                    }
                }
            }
        }

        /// <summary>
        /// A bone-headed approach to heap visualization that looks for non-zeroed data.
        /// This will not be very accurate, especially if the game does not zero freed memory.
        /// </summary>
        private void ColorHeapSlotMemory(byte[] rawData, Int32 heapIndex, Color color)
        {
            if (rawData == null || rawData.Length == 0)
            {
                return;
            }

            Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
            double addressToByteRatio = (double)rawData.Length / (double)HeapImageWidth;

            for (Int32 index = 0; index < HeapImageWidth; index++)
            {
                bool shouldColor = false;

                Int32 memoryIndexStart = Math.Min((Int32)(index * addressToByteRatio), rawData.Length);
                Int32 memoryIndexEnd = Math.Min((Int32)((index + 1) * addressToByteRatio), rawData.Length);

                for (Int32 memoryIndex = memoryIndexStart; memoryIndex < memoryIndexEnd; memoryIndex++)
                {
                    shouldColor |= (rawData[memoryIndex] != 0);
                }

                if (shouldColor)
                {
                    this.HeapBitmapBuffers[heapIndex][index * bytesPerPixel] |= color.B;
                    this.HeapBitmapBuffers[heapIndex][index * bytesPerPixel + 1] |= color.G;
                    this.HeapBitmapBuffers[heapIndex][index * bytesPerPixel + 2] |= color.R;
                }
            }
        }

        private void ColorHeapSlotMemory(UInt32 startAddress, UInt32 dataSize, UInt32 heapStartAddress, UInt32 heapSize, Int32 heapIndex, Color color, bool overWrite = false)
        {
            if (heapSize == 0)
            {
                return;
            }

            Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
            UInt32 offset = startAddress - heapStartAddress;
            Int32 pixelStart = (Int32)((double)offset * (double)HeapImageWidth / (double)heapSize);
            Int32 pixelEnd = (Int32)((double)(offset + dataSize) * (double)HeapImageWidth / (double)heapSize);

            // TODO: Reenable logs once the heap visualization is fixed and the spam is gone.

            if (startAddress < heapStartAddress)
            {
                // Logger.Log(LogLevel.Warn, "Start address for this data block is outside of specified heap range.");
                return;
            }
            else if (startAddress + dataSize > heapStartAddress + heapSize)
            {
                // Logger.Log(LogLevel.Warn, "End address for this data block is outside of specified heap range.");
                return;
            }

            for (Int32 pixelIndex = pixelStart; pixelIndex < pixelEnd; pixelIndex++)
            {
                if (pixelIndex >= HeapImageWidth)
                {
                    // Logger.Log(LogLevel.Warn, "Address out of bitmap range!");
                    return;
                }

                if (overWrite)
                {
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel] = color.B;
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 1] = color.G;
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 2] = color.R;
                }
                else
                {
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel] |= color.B;
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 1] |= color.G;
                    this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 2] |= color.R;
                }
            }
        }
    }
    //// End class
}
//// End namespace