namespace Twilight.View
{
    using Source.Main;
    using Twilight.Source.Controls;
    using Twilight.Source.Docking;
    using Twilight.Source.HeapVisualizer;
    using Twilight.Source.Output;
    using Twilight.Source.ProcessSelector;
    using Twilight.Source.Tasks;
    using Twilight.Source.PropertyViewer;
    using Twilight.Source.FlagRecorder;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
        }

        /// <summary>
        /// Gets the Docking view model.
        /// </summary>
        public DockingViewModel DockingViewModel
        {
            get
            {
                return DockingViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Action Scheduler view model.
        /// </summary>
        public TaskTrackerViewModel TaskTrackerViewModel
        {
            get
            {
                return TaskTrackerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Process Selector view model.
        /// </summary>
        public ProcessSelectorViewModel ProcessSelectorViewModel
        {
            get
            {
                return ProcessSelectorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Output view model.
        /// </summary>
        public OutputViewModel OutputViewModel
        {
            get
            {
                return OutputViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Main view model.
        /// </summary>
        public MainViewModel MainViewModel
        {
            get
            {
                return MainViewModel.GetInstance();
            }
        }


        /// <summary>
        /// Gets the Property Editor view model.
        /// </summary>
        public PropertyViewerViewModel PropertyViewerViewModel
        {
            get
            {
                return PropertyViewerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Heap Visualizer view model.
        /// </summary>
        public HeapVisualizerViewModel HeapVisualizerViewModel
        {
            get
            {
                return HeapVisualizerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Flag Recorder view model.
        /// </summary>
        public FlagRecorderViewModel FlagRecorderViewModel
        {
            get
            {
                return FlagRecorderViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a HexDec box view model.
        /// </summary>
        public HexDecBoxViewModel HexDecBoxViewModel
        {
            get
            {
                return new HexDecBoxViewModel();
            }
        }
    }
    //// End class
}
//// End namespace