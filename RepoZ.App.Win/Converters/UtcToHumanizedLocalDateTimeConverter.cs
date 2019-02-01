using RepoZ.Api.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RepoZ.App.Win.Converters
{
	public class UtcToHumanizedLocalDateTimeConverter : IValueConverter
	{
		public UtcToHumanizedLocalDateTimeConverter()
		{
			Humanizer = new HardcodededMiniHumanizer();
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var date = DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc).ToLocalTime();
			return Humanizer.HumanizeTimestamp(date);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public IHumanizer Humanizer { get; }
	}
}
