using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TinySoup.Identifier;
using TinySoup.Internal;
using TinySoup.Model;

namespace TinySoup
{
    public interface ISoupClient
    {
        Task<IList<AvailableVersion>> CheckForUpdatesAsync(UpdateRequest request);
    }

    public class WebSoupClient : ISoupClient
    {
        const string URL = "https://www.sodacore.net/webservices/updateservice.php";

        private Action<Exception> _exceptionHandler;

        public Task<IList<AvailableVersion>> CheckForUpdatesAsync(UpdateRequest request)
        {
            var parameters = new ServiceParameterCollection
            {
                { "cid", Uri.EscapeDataString(request.ClientIdentifier?.GetIdentifier() ?? "") },
                { "pid", Uri.EscapeDataString(request.ApplicationIdentifier ?? "") },
                { "ver", Uri.EscapeDataString(request.CurrentVersionInUse ?? "") },
                { "vai", Uri.EscapeDataString(request.Channel ?? "") },
                { "ext", Uri.EscapeDataString(request.UserAgent ?? "") },
                { "vol", Uri.EscapeDataString(request.FreeText ?? "") }
            };

            return PutAsync(parameters.ToString());
        }

        public void RegisterExceptionHandler(Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        internal Task<IList<AvailableVersion>> PutAsync(string parameterString)
        {
            return CallWebServiceAsync("PutData", parameterString);
        }

        internal Task<IList<AvailableVersion>> GetAsync(string table, string where)
        {
            var parameters = new ServiceParameterCollection("table", table).Add("where", where);
            return CallWebserviceAsync("GetData", parameters);
        }

        internal Task<IList<AvailableVersion>> CallWebserviceAsync(string method, ServiceParameterCollection parameters)
        {
            return CallWebServiceAsync(method, parameters.ToString());
        }

        internal async Task<IList<AvailableVersion>> CallWebServiceAsync(string method, string parameterString)
        {
            var url = $"{URL}?method={method}&{parameterString}";

            var serializer = new DataContractJsonSerializer(typeof(List<AvailableVersion>));

            try
            {
                var client = new HttpClient();
                return serializer.ReadObject(await client.GetStreamAsync(url).ConfigureAwait(false)) as List<AvailableVersion>;
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
                return await Task.FromResult(new List<AvailableVersion>()).ConfigureAwait(false);
            }
        }
    }
}

namespace TinySoup.Identifier
{
    public interface IClientIdentifier
    {
        string GetIdentifier();
    }

    public class AnonymousClientIdentifier : IClientIdentifier
    {
        public string GetIdentifier()
        {
            var id = "";

            var file = Path.Combine(EnsureConfigPath(), "anonymous.id");

            if (FileExists(file))
                id = ReadId(file);

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("n");
                WriteId(file, id);
            }

            return id;
        }

        protected virtual string EnsureConfigPath()
        {
            var path = Path.Combine(GetPlatformConfigPath(), "sodacore studios", "TinySoup");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        protected virtual string GetPlatformConfigPath()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    // AppData Roaming
                    var appDataPath = Environment.GetEnvironmentVariable("AppData");
                    return appDataPath ?? throw new PlatformNotSupportedException(nameof(AnonymousClientIdentifier) + " cannot be used with UWP. Derive a custom identifer from it and handle the file IO in UWP manner.");

                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // macOS: /Users/USERNAME/.config
                    // Linux: /home/USERNAME/.config
                    var homePath = Environment.GetEnvironmentVariable("HOME");
                    return Path.Combine(homePath, ".config");
            }

            throw new PlatformNotSupportedException();
        }

        protected virtual bool FileExists(string file) => File.Exists(file);

        protected virtual string ReadId(string file) => File.ReadAllText(file, Encoding.UTF8);

        protected virtual void WriteId(string file, string id) => File.WriteAllText(file, id, Encoding.UTF8);
    }
}

namespace TinySoup.Internal
{
    internal class ServiceParameter
    {
        public string Name { get; private set; }

        public object Value { get; set; }

        public ServiceParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            string value = Value != null ? Value.ToString() : string.Empty;
            return string.Format("{0}={1}", Name, value);
        }
    }

    internal class ServiceParameterCollection : List<ServiceParameter>
    {
        public ServiceParameterCollection()
        {
        }

        public ServiceParameterCollection(string name, object value)
            : this()
        {
            Add(name, value);
        }

        public ServiceParameterCollection Add(string name, object value)
        {
            Add(new ServiceParameter(name, value));
            return this;
        }

        public ServiceParameterCollection Copy()
        {
            ServiceParameterCollection result = new ServiceParameterCollection();

            foreach (ServiceParameter param in this)
                result.Add(new ServiceParameter(param.Name, param.Value));

            return result;
        }

        public override string ToString()
        {
            string result = string.Empty;

            foreach (ServiceParameter param in this)
            {
                if (!string.IsNullOrEmpty(result))
                    result += "&";

                result += param.ToString();
            }

            return result;
        }
    }
}

namespace TinySoup.Model
{
    [DataContract]
    public class AvailableVersion
    {
        [DataMember(Name = "Application")]
        public string ApplicationIdentifier { get; set; } = "";

        [DataMember(Name = "VersionString")]
        public string Version { get; set; } = "";

        [DataMember(Name = "VersionAdditionalInfo")]
        public string Channel { get; set; } = "";

        [DataMember(Name = "Url")]
        public string Url { get; set; } = "";

        [DataMember(Name = "ExternalInfo")]
        public string Info { get; set; } = "";

        public override string ToString()
        {
            return $"{ApplicationIdentifier ?? ""} {Version ?? ""}".Trim();
        }
    }

    public class UpdateRequest
    {
        public IClientIdentifier ClientIdentifier { get; set; }

        public string ApplicationIdentifier { get; set; }

        public string CurrentVersionInUse { get; set; }

        public string Channel { get; set; }

        public string UserAgent { get; set; }

        public string FreeText { get; set; }
    }
}