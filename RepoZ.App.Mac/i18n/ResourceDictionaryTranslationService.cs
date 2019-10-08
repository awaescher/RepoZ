using System.IO;
using System.Collections.Generic;
using System.Threading;
using RepoZ.Api.Common.Common;
using System.Text.RegularExpressions;
using System;
using System.Xml.Linq;
using System.Linq;

namespace RepoZ.App.Mac.i18n
{
	public class ResourceDictionaryTranslationService : ITranslationService
	{
		private static Dictionary<string, string> _translations;

		public string Translate(string value)
		{
			if (Translations.TryGetValue(value, out var translation))
				return translation;

			return value;
		}

		public string Translate(string value, params object[] args)
		{
			var translation = Translate(value);
			return string.Format(translation, args);
		}


		private static Dictionary<string, string> GetLocalResourceDictionary()
		{
			try
			{
				var dictionaryLocation = $"i18n/{Thread.CurrentThread.CurrentUICulture}.xaml";
				return ParseResourceDictionary(dictionaryLocation);
			}
			catch (IOException)
			{
				return ParseResourceDictionary("i18n/en-US.xaml");
			}
		}

		private static Dictionary<string, string> ParseResourceDictionary(string location)
		{
			var content = File.ReadAllText(location);

			var root = XElement.Parse(content);
			return root.Descendants().ToDictionary(d => d.Attributes().Single(a => a.Name.LocalName == "Key").Value, d => d.Value);
		}

		public static Dictionary<string, string> Translations
		{
			get
			{
				if (_translations == null)
					_translations = GetLocalResourceDictionary();

				return _translations;
			}
		}
	}
}
