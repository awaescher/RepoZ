using System;
using RepoZ.Api.Common.Common;

namespace RepoZ.Api.Common.Git.AutoFetch
{
    public class DefaultAutoFetchHandler : IAutoFetchHandler
    {
        private readonly IAppSettingsService _appSettingsService;
        private bool _active;
        private AutoFetchMode? _mode = null;

        public DefaultAutoFetchHandler(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
            _appSettingsService.RegisterInvalidationHandler(() => Mode = _appSettingsService.AutoFetchMode);
        }

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;

                if (value && _mode == null)
                    Mode = _appSettingsService.AutoFetchMode;
            }
        }

        public AutoFetchMode Mode
        {
            get => _mode ?? AutoFetchMode.Off;
            set
            {
                if (value == _mode)
                    return;

                _mode = value;

                Console.WriteLine("AUTO-FETCH: " + _mode.ToString());
            }
        }
    }
}
