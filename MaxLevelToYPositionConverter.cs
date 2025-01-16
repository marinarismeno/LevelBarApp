using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LevelBarApp
{
    public class MaxLevelToYPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double maxLevel = System.Convert.ToDouble(value);
            int canvasHeight = 310;
            double targetY = canvasHeight - (maxLevel*310); // Inverts the scaling to match WPF's coordinate system

            return targetY;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
