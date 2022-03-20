namespace Twilight.Source.ActorReferenceCountVisualizer
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
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
    public class ActorReferenceCountVisualizerViewModel : ToolViewModel
    {
        private static readonly UInt32 ActorReferenceTableBase = 0x804224B8;
        private static readonly Int32 ActorReferenceCountTableMaxEntries = 128;
        private static readonly Int32 ActorSlotStructSize = typeof(ActorReferenceCountTableSlot).StructLayoutAttribute.Size;

        private static readonly Int32 ActorSlotImageWidth = 2048;
        private static readonly Int32 ActorSlotImageHeight = 1;
        private static readonly Int32 DPI = 72;

        /// <summary>
        /// Singleton instance of the <see cref="ActorReferenceCountVisualizer" /> class.
        /// </summary>
        private static ActorReferenceCountVisualizerViewModel actorReferenceCountVisualizerInstance = new ActorReferenceCountVisualizerViewModel();

        /// <summary>
        /// Prevents a default instance of the <see cref="HeapVisualizerViewModel" /> class from being created.
        /// </summary>
        private ActorReferenceCountVisualizerViewModel() : base("Actor Reference Count Visualizer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.ActorReferenceCountSlots = new FullyObservableCollection<ActorReferenceCountTableSlot>();

            for (int index = 0; index < ActorReferenceCountTableMaxEntries; index++)
            {
                this.ActorReferenceCountSlots.Add(new ActorReferenceCountTableSlot());
            }

            try
            {
                this.ActorSlotBitmap = new WriteableBitmap(ActorSlotImageWidth, ActorSlotImageHeight, DPI, DPI, PixelFormats.Bgr24, null);
                this.ActorSlotBitmapBuffer = new Byte[this.ActorSlotBitmap.BackBufferStride * ActorSlotImageHeight];
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
        /// Gets the actor visualization bitmap.
        /// </summary>
        public WriteableBitmap ActorSlotBitmap { get; private set; }

        /// <summary>
        /// Gets the list of actor reference count slots.
        /// </summary>
        public FullyObservableCollection<ActorReferenceCountTableSlot> ActorReferenceCountSlots { get; private set; }

        /// <summary>
        /// Gets the raw bitmap data for actor slot visualization.
        /// </summary>
        public Byte[] ActorSlotBitmapBuffer { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the actor reference count visualizer update loop can run.
        /// </summary>
        private bool CanUpdate { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ActorReferenceCountVisualizerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ActorReferenceCountVisualizerViewModel GetInstance()
        {
            return ActorReferenceCountVisualizerViewModel.actorReferenceCountVisualizerInstance;
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
                                UpdateActorSlots();
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

        private void UpdateActorSlots()
        {
            // Read the entire actor reference counting table
            bool success = false;
            byte[] actorReferenceCountTable = MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                MemoryQueryer.Instance.EmulatorAddressToRealAddress(SessionManager.Session.OpenedProcess, ActorReferenceTableBase, EmulatorType.Dolphin),
                ActorSlotStructSize * ActorReferenceCountTableMaxEntries,
                out success);

            // Clear out old visual data
            Array.Clear(this.ActorSlotBitmapBuffer, 0, this.ActorSlotBitmapBuffer.Length);

            // Update new data / visual data
            if (success)
            {
                byte[] slotData = new byte[ActorSlotStructSize];
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

                    this.ColorActorSlotMemory(actorSlotIndex, this.ActorReferenceCountSlots[actorSlotIndex].ReferenceCount > 0 ? Color.FromRgb(255, 0, 0) : Color.FromRgb(0, 0, 0));
                }
            }

            Int32 bytesPerPixel = this.ActorSlotBitmap.Format.BitsPerPixel / 8;
            this.ActorSlotBitmap.WritePixels(
                new Int32Rect(0, 0, ActorSlotImageWidth, ActorSlotImageHeight),
                this.ActorSlotBitmapBuffer,
                this.ActorSlotBitmap.PixelWidth * bytesPerPixel,
                0
            );
        }

        private void ColorActorSlotMemory(Int32 actorSlotIndex, Color color)
        {
            const Int32 seperatorSize = 4;
            Int32 bytesPerPixel = this.ActorSlotBitmap.Format.BitsPerPixel / 8;
            Int32 pixelStart = (Int32)((double)actorSlotIndex * (double)ActorSlotImageWidth / (double)ActorReferenceCountTableMaxEntries);
            Int32 pixelEnd = (Int32)((double)(actorSlotIndex + 1) * (double)ActorSlotImageWidth / (double)ActorReferenceCountTableMaxEntries);

            for (Int32 pixelIndex = pixelStart + seperatorSize; pixelIndex < pixelEnd; pixelIndex++)
            {
                if (pixelIndex >= ActorSlotImageWidth)
                {
                    throw new Exception("Address out of bitmap range!");
                }

                this.ActorSlotBitmapBuffer[pixelIndex * bytesPerPixel] = color.B;
                this.ActorSlotBitmapBuffer[pixelIndex * bytesPerPixel + 1] = color.G;
                this.ActorSlotBitmapBuffer[pixelIndex * bytesPerPixel + 2] = color.R;
            }
        }
    }
    //// End class
}
//// End namespace