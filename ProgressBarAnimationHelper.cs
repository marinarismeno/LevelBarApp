using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace LevelBarApp
{
    public static class ProgressBarAnimationHelper
    {
        public static readonly DependencyProperty AnimatedValueProperty =
            DependencyProperty.RegisterAttached(
                "AnimatedValue",
                typeof(double),
                typeof(ProgressBarAnimationHelper),
                new PropertyMetadata(0.0, OnAnimatedValueChanged));

        public static void SetAnimatedValue(DependencyObject obj, double value)
        {
            obj.SetValue(AnimatedValueProperty, value);
        }

        public static double GetAnimatedValue(DependencyObject obj)
        {
            return (double)obj.GetValue(AnimatedValueProperty);
        }

        private static void OnAnimatedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProgressBar progressBar)
            {
                double newValue = (double)e.NewValue;
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = progressBar.Value,
                    To = newValue,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase() // Add easing for smoother transitions
                };
                progressBar.BeginAnimation(RangeBase.ValueProperty, animation);
            }
        }
    }
}
