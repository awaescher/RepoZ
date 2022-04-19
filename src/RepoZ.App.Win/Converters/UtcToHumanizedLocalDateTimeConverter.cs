namespace RepoZ.App.Win.Converters
{
    using RepoZ.Api.Common;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class UtcToHumanizedLocalDateTimeConverter : IValueConverter
    {
        public UtcToHumanizedLocalDateTimeConverter()
        {
            Humanizer = new HardcodededMiniHumanizer();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc).ToLocalTime();
            return Humanizer.HumanizeTimestamp(date);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public IHumanizer Humanizer { get; }
    }
}