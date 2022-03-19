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
        static readonly UInt32 ActorReferenceTableBase = 0x804224B8;
        static readonly Int32 ActorReferenceCountTableMaxEntries = 128;

        static readonly Int32 HeapCount = 1;

        static readonly Int32 ActorSlotImageWidth = 2048;
        static readonly Int32 ActorSlotImageHeight = 1;
        static readonly Int32 HeapImageWidth = 65536;
        static readonly Int32 HeapImageHeight = 1;
        static readonly Int32 DPI = 72;

        static readonly int ActorSlotStructSize = typeof(ActorReferenceCountTableSlot).StructLayoutAttribute.Size;

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
            this.ActorReferenceCountSlots = new FullyObservableCollection<ActorReferenceCountTableSlot>();
            this.HeapBitmapBuffers = new List<Byte[]>();

            for (int index = 0; index < ActorReferenceCountTableMaxEntries; index++)
            {
                this.ActorReferenceCountSlots.Add(new ActorReferenceCountTableSlot());
            }

            try
            {
                WriteableBitmap HeapBitmap = new WriteableBitmap(ActorSlotImageWidth, ActorSlotImageHeight, DPI, DPI, PixelFormats.Bgr24, null);
                Byte[] HeapBuffer = new Byte[HeapBitmap.BackBufferStride * ActorSlotImageHeight];

                this.HeapBitmaps.Add(HeapBitmap);
                this.HeapBitmapBuffers.Add(HeapBuffer);
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
        /// Gets the list of visualization bitmaps.
        /// </summary>
        public List<WriteableBitmap> HeapBitmaps { get; private set; }

        /// <summary>
        /// Gets the list of actor reference count slots.
        /// </summary>
        public FullyObservableCollection<ActorReferenceCountTableSlot> ActorReferenceCountSlots { get; private set; }

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
                byte[] slotData = new byte[ActorSlotStructSize];

                while (this.CanUpdate)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Read the entire actor reference counting table
                            bool success = false;
                            byte[] actorReferenceCountTable = MemoryReader.Instance.ReadBytes(
                                SessionManager.Session.OpenedProcess,
                                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceTableBase, EmulatorType.Dolphin),
                                ActorSlotStructSize * ActorReferenceCountTableMaxEntries,
                                out success);

                            // Clear out old visual data
                            for (Int32 heapIndex = 0; heapIndex < HeapCount; heapIndex++)
                            {
                                Array.Clear(this.HeapBitmapBuffers[0], 0, this.HeapBitmapBuffers[0].Length);
                            }

                            // Update new data / visual data
                            if (success)
                            {
                                for (int actorSlotIndex = 0; actorSlotIndex < ActorReferenceCountTableMaxEntries; actorSlotIndex++)
                                {
                                    Array.Copy(actorReferenceCountTable, actorSlotIndex * ActorSlotStructSize, slotData, 0, ActorSlotStructSize);
                                    ActorReferenceCountTableSlot result = ActorReferenceCountTableSlot.FromByteArray(slotData);

                                    // Copy data over field by field to avoid triggering the FullyObservableCollection changes.
                                    this.ActorReferenceCountSlots[actorSlotIndex].Name = result.Name;
                                    this.ActorReferenceCountSlots[actorSlotIndex].ReferenceCount = result.ReferenceCount;
                                    this.ActorReferenceCountSlots[actorSlotIndex].Padding = result.Padding;
                                    this.ActorReferenceCountSlots[actorSlotIndex].MDMCommandPtr = result.MDMCommandPtr;
                                    this.ActorReferenceCountSlots[actorSlotIndex].MArchivePtr = result.MArchivePtr;
                                    this.ActorReferenceCountSlots[actorSlotIndex].HeapPtr = result.HeapPtr;
                                    this.ActorReferenceCountSlots[actorSlotIndex].MDataHeapPtr = result.MDataHeapPtr;
                                    this.ActorReferenceCountSlots[actorSlotIndex].MResPtrPtr = result.MResPtrPtr;

                                    this.ColorActorSlotMemory(actorSlotIndex, 0, this.ActorReferenceCountSlots[actorSlotIndex].ReferenceCount > 0 ? Color.FromRgb(255, 0, 0) : Color.FromRgb(0, 0, 0));
                                }
                            }

                            for (int heapIndex = 0; heapIndex < this.HeapBitmaps.Count; heapIndex++)
                            {
                                Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
                                this.HeapBitmaps[heapIndex].WritePixels(
                                    new Int32Rect(0, 0, ActorSlotImageWidth, ActorSlotImageHeight),
                                    this.HeapBitmapBuffers[heapIndex],
                                    this.HeapBitmaps[heapIndex].PixelWidth * bytesPerPixel,
                                    0
                                );
                            }
                        });
                    }
                    catch(Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error updating the Heap Visualizer", ex);
                    }

                    await Task.Delay(250);
                }
            });
        }

        private void ColorActorSlotMemory(Int32 actorSlotIndex, Int32 heapIndex, Color color)
        {
            const Int32 seperatorSize = 4;
            Int32 bytesPerPixel = this.HeapBitmaps[heapIndex].Format.BitsPerPixel / 8;
            Int32 memoryStart = (Int32)((double)actorSlotIndex * (double)ActorSlotImageWidth / (double)ActorReferenceCountTableMaxEntries);
            Int32 memoryEnd = (Int32)((double)(actorSlotIndex + 1) * (double)ActorSlotImageWidth / (double)ActorReferenceCountTableMaxEntries);

            for (Int32 pixelIndex = memoryStart + seperatorSize; pixelIndex < memoryEnd; pixelIndex++)
            {
                if (pixelIndex >= ActorSlotImageWidth)
                {
                    throw new Exception("Address out of bitmap range!");
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