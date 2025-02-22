namespace UpdatePiWebConnection.UI.Services
{
    public interface IMessageDialogService
    {
        void ShowInfoDialog(string text, string title);
        void ShowErrorDialog(string text, string title);
    }
}
