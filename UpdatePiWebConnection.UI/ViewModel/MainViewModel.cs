using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using UpdatePiWebConnection.Common;
using UpdatePiWebConnection.UI.Model;
using UpdatePiWebConnection.UI.Services;
using UpdatePiWebConnection.UI.View;

namespace UpdatePiWebConnection.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string IMAGE_PATH_PREFIX = "/Images/";
        private const string NO_AUTH_IMAGE = IMAGE_PATH_PREFIX + "padlock-half-half.png";
        private const string WINDOWS_AUTH_IMAGE = IMAGE_PATH_PREFIX + "platform-windows.png";
        private const string CERTIFICATE_AUTH_IMAGE = IMAGE_PATH_PREFIX + "certificate.png";
        private const string INTERNET_AUTH_IMAGE = IMAGE_PATH_PREFIX + "internet.png";

        private const string PIWEB_REGISTRY_PATH = @"Software\Zeiss\PiWeb";
        private const string PIWEB_REGISTRY_KEY_NAME = "Path";
        private readonly string DATABASE_CONNECTION_FILE = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZEISS", "PiWeb", "DatabaseConnections.xml");

        private string _cmdmonPath = string.Empty;
        private readonly IMessageDialogService _messageDialogService;
        private Connection _databaseConnectionSettings;
        private AuthenticationMode _selectedAuthenticationMode;

        public MainViewModel(IMessageDialogService messageDialogService)
        {
            _messageDialogService = messageDialogService;

            CheckPrerequisite();
            InitializeCommands();
            InitializeAuthenticationModes();
            InitializeDatabaseConnectionSettings();
        }

        public Connection Connection
        {
            get => _databaseConnectionSettings;
            set
            {
                _databaseConnectionSettings = value;
                OnPropertyChanged();
                UpdateCommand.OnCanExecuteChanged();
            }
        }

        public List<AuthenticationMode> AuthenticationModes { get; private set; }

        public AuthenticationMode SelectedAuthenticationMode
        {
            get => _selectedAuthenticationMode;
            set
            {
                _selectedAuthenticationMode = value;
                OnPropertyChanged();

                Connection.AuthenticationMode = _selectedAuthenticationMode;

                if (!Connection.DefaultDatabaseConnection)
                {
                    Connection.DatabaseUrl = string.Empty;
                }

                if (SelectedAuthenticationMode.Type == AuthenticationType.MicrosoftAccountOAuth)
                {
                    Connection.DatabaseUrl = "https://piwebcloud-service.metrology.zeiss.com/{Your_Database_Instance_Id}";
                }

                UpdateCommand.OnCanExecuteChanged();
            }
        }

        public DelegateCommand DefaultConnectionCommand { get; private set; }

        public DelegateCommand OpenFolderBrowserDialogCommand { get; private set; }

        public DelegateCommand UpdateCommand { get; private set; }

        public DelegateCommand ClearCommand { get; private set; }

        public DelegateCommand CloseCommand { get; private set; }


        private void CheckPrerequisite()
        {
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(PIWEB_REGISTRY_PATH))
            {
                if (key == null)
                {
                    _messageDialogService
                        .ShowErrorDialog("PiWeb is not installed on this system. Please install the PiWeb Cmdmon.exe module.", "Cmdmon");
                    Application.Current.Shutdown();
                    return;
                }

                var value = key.GetValue(PIWEB_REGISTRY_KEY_NAME, null);
                if (value == null)
                {
                    _messageDialogService
                        .ShowErrorDialog("PiWeb Cmdmon.exe is a prerequisite for changing the connection of the reports. Please install Cmdmon.exe.", "Cmdmon");
                    Application.Current.Shutdown();
                    return;
                }

                _cmdmonPath = Path.Combine(value.ToString(), "Cmdmon.exe");
                if (!File.Exists(_cmdmonPath))
                {
                    _messageDialogService
                        .ShowErrorDialog("PiWeb Cmdmon.exe is a prerequisite for changing the connection of the reports. Please install Cmdmon.exe.", "Cmdmon");
                    Application.Current.Shutdown();
                    return;
                }
            }
        }

        private void InitializeCommands()
        {
            DefaultConnectionCommand = new DelegateCommand(InitializeForm);
            OpenFolderBrowserDialogCommand = new DelegateCommand(OpenFolderBrowserDialog);
            UpdateCommand = new DelegateCommand(Update, CanUpdate);
            ClearCommand = new DelegateCommand(Clear);
            CloseCommand = new DelegateCommand(Close);
        }

        private void InitializeAuthenticationModes()
        {
            AuthenticationModes = new List<AuthenticationMode>
            {
                new AuthenticationMode
                {
                    Image = NO_AUTH_IMAGE,
                    Type = AuthenticationType.NoneOrBasic,
                    DisplayName = "No or username password authentication"
                },
                new AuthenticationMode
                {
                    Image = WINDOWS_AUTH_IMAGE,
                    Type = AuthenticationType.Windows,
                    DisplayName = "Integrated Windows authentication"
                },
                new AuthenticationMode
                {
                    Image = CERTIFICATE_AUTH_IMAGE,
                    Type = AuthenticationType.CertificateHardware,
                    DisplayName = "Hardware certificate authentication"
                },
                new AuthenticationMode
                {
                    Image = CERTIFICATE_AUTH_IMAGE,
                    Type = AuthenticationType.CertificateAskUser,
                    DisplayName = "Authentication by selecting a certificate"
                },
                new AuthenticationMode
                {
                    Image = INTERNET_AUTH_IMAGE,
                    Type = AuthenticationType.MicrosoftAccountOAuth,
                    DisplayName = "Web login authentication"
                }
            };
        }

        private void InitializeDatabaseConnectionSettings()
        {
            Connection = new Connection
            {
                DefaultDatabaseConnection = true,
                Directory = Directory.GetParent(Application.ResourceAssembly.Location).FullName
            };
            Connection.ErrorsChanged += (sender, e) => UpdateCommand.OnCanExecuteChanged();

            if (File.Exists(DATABASE_CONNECTION_FILE))
            {
                var document = new XmlDocument();
                document.Load(DATABASE_CONNECTION_FILE);
                var defaultDatabaseConnectionNode = document.SelectSingleNode("//DatabaseConnections/DatabaseConnection");
                if (defaultDatabaseConnectionNode != null)
                {
                    Connection.DatabaseName = defaultDatabaseConnectionNode.Attributes["Name"].Value;
                    Connection.DatabaseUrl = defaultDatabaseConnectionNode.Attributes["DatabaseUrl"].Value;
                    var authenticationMode = defaultDatabaseConnectionNode.Attributes["ClientAuthenticationMode"];
                    if (authenticationMode != null)
                    {
                        SelectedAuthenticationMode = AuthenticationModes
                            .SingleOrDefault(a => a.Type == (AuthenticationType)Enum.Parse(typeof(AuthenticationType), authenticationMode.Value));

                        Connection.AuthenticationMode = SelectedAuthenticationMode;
                    }
                }
            }
            else
            {
                Connection.DefaultDatabaseConnection = false;
            }
        }

        private void InitializeForm(object obj)
        {
            if (Connection.DefaultDatabaseConnection)
            {
                InitializeDatabaseConnectionSettings();
            }
            else
            {
                Connection = new Connection
                {
                    DefaultDatabaseConnection = false,
                    Directory = string.Empty,
                    DatabaseName = string.Empty,
                    DatabaseUrl = string.Empty,
                    AuthenticationMode = AuthenticationModes.FirstOrDefault()
                };
                SelectedAuthenticationMode = AuthenticationModes.FirstOrDefault();
                Connection.ErrorsChanged += (sender, e) => UpdateCommand.OnCanExecuteChanged();
            }
        }

        private void OpenFolderBrowserDialog(object obj)
        {
            using (var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog
            { ShowNewFolderButton = false, Description = "Select PiWeb Reports Folder" }
            )
            {
                folderBrowserDialog.ShowDialog();
                if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                {
                    Connection.Directory = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private async void Update(object obj)
        {
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Visibility = Visibility.Collapsed;

            var reports = Directory.GetFiles(Connection.Directory, @"*.ptx", SearchOption.AllDirectories);
            var progressViewModel = new ProgressViewModel(reports.Length);
            var progressView = new ProgressView { DataContext = progressViewModel };
            progressView.Show();

            var processStartInfo = new ProcessStartInfo { FileName = _cmdmonPath, WindowStyle = ProcessWindowStyle.Hidden };

            var sumOfExecutionTime = 0;
            var currentFileCount = 1;

            try
            {
                foreach (var report in reports)
                {
                    try
                    {
                        var executionTime = await RunProcessAsync(processStartInfo, report);
                        sumOfExecutionTime += executionTime;

                        var averageTime = (int)Math.Ceiling((double)(sumOfExecutionTime / currentFileCount));
                        var duration = averageTime * (reports.Length - currentFileCount);

                        UpdateProgressView(progressViewModel, currentFileCount, reports.Length, averageTime, duration);
                        currentFileCount++;
                    }
                    catch (Exception ex)
                    {
                        _messageDialogService.ShowErrorDialog($"Error while processing the file: {report}\n{ex.Message}", "Error");
                        break;
                    }
                }

                _messageDialogService.ShowInfoDialog("The database connection of the reports has been updated.", "Database connection");
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowErrorDialog($"A critical error has occurred: {ex.Message}", "Critical error");
            }
            finally
            {
                progressView.Close();
                mainWindow.Visibility = Visibility.Visible;
            }
        }

        private async Task<int> RunProcessAsync(ProcessStartInfo processStartInfo, string report)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            processStartInfo.Arguments = GenerateProcessArguments(report);

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("The process could not be started.");
                }

                await Task.Run(() => process.WaitForExit());
            }

            stopWatch.Stop();
            return (int)stopWatch.Elapsed.TotalSeconds;
        }

        private string GenerateProcessArguments(string report)
        {
            return $@"-open ""{report}"" " +
                   (Connection.DefaultDatabaseConnection ? "-setDefaultConnection" :
                   $@"-databaseName ""{Connection.DatabaseName}"" -setDatabaseUrl {Connection.DatabaseUrl} -clientAuthenticationMode {SelectedAuthenticationMode.Type}");
        }

        private void UpdateProgressView(ProgressViewModel progressViewModel, int currentFileCount, int totalFiles, int averageTime, int duration)
        {
            var statusMessage = $"Edit file:\t\t\t{currentFileCount} of {totalFiles}" + Environment.NewLine +
                                $"Average time:\t\t{averageTime} seconds" + Environment.NewLine +
                                $"Expected duration:\t{duration} seconds";

            progressViewModel.StatusMessage = statusMessage;
            progressViewModel.CurrentValue = currentFileCount;
        }

        private bool CanUpdate(object arg) => !Connection.HasErrors;

        private void Clear(object obj) => InitializeForm(null);

        private void Close(object obj) => Application.Current.Shutdown();
    }
}