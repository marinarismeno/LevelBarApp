using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System;


namespace LevelBarApp
{
    public class ValueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progressValue)
            {
                var gradient = new LinearGradientBrush
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = new System.Windows.Point(1, 0)
                };

                if (progressValue <= 0.2)
                {
                    gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.0));
                    gradient.GradientStops.Add(new GradientStop(Colors.Lime, 1.0));
                }
                else if (progressValue <= 0.4)
                {
                    gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.0));
                    gradient.GradientStops.Add(new GradientStop(Colors.Lime, 0.6));
                    gradient.GradientStops.Add(new GradientStop(Colors.YellowGreen, 1.0));
                    gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
                }
                else if (progressValue <= 0.7)
                {
                    gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.0));
                    gradient.GradientStops.Add(new GradientStop(Colors.GreenYellow, 0.6));
                    gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 1.0));
                }
                else if (progressValue <= 0.95)
                {
                    gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.0));
                    gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.6));
                    gradient.GradientStops.Add(new GradientStop(Colors.Orange, 0.8));
                    gradient.GradientStops.Add(new GradientStop(Colors.OrangeRed, 1.0));
                }
                else
                {
                    gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.0)); 
                    gradient.GradientStops.Add(new GradientStop(Colors.GreenYellow, 0.3)); 
                    gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.45));
                    gradient.GradientStops.Add(new GradientStop(Colors.Orange, 0.60)); 
                    gradient.GradientStops.Add(new GradientStop(Colors.DarkOrange, 0.75));
                    gradient.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                }

                return gradient;
            }

            return Brushes.Green; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}