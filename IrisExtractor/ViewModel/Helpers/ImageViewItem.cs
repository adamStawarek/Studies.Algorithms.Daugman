using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImageEditor.ViewModel.Helpers
{
    public class ImageViewItem:INotifyPropertyChanged
    {
        public string FilePath { get; set; }

        public Bitmap OriginalBitmap { get; set; }

        private Bitmap _processedBitmap;
        public Bitmap ProcessedBitmap
        {
            get => _processedBitmap;
            set
            {
                _processedBitmap = value; 
                OnPropertyChanged(nameof(ProcessedBitmap));
            }
        }

        private Visibility _spinnerVisibility = Visibility.Hidden;
        public Visibility SpinnerVisibility
        {
            get => _spinnerVisibility;
            set
            {
                _spinnerVisibility = value;
                OnPropertyChanged(nameof(SpinnerVisibility));
            }
        }

        #region property changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
        #endregion
    }
}