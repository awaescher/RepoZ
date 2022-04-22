namespace RepoZ.App.Win.i18n
{
    using RepoZ.Api.Common.Common;
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows;

    public class ResourceDictionaryTranslationService : ITranslationService
    {
        private static ResourceDictionary _dictionary = null;

        public string Translate(string value)
        {
            var translated = ResourceDictionary[value]?.ToString();
            return translated ?? value;
        }

        public string Translate(string value, params object[] args)
        {
            var translated = ResourceDictionary[value]?.ToString();

            return string.IsNullOrEmpty(translated)
                ? string.Empty
                : string.Format(translated, args);
        }

        private static ResourceDictionary GetLocalResourceDictionary()
        {
            try
            {
                var dictionaryLocation = $"i18n\\{Thread.CurrentThread.CurrentUICulture}.xaml";
                return new ResourceDictionary
                    {
                        Source = new Uri(dictionaryLocation, UriKind.RelativeOrAbsolute),
                    };
            }
            catch (IOException)
            {
                return new ResourceDictionary
                    {
                        Source = new Uri("i18n\\en-US.xaml", UriKind.RelativeOrAbsolute),
                    };
            }
        }

        public static ResourceDictionary ResourceDictionary => _dictionary ??= GetLocalResourceDictionary();
    }
}
