using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageEditor.Annotations;
using ImageEditor.Filters.Interfaces;

namespace ImageEditor.ViewModel.Helpers
{
    public class FiltersListViewItem:INotifyPropertyChanged
    {
        private int _applicationCounter;       
        public int ApplicationCounter
        {
            get => _applicationCounter;
            set
            {
                if (value == _applicationCounter) return;
                _applicationCounter = value;
                OnPropertyChanged();
            }
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (value == _errorMessage) return;
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        public IFilter Filter { get; set; }  
        public FiltersListViewItem( IFilter filter)
        {
            Filter = filter;
        }       

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }    
        #endregion
    }
}
