using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageEditor.Annotations;
using ImageEditor.Filters.Interfaces;

namespace ImageEditor.ViewModel.Helpers
{
    public class FilterViewItem:INotifyPropertyChanged
    {
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
        public FilterViewItem( IFilter filter)
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
