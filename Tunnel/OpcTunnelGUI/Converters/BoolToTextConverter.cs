using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpcTunnelGUI.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "▲ 隐藏高级设置" : "▼ 显示高级设置";
        }
        return "▼ 显示高级设置";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
