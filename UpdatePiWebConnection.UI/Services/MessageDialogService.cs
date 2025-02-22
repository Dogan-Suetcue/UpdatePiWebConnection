using System.Windows;

namespace UpdatePiWebConnection.UI.Services
{
    public class MessageDialogService : IMessageDialogService
    {
        public void ShowErrorDialog(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowInfoDialog(string text, string title)
        {
            MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
