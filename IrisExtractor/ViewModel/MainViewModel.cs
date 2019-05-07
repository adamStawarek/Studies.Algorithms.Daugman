using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageEditor.Filters;
using ImageEditor.Filters.Interfaces;
using ImageEditor.ViewModel.Helpers;
using Microsoft.Win32;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Linq;

namespace ImageEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region properties
        public FilterViewItem FilterItem => new FilterViewItem(new Daugman());
        public ObservableCollection<ImageViewItem> ImageViewItems { get; set; }

        private string _rgbVal;       
        public string RgbVal
        {
            get => _rgbVal;
            set
            {
                _rgbVal = value;
                RaisePropertyChanged("RgbVal");
            }
        }
        #endregion

        #region relay commands
        public RelayCommand OpenFolderCommand { get; private set; }
        public RelayCommand ClearFiltersCommand { get; private set; }
        public RelayCommand<object> ApplyFilterCommand { get; private set; }
        public RelayCommand<object> SetProjectionCommand { get; set; }
        public RelayCommand<object> OpenPopupCommand { get; private set; }
        #endregion

        public MainViewModel()
        {
            OpenFolderCommand = new RelayCommand(OpenFolder);
            ApplyFilterCommand = new RelayCommand<object>(ApplyFilter);           
            OpenPopupCommand = new RelayCommand<object>(SetCurrentPixelValuesToRgbBox);
            ClearFiltersCommand = new RelayCommand(ResetFilter);
            ImageViewItems=new ObservableCollection<ImageViewItem>();

            if (FilterItem.Filter is IError e)
            {
                e.ErrorOccured += delegate { FilterItem.ErrorMessage = e.ErrorMessage; };
                e.NoErrorOccured += delegate { FilterItem.ErrorMessage = e.ErrorMessage; };
            }
        }

        private void OpenFolder()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var files=Directory.GetFiles(dialog.FileName, "*.jpg", SearchOption.AllDirectories);
                SetUpImageViews(files);
                FilterItem.ErrorMessage = "";
            }
        }

        private void SetUpImageViews(string[] files)
        {
            ImageViewItems.Clear();
            foreach (var file in files)
            {
                ImageViewItems.Add(new ImageViewItem()
                {
                    FilePath = file,
                    OriginalBitmap = new Bitmap(file),
                    ProcessedBitmap = new Bitmap(file),
                    SpinnerVisibility = Visibility.Hidden
                });
            }           
        }

        private void SetCurrentPixelValuesToRgbBox(object obj)
        {
            var color = ColorUnderCursor.Get();
            POINT p;
            ColorUnderCursor.GetCursorPos(out p);
            RgbVal = "Red: " + color.B + "   Green: " + color.G + "   Blue: " + color.R;
        }

        private async void ApplyFilter(object obj)
        {
            foreach (var item in ImageViewItems)
            {
                if (item.ProcessedBitmap == null || item.SpinnerVisibility == Visibility.Visible) return;
                var filter = obj as FilterViewItem;
                item.SpinnerVisibility = Visibility.Visible;
                item.ProcessedBitmap = await ApplyFilterAsync(new Bitmap(item.ProcessedBitmap), filter);
                RaisePropertyChanged(nameof(item.ProcessedBitmap));
                item.SpinnerVisibility = Visibility.Hidden;
            }
        
        }

        private async Task<Bitmap> ApplyFilterAsync(Bitmap b, FilterViewItem filterItem)
        {
            await Task.Run(delegate
            {
                filterItem.Filter.Filter(b);
            });
            return b;
        }

        private void ResetFilter()
        {
            foreach (var item in ImageViewItems)
            {
                if (item.ProcessedBitmap== null || item.SpinnerVisibility == Visibility.Visible) return;
                item.ProcessedBitmap = new Bitmap(item.OriginalBitmap);
                RaisePropertyChanged(nameof(item.ProcessedBitmap));
            }         
        }
    }
}
