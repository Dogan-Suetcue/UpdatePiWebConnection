using System.Windows;
using UpdatePiWebConnection.UI.Services;
using UpdatePiWebConnection.UI.ViewModel;

namespace UpdatePiWebConnection.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel(new MessageDialogService());
        }
    }
}
