using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageEditor.Filters;
using ImageEditor.Filters.Interfaces;
using ImageEditor.ViewModel.Helpers;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows.Threading;

namespace ImageEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        
        #region properties
        public FilterViewItem FilterItem => new FilterViewItem(new Daugman());
        public ObservableCollection<ImageViewItem> ImageViewItems { get; set; }
        private int _processedImagesCount;
        public int ProcessedImagesCount
        {
            get => _processedImagesCount;
            set
            {
                _processedImagesCount = value;
                RaisePropertyChanged(nameof(ProcessedImagesCount));
            }
        }
        #endregion

        #region relay commands
        public RelayCommand OpenFolderCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand<object> ApplyDaugmanCommand { get; private set; }
        #endregion

        public MainViewModel()
        {
            OpenFolderCommand = new RelayCommand(OpenFolder);
            ApplyDaugmanCommand = new RelayCommand<object>(ApplyFilter);
            ResetCommand = new RelayCommand(ResetFilter);
            ImageViewItems = new ObservableCollection<ImageViewItem>();

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
                var files = Directory.GetFiles(dialog.FileName, "*.jpg", SearchOption.AllDirectories);
                SetUpImageViews(files);
                FilterItem.ErrorMessage = "";
                ProcessedImagesCount = 0;
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

        private void ApplyFilter(object obj)
        {
            Parallel.ForEach(ImageViewItems,async item =>
            {
                if (item.ProcessedBitmap == null || item.SpinnerVisibility == Visibility.Visible) return;
                var filter = obj as FilterViewItem;
                Dispatcher.CurrentDispatcher.Invoke(() => item.SpinnerVisibility = Visibility.Visible);

                var bitmap = await ApplyFilterAsync(new Bitmap(item.ProcessedBitmap), FilterItem);
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    item.ProcessedBitmap = bitmap;
                    RaisePropertyChanged(nameof(item.ProcessedBitmap));
                    item.SpinnerVisibility = Visibility.Hidden;
                    ProcessedImagesCount++;
                });
            });          
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
                if (item.ProcessedBitmap == null || item.SpinnerVisibility == Visibility.Visible) return;
                item.ProcessedBitmap = new Bitmap(item.OriginalBitmap);
                RaisePropertyChanged(nameof(item.ProcessedBitmap));
            }

            ProcessedImagesCount = 0;
        }
    }
}
