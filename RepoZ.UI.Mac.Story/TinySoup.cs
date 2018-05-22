using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TinySoup.Identifier;
using TinySoup.Internal;
using TinySoup.Model;
using System.Net;

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

        public Task<IList<AvailableVersion>> CheckForUpdatesAsync(UpdateRequest request) => CheckForUpdatesAsync(request, new DefaultVersionComparer());

        public async Task<IList<AvailableVersion>> CheckForUpdatesAsync(UpdateRequest request, IComparer<Version> versionComparer)
        {
            var parameters = new ServiceParameterCollection
            {
                { "cid", Uri.EscapeDataString(request.ClientIdentifier?.ToString() ?? "") },
                { "pid", Uri.EscapeDataString(request.ApplicationIdentifier ?? "") },
                { "ver", Uri.EscapeDataString(MakeFourDigitVersionNumber(request.CurrentVersionInUse)) }, // Mono's Version.ToString(4) skips zeros anyway
                { "vai", Uri.EscapeDataString(request.Channel ?? "") },
                { "ext", Uri.EscapeDataString(request.PlatformIdentifier?.ToString() ?? "") },
                { "vol", Uri.EscapeDataString(request.FreeText ?? "") }
            };

            var versions = await PutAsync(parameters.ToString());

            return versions
                .Where(v => versionComparer.Compare(v.Version, request.CurrentVersionInUse) > 0)
                .OrderByDescending(v => v.Version, versionComparer)
                .ToList();
        }

        public WebSoupClient WithExceptionHandler(Action<Exception> exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
            return this;
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
            HttpClient client;

            var uri = new Uri($"{URL}?method={method}&{parameterString}");

            var serializer = new DataContractJsonSerializer(typeof(List<AvailableVersion>));

            try
            {
                var proxyUri = new Uri(WebRequest.DefaultWebProxy.GetProxy(uri).AbsoluteUri);
                var directAccess = proxyUri?.Authority?.Equals(uri.Authority) == true;

                if (directAccess)
                {
                    client = new HttpClient();
                }
                else
                {
                    var proxy = new WebProxy()
                    {
                        Address = proxyUri,
                        BypassProxyOnLocal = false,
                        UseDefaultCredentials = true
                    };
                    var httpClientHandler = new HttpClientHandler() { Proxy = proxy };
                    client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
                }

                return serializer.ReadObject(await client.GetStreamAsync(uri).ConfigureAwait(false)) as List<AvailableVersion>;
            }
            catch (Exception ex)
            {
                _exceptionHandler?.Invoke(ex);
                return await Task.FromResult(new List<AvailableVersion>()).ConfigureAwait(false);
            }
        }

        private string MakeFourDigitVersionNumber(Version version)
        {
            if (version == null)
                version = new Version(0, 0, 0, 0);
            
            int NotNegative(int value) => Math.Max(value, 0);

            // Mono's Version.ToString(4) skips zeros, lets format this manually
            return $"{NotNegative(version.Major)}.{NotNegative(version.Minor)}.{NotNegative(version.Build)}.{NotNegative(version.Revision)}";
        }
    }

    public class DefaultVersionComparer : IComparer<Version>
    {
        public int Compare(Version x, Version y) => x.CompareTo(y);
    }
}

namespace TinySoup.Identifier
{
    public interface IClientIdentifier
    {
    }

    public class AnonymousClientIdentifier : IClientIdentifier
    {
        public override string ToString()
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
    [System.Diagnostics.DebuggerDisplay("{ApplicationIdentifier,nq} {Version.ToString(), nq}")]
    [DataContract]
    public class AvailableVersion
    {
        private Version _version;
        private string _versionString;

        [DataMember(Name = "Application")]
        public string ApplicationIdentifier { get; set; } = "";

        [IgnoreDataMember]
        public Version Version
        {
            get => _version ?? new Version();
        }

        [DataMember(Name = "VersionString")]
        public string VersionString
        {
            get => _versionString;
            set
            {
                _versionString = value;
                if (!Version.TryParse(_versionString, out _version))
                    _version = null;
            }
        }

        [DataMember(Name = "VersionAdditionalInfo")]
        public string Channel { get; set; } = "";

        [DataMember(Name = "Url")]
        public string Url { get; set; } = "";

        [DataMember(Name = "ExternalInfo")]
        public string Info { get; set; } = "";

        public override string ToString()
        {
            return $"{ApplicationIdentifier ?? ""} {Version.ToString() ?? ""}".Trim();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{ApplicationIdentifier,nq} {CurrentVersionInUse.ToString(),nq}")]
    public class UpdateRequest
    {
        public UpdateRequest WithNameAndVersionFromAssembly(Assembly assembly)
        {
            var name = assembly.GetName();
            ApplicationIdentifier = name.Name;
            CurrentVersionInUse = name.Version;

            return this;
        }

        public UpdateRequest WithNameAndVersionFromEntryAssembly() => WithNameAndVersionFromAssembly(Assembly.GetEntryAssembly());

        public UpdateRequest WithNameAndVersionFromExecutingAssembly() => WithNameAndVersionFromAssembly(Assembly.GetExecutingAssembly());

        public UpdateRequest WithNameAndVersionFromCallingAssembly() => WithNameAndVersionFromAssembly(Assembly.GetCallingAssembly());

        public UpdateRequest WithClientIdentifier(IClientIdentifier clientIdentifier)
        {
            ClientIdentifier = clientIdentifier;
            return this;
        }

        public UpdateRequest AsAnonymousClient() => WithClientIdentifier(new AnonymousClientIdentifier());

        public UpdateRequest OnChannel(string channel)
        {
            Channel = channel;
            return this;
        }

        public UpdateRequest OnPlatform(IPlatformIdentifier platformIdentifier)
        {
            PlatformIdentifier = platformIdentifier;
            return this;
        }

        public UpdateRequest WithVersion(Version version)
        {
            CurrentVersionInUse = version;
            return this;
        }

        public UpdateRequest WithVersion(string version)
        {
            CurrentVersionInUse = Version.Parse(version);
            return this;
        }

        public IClientIdentifier ClientIdentifier { get; set; }

        public string ApplicationIdentifier { get; set; }

        public Version CurrentVersionInUse { get; set; }

        public string Channel { get; set; }

        public IPlatformIdentifier PlatformIdentifier { get; set; }

        public string FreeText { get; set; }
    }

    public interface IPlatformIdentifier
    {
    }

    public class OperatingSystemIdentifier : IPlatformIdentifier
    {
        public override string ToString()
        {
            var platformName = Environment.OSVersion.ToString();
            var prefix = string.IsNullOrWhiteSpace(Prefix) ? "" : Prefix;
            var suffix = string.IsNullOrWhiteSpace(Suffix) ? "" : Suffix;

            return $"{prefix} {platformName} {suffix}".Trim();
        }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public OperatingSystemIdentifier WithPrefix(string prefix)
        {
            Prefix = prefix;
            return this;
        }

        public OperatingSystemIdentifier WithSuffix(string suffix)
        {
            Suffix = suffix;
            return this;
        }
    }
}