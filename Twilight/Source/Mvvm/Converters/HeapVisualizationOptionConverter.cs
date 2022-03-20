namespace Twilight.Source.Mvvm.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using static Twilight.Source.HeapVisualizer.HeapVisualizerViewModel;

    /// <summary>
    /// Converter class to convert a boolean to an arbitrary value.
    /// </summary>
    /// <typeparam name="T">The target conversion type.</typeparam>
    public class HeapVisualizationOptionConverter : IValueConverter
    {
        public HeapVisualizationOptionConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HeapVisualizationOption)
            {
                switch ((HeapVisualizationOption)value)
                {
                    case HeapVisualizationOption.CMem:
                        return "CMemoryBlock Iteration";
                    case HeapVisualizationOption.NonZeroMemory:
                        return "Non-Zero Memory (Inaccurate)";
                    default:
                        throw new ArgumentOutOfRangeException("value", value, null);
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace