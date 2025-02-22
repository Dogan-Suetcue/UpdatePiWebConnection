using UpdatePiWebConnection.Common;

namespace UpdatePiWebConnection.UI.ViewModel
{
    public class ProgressViewModel : ViewModelBase
    {
        private string _statusMessage;
        private int _currentValue;
        private int _maxValue;

        public ProgressViewModel(int maxValue)
        {
            StatusMessage = "Determine the execution time...";
            MaxValue = maxValue;
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public int CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                OnPropertyChanged();
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            private set { _maxValue = value; }
        }
    }
}
