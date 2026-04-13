using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace AppUser.Converters
{
    /// <summary>Converts bool to inverted bool (used for IsEnabled = !IsLoading)</summary>
    public class BoolInverterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : value;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b ? !b : value;
    }

    /// <summary>Returns true if string is not null or empty</summary>
    public class StringNotEmptyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => !string.IsNullOrEmpty(value?.ToString());

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns true if string contains substring</summary>
    public class StringContainsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && parameter is string sub)
                return str.Contains(sub, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns true if int is zero</summary>
    public class IntIsZeroConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int i ? i == 0 : false;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Formats TimeSpan as mm:ss</summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
                return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}";
            return "00:00";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Converts bool visibility to FontAwesome icon code for eye</summary>
    public class EyeIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isVisible = value is bool b && b;
            return isVisible ? "🙈" : "👁️"; // Eye-slash -> Monkey, Eye -> Eye
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns a RoundRectangle shape based on whether the message is from user</summary>
    public class ChatBubbleShapeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isFromUser = value is bool b && b;
            return new RoundRectangle
            {
                CornerRadius = isFromUser ? new CornerRadius(20, 20, 4, 20) : new CornerRadius(20, 20, 20, 4)
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Returns true if int is greater than zero</summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is int i && i > 0;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}