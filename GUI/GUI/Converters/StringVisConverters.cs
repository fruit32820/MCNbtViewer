using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace GUI.Converters;

public class StringNotEmptyToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var s = value as string;
        return string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}

public class StringEmptyToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var s = value as string;
        return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
