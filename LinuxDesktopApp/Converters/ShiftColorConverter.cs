namespace LinuxDesktopApp.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media;

public class ShiftColorConverter : IValueConverter
{
    public IBrush ActiveColor { get; set; } = Brushes.Turquoise;

    public IBrush InactiveColor { get; set; } = Brushes.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if ((value is int currentShift) && (parameter is int gear))
        {
            return currentShift == gear ? ActiveColor : InactiveColor;
        }

        return InactiveColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
