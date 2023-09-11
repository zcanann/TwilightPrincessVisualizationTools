namespace Twilight.Source.Main
{
    using System;
    using System.Numerics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.Hardware;
    using Twilight.Engine.Common.Logging;
    using Twilight.Engine.Common.OS;
    using Twilight.Engine.Memory;
    using Twilight.Source.Docking;
    using Twilight.Source.Output;

    public enum EDetectedVersion
    {
        None,
        GC_En,
        GC_Jp,
        GC_Pal,
        Wii_En_1_0,
        Wii_En_1_2,
    };

    /// <summary>
    /// Main view model.
    /// </summary>
    public class MainViewModel : WindowHostViewModel
    {

        /// <summary>
        /// Singleton instance of the <see cref="MainViewModel" /> class
        /// </summary>
        private static Lazy<MainViewModel> mainViewModelInstance = new Lazy<MainViewModel>(
                () => { return new MainViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="MainViewModel" /> class from being created.
        /// </summary>
        private MainViewModel() : base()
        {
            // Attach the logger view model to the engine's output
            Logger.Subscribe(OutputViewModel.GetInstance());

            if (Vectors.HasVectorSupport)
            {
                Logger.Log(LogLevel.Info, "Hardware acceleration enabled (vector size: " + Vector<Byte>.Count + ")");
            }

            Logger.Log(LogLevel.Info, "Twilight Princess Visualization Tools started");

            Application.Current.Exit += this.OnAppExit;
            this.RunUpdateLoop();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            this.CanUpdate = false;
        }

        public EDetectedVersion DetectedVersion { get; private set; }

        /// <summary>
        /// Default layout file for browsing cheats.
        /// </summary>
        protected override String DefaultLayoutResource { get { return "DefaultLayout.xml"; } }

        /// <summary>
        /// The save file for the docking layout.
        /// </summary>
        protected override String LayoutSaveFile { get { return "Layout.xml"; } }

        /// <summary>
        /// Gets or sets a value indicating whether the update loop can run.
        /// </summary>
        private bool CanUpdate { get; set; }

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
                                this.DetectVersion();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error updating the Heap Visualizer", ex);
                    }

                    await Task.Delay(2500);
                }
            });
        }


        private Byte[] GameCode = new Byte[6];

        private void DetectVersion()
        {
            UInt64 gameCodeAddress = MemoryQueryer.Instance.ResolveModule(SessionManager.Session.OpenedProcess, "GC", EmulatorType.Dolphin);

            Boolean success;
            MemoryReader.Instance.ReadBytes(
                SessionManager.Session.OpenedProcess,
                this.GameCode,
                gameCodeAddress,
                out success);

            if (success)
            {
                const String GcVersionEn = "GZ2E01";
                const String GcVersionJp = "GZ2J01";
                const String GcVersionPal = "GZ2P01";
                const String GcVersionWii1 = "RZDE01";

                String gbaGcVersion = Encoding.ASCII.GetString(this.GameCode);

                EDetectedVersion detectedVersion = EDetectedVersion.None;

                if (gbaGcVersion == GcVersionEn)
                {
                    detectedVersion = EDetectedVersion.GC_En;
                }
                else if (gbaGcVersion == GcVersionJp)
                {
                    detectedVersion = EDetectedVersion.GC_Jp;
                }
                else if (gbaGcVersion == GcVersionPal)
                {
                    detectedVersion = EDetectedVersion.GC_Pal;
                }
                else if (gbaGcVersion == GcVersionWii1)
                {
                    detectedVersion = EDetectedVersion.Wii_En_1_2;
                }

                if (this.DetectedVersion != detectedVersion)
                {
                    this.DetectedVersion = detectedVersion;
                    // this.RefreshAllViews();
                }
            }
            else
            {
                this.DetectedVersion = EDetectedVersion.None;
            }
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="MainViewModel" /> class.</returns>
        public static MainViewModel GetInstance()
        {
            return mainViewModelInstance.Value;
        }

        /// <summary>
        /// Closes the main window.
        /// </summary>
        /// <param name="window">The window to close.</param>
        protected override void Close(Window window)
        {
            base.Close(window);
        }
    }
    //// End class
}
//// End namesapce