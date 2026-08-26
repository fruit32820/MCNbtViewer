using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace GUI.Converters;

public class BoolToCardCornerConverter : IValueConverter
{
    /// <summary>
    /// true → 激活状态圆角；false → 默认圆角
    /// </summary>
    public CornerRadius TrueCorner { get; set; }
    public CornerRadius FalseCorner { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool flag)
        {
            return flag ? TrueCorner : FalseCorner;
        }
        return FalseCorner;
    }

    // OneWay绑定不需要反向转换，直接抛异常
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
