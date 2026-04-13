using System.Globalization;

namespace AppUser.Converters;

public class ChatBubbleColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isFromUser = value is bool b && b;
        return isFromUser ? Color.FromArgb("#6200EE") : Color.FromArgb("#F1F1F1");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ChatTextColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isFromUser = value is bool b && b;
        return isFromUser ? Colors.White : Colors.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ChatAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isFromUser = value is bool b && b;
        return isFromUser ? LayoutOptions.End : LayoutOptions.Start;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
