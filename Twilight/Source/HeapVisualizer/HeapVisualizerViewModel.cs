namespace Twilight.Source.HeapVisualizer
{
    using GalaSoft.MvvmLight.Command;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.Logging;
    using Twilight.Engine.Memory;
    using Twilight.Source.Docking;

    /// <summary>
    /// View model for the Heap Visualizer.
    /// </summary>
    public class HeapVisualizerViewModel : ToolViewModel
    {
        private static readonly UInt32 HeapTableBase = 0x803D32E0;
        private static readonly Int32 HeapCount = 8;

        private static readonly Int32 HeapImageWidth = 65536;
        private static readonly Int32 HeapImageHeight = 1;
        private static readonly Int32 DPI = 72;

        private static readonly int HeapCheckSize = typeof(HeapCheck).StructLayoutAttribute.Size;

        public enum HeapVisualizationOption
        {
            CMem,
            NonZeroMemory,
        }

        /// <summary>
        /// Singleton instance of the <see cref="HeapVisualizerViewModel" /> class.
        /// </summary>
        private static HeapVisualizerViewModel heapVisualizerViewModelInstance = new HeapVisualizerViewModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="HeapVisualizerViewModel" /> class from being created.
        /// </summary>
        private HeapVisualizerViewModel() : base("Heap Visualizer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.HeapBitmaps = new List<WriteableBitmap>();
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

            switch(this.SelectedHeapVisualizationOption)
            {
                case HeapVisualizationOption.CMem:
                    this.BuildHeapVisualizationsFromCMem();
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

        private void BuildHeapVisualizationsFromNonZeroMemory()
        {
            bool success = false;
            byte[] heapTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, HeapTableBase, EmulatorType.Dolphin),
                HeapCheckSize * HeapCount,
                out success);

            if (success)
            {
                for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                {
                    byte[] heapData = new byte[HeapCheckSize];
                    Array.Copy(heapTable, heapIndex * HeapCheckSize, heapData, 0, HeapCheckSize);
                    HeapCheck heap = HeapCheck.FromByteArray(heapData);

                    // Arbitrary size cutoff
                    if (heap.heapSize > 1073741824)
                    {
                        continue;
                    }

                    byte[] fullHeapData = MemoryReader.Instance.ReadBytes(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, heap.heapPointer, EmulatorType.Dolphin),
                        (Int32)heap.heapSize,
                        out success);

                    if (success)
                    {
                        this.ColorHeapSlotMemory(fullHeapData, heapIndex, Color.FromRgb(255, 0, 0));
                    }
                }
            }
        }

        /// <summary>
        /// Experimental, no idea what I am doing here.
        /// </summary>
        private void BuildHeapVisualizationsFromJkrHeaps()
        {
            bool success = false;
            byte[] heapTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, HeapTableBase, EmulatorType.Dolphin),
                HeapCheckSize * HeapCount,
                out success);

            if (success)
            {
                for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                {
                    byte[] heapData = new byte[HeapCheckSize];
                    Array.Copy(heapTable, heapIndex * HeapCheckSize, heapData, 0, HeapCheckSize);
                    HeapCheck heap = HeapCheck.FromByteArray(heapData);

                    byte[] jkrExpHeapData = MemoryReader.Instance.ReadBytes(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, heap.heapPointer, EmulatorType.Dolphin),
                        typeof(JKRExpHeap).StructLayoutAttribute.Size,
                        out success);

                    if (success)
                    {
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
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, nextStackPointer, EmulatorType.Dolphin),
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
            }
        }

        private void BuildHeapVisualizationsFromCMem()
        {
            bool success = false;
            byte[] heapTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, HeapTableBase, EmulatorType.Dolphin),
                HeapCheckSize * HeapCount,
                out success);

            if (success)
            {
                for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                {
                    byte[] heapData = new byte[HeapCheckSize];
                    Array.Copy(heapTable, heapIndex * HeapCheckSize, heapData, 0, HeapCheckSize);
                    HeapCheck heap = HeapCheck.FromByteArray(heapData);

                    byte[] jkrExpHeapData = MemoryReader.Instance.ReadBytes(
                        SessionManager.Session.OpenedProcess,
                        MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, heap.heapPointer, EmulatorType.Dolphin),
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
                        while (nextCMemBlockPtr > 0)
                        {
                            byte[] cMemBlockData = MemoryReader.Instance.ReadBytes(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, nextCMemBlockPtr, EmulatorType.Dolphin),
                                typeof(CMemBlock).StructLayoutAttribute.Size,
                                out success);

                            if (success)
                            {
                                CMemBlock cMemBlock = CMemBlock.FromByteArray(cMemBlockData);
                                usedBlocks++;
                                usedMemory += cMemBlock.blockSize;
                                nextCMemBlockPtr = cMemBlock.nextPtr;

                                // Color used memory
                                this.ColorHeapSlotMemory(nextCMemBlockPtr, cMemBlock.blockSize, heap.heapPointer, heap.heapSize, heapIndex, Color.FromRgb(255, 0, 0));
                            }
                        }

                        nextCMemBlockPtr = jkrExpHeap.freeBlocksHeadPtr;
                        while (nextCMemBlockPtr > 0)
                        {
                            byte[] cMemBlockData = MemoryReader.Instance.ReadBytes(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, nextCMemBlockPtr, EmulatorType.Dolphin),
                                typeof(CMemBlock).StructLayoutAttribute.Size,
                                out success);

                            if (success)
                            {
                                CMemBlock cMemBlock = CMemBlock.FromByteArray(cMemBlockData);
                                freeBlocks++;
                                freeMemory += cMemBlock.blockSize;
                                nextCMemBlockPtr = cMemBlock.nextPtr;

                                this.ColorHeapSlotMemory(nextCMemBlockPtr, cMemBlock.blockSize, heap.heapPointer, heap.heapSize, heapIndex, Color.FromRgb(0, 0, 255));
                            }
                        }

                        String blockUsage = String.Format("Blocks: {0} / {1}", usedBlocks, usedBlocks + freeBlocks);
                        String memoryUsage;

                        if (usedMemory + freeMemory < 65535)
                        {
                            memoryUsage = String.Format("Mem (b): {0} / {1} (b)", usedMemory, heap.heapSize); // usedMemory + freeMemory
                        }
                        else
                        {
                            memoryUsage = String.Format("Mem: {0} / {1} (kb)", usedMemory / 1024, (heap.heapSize) / 1024);
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

        private void ColorHeapSlotMemory(UInt32 startAddress, UInt32 dataSize, UInt32 heapStartAddress, UInt32 heapSize, Int32 heapIndex, Color color)
        {
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

                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel] |= color.B;
                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 1] |= color.G;
                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 2] |= color.R;
            }
        }
    }
    //// End class
}
//// End namespace