namespace Twilight.Source.HeapVisualizer
{
    using GalaSoft.MvvmLight.Command;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.DataStructures;
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
                    catch(Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error updating the Heap Visualizer", ex);
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
                Array.Clear(this.HeapBitmapBuffers[0], 0, this.HeapBitmapBuffers[0].Length);
            }

            // Read the entire actor reference counting table
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


                        /*
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
                        */

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
                                this.ColorHeapSlotMemory(nextCMemBlockPtr, cMemBlock.blockSize, heap.heapPointer, heap.heapSize, heapIndex, Color.FromRgb(255, 0, 0));
                                usedBlocks++;
                                usedMemory += cMemBlock.blockSize;
                                nextCMemBlockPtr = cMemBlock.nextPtr;
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

        private void ColorHeapSlotMemory(UInt32 startAddress, UInt32 dataSize, UInt32 heapStartAddress, UInt32 heapSize, Int32 heapIndex, Color color)
        {
            Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
            UInt32 offset = startAddress - heapStartAddress;
            Int32 pixelStart = (Int32)((double)offset * (double)HeapImageWidth / (double)heapSize);
            Int32 pixelEnd = (Int32)((double)(offset + dataSize) * (double)HeapImageWidth / (double)heapSize);

            if (startAddress < heapStartAddress)
            {
                Logger.Log(LogLevel.Warn, "Start address for this data block is outside of specified heap range.");
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
                    Logger.Log(LogLevel.Warn, "Address out of bitmap range!");
                    return;
                }

                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel] = color.B;
                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 1] = color.G;
                this.HeapBitmapBuffers[heapIndex][pixelIndex * bytesPerPixel + 2] = color.R;
            }
        }
    }
    //// End class
}
//// End namespace