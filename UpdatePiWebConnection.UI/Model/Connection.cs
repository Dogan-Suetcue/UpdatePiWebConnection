using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.RegularExpressions;
using UpdatePiWebConnection.Common;

namespace UpdatePiWebConnection.UI.Model
{
    public class Connection : ValidationViewModelBase
    {
        private const string DATABASE_CONNECTION_REGEX = @"^https?:\/\/[A-Za-z0-9\.]+:\d{2,}$";
        private const string CLOUD_CONNECTION_REGEX = @"^https:\/\/piwebcloud-service\.metrology\.zeiss\.com\/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

        private bool defaultDatabaseConnection;
        private string _directory;
        private string _databaseName;
        private string _databaseUrl;
        private AuthenticationMode _authenticationMode;

        public bool DefaultDatabaseConnection
        {
            get => defaultDatabaseConnection;
            set
            {
                defaultDatabaseConnection = value;
                OnPropertyChanged();
            }
        }

        [Required(ErrorMessage = "The Directory field is required.")]
        public string Directory
        {
            get => _directory;
            set
            {
                _directory = value;
                OnPropertyChanged();
                ValidateDataAnnotations(this, value);

                if (!string.IsNullOrEmpty(value))
                {
                    var reports = System.IO.Directory.GetFiles(Directory, @"*.ptx", SearchOption.AllDirectories);
                    if (reports.Length == 0)
                        AddError("No reports were found in the directory. Please select another directory.");
                    else
                        ClearErrors();
                }
            }
        }

        [Required(ErrorMessage = "The Database Name field is required.")]
        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value;
                OnPropertyChanged();
                ValidateDataAnnotations(this, value);
            }
        }

        public string DatabaseUrl
        {
            get => _databaseUrl;
            set
            {
                _databaseUrl = value;
                OnPropertyChanged();
                ValidateDatabaseUrl();
            }
        }

        public AuthenticationMode AuthenticationMode
        {
            get => _authenticationMode;
            set
            {
                _authenticationMode = value;
                OnPropertyChanged();
            }
        }

        private void ValidateDatabaseUrl()
        {
            if (AuthenticationMode == null)
            {
                return;
            }

            ClearErrors(nameof(DatabaseUrl));

            var regexPattern = AuthenticationMode.Type == AuthenticationType.MicrosoftAccountOAuth
                ? CLOUD_CONNECTION_REGEX
                : DATABASE_CONNECTION_REGEX;

            var errorMessage = AuthenticationMode.Type == AuthenticationType.MicrosoftAccountOAuth
                ? "Invalid URL format. Please use: https://piwebcloud-service.metrology.zeiss.com/{Your_Database_Instance_Id}. Replace {Your_Database_Instance_Id}."
                : "Invalid URL format. The URL should match the pattern, e.g. http://{Hostname}:{Port}";

            if (!Regex.IsMatch(DatabaseUrl, regexPattern))
            {
                AddError(errorMessage, nameof(DatabaseUrl));
            }
        }
    }
}
