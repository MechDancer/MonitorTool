using System;
using Windows.UI.Xaml.Data;

namespace MonitorTool.Controls {
    internal class IntFormatter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is int number
                ? number == int.MaxValue ? "+∞"
                : number == int.MinValue ? "-∞"
                : number.ToString()
                : value.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            var text = ((string) value).ToLower();
            return int.TryParse(text, out var result)
                ? result
                : text == "inf"
                  || text == "+inf"
                  || text == "∞"
                  || text == "+∞"
                    ? int.MaxValue
                    : text == "-inf"
                      || text == "-∞"
                        ? int.MinValue
                        : 0;
        }
    }
}