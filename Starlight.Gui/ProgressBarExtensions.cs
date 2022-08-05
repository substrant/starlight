using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Starlight.Gui
{
    internal static class ProgressBarExtensions
    {
        static TimeSpan Duration = TimeSpan.FromSeconds(2);

        public static void SetPercent(this ProgressBar progressBar, double percentage)
        {
            DoubleAnimation animation = new(percentage, Duration);
            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
