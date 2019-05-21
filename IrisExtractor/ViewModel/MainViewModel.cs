using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ImageEditor.Filters;
using ImageEditor.Filters.Interfaces;
using ImageEditor.Helpers;
using ImageEditor.ViewModel.Helpers;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace ImageEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region properties
        public bool IsSaveToDbEnabled { get; set; }
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
        public RelayCommand EncodeIrisCommand { get; private set; }
        public RelayCommand LoadIrisFeaturesCommand { get; set; }
        public RelayCommand<object> ApplyDaugmanCommand { get; private set; }
        #endregion

        public MainViewModel()
        {
            OpenFolderCommand = new RelayCommand(OpenFolder);
            ApplyDaugmanCommand = new RelayCommand<object>(ApplyFilter);
            ResetCommand = new RelayCommand(ResetFilter);
            EncodeIrisCommand=new RelayCommand(EncodeIris);
            LoadIrisFeaturesCommand=new RelayCommand(LoadIrisFeatures);
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

        private async void ApplyFilter(object obj)
        {
            foreach (var item in ImageViewItems)
            {
                if (item.ProcessedBitmap == null || item.SpinnerVisibility == Visibility.Visible) return;
                var filter = obj as FilterViewItem;
                item.SpinnerVisibility = Visibility.Visible;
                var filterResult= await ApplyFilterAsync(new Bitmap(item.ProcessedBitmap), filter);
                item.ProcessedBitmap = filterResult.Bitmap;
                item.Pupil = filterResult.Pupil;
                item.Radius = filterResult.Radius;
                RaisePropertyChanged(nameof(item.ProcessedBitmap));
                item.SpinnerVisibility = Visibility.Hidden;
                ProcessedImagesCount++;
                SaveIrisFeaturesToDb(item, filterResult);
            }
        }

        private void SaveIrisFeaturesToDb(ImageViewItem item, FilterResult filterResult)
        {
            if (!IsSaveToDbEnabled) return;
            using (var context = new DaugmanContext())
            {
                context.Photos.AddOrUpdate(new Photo()
                {
                    Path = item.FilePath,
                    CenterX = filterResult.Pupil.X,
                    CenterY = filterResult.Pupil.Y,
                    Radius = filterResult.Radius
                });
                context.SaveChanges();
            }
        }

        private async Task<FilterResult> ApplyFilterAsync(Bitmap b, FilterViewItem filterItem)
        {
            return await Task.Run(delegate
            {
                return filterItem.Filter.Filter(b);
            });
        }

        private void LoadIrisFeatures()
        {
            using (var context=new DaugmanContext())
            {
                foreach (var item in ImageViewItems)
                {
                    var irisFeature = context.Photos.Find(item.FilePath);
                    var tmp = new Bitmap(item.ProcessedBitmap);
                   
                    var pupil = new Point(irisFeature.CenterX, irisFeature.CenterY);
                    var radius = irisFeature.Radius;
                    item.Pupil = pupil;
                    item.Radius = radius;

                    MarkPoint(tmp,pupil,Color.Yellow);
                    foreach (var p in pupil.GetCircularPoints(radius, Math.PI / 17.0f))
                    {
                        if (p.Y + 1 >= tmp.Height || p.Y - 1 < 0 || p.X - 1 < 0 || p.X + 1 >=tmp.Width)
                            continue;
                        MarkPoint(tmp,p, Color.Red);
                    }

                    item.ProcessedBitmap = tmp;
                }
            }          
        }

        private void EncodeIris()
        {
            foreach (var item in ImageViewItems)
            {
                var radius = item.Radius;
                var pupil = item.Pupil;
                var height = item.OriginalBitmap.Height;
                var width = item.OriginalBitmap.Width;

                Bitmap bmp = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.AntiqueWhite); }

                var pointsWithDistanceToPupil = GetPointsInsideIris(radius, pupil);
                foreach (var p in pointsWithDistanceToPupil)
                {
                    var angle = pupil.GetAngle(p.point);
                    var newPoint = PointExtensions.PolarToCartesian(angle, p.distance);
                    var color = item.OriginalBitmap.GetPixel(p.point.X, p.point.Y);
                    bmp.SetPixel(p.point.X, p.point.Y, color);
                }

                item.ProcessedBitmap = bmp;
            }           
        }

        private List<(Point point, double distance)> GetPointsInsideIris(int radius, Point pupil)
        {
            var points = new List<(Point,double)>();
            for (int i = pupil.X - radius; i < pupil.X + radius; i++)
            {
                for (int j = pupil.Y - radius; j < pupil.Y + radius; j++)
                {
                    var point = new Point(i, j);
                    var distance = Math.Sqrt(Math.Pow(i - pupil.X, 2) + Math.Pow(j - pupil.Y, 2));
                    if (distance > radius) continue;
                    points.Add((point,distance));
                }
            }

            return points;
        }

        private void MarkPoint(Bitmap bitmap,Point p, Color color)
        {
            bitmap.SetPixel(p.X, p.Y, color);
            bitmap.SetPixel(p.X - 1, p.Y, color);
            bitmap.SetPixel(p.X + 1, p.Y, color);
            bitmap.SetPixel(p.X - 1, p.Y + 1, color);
            bitmap.SetPixel(p.X, p.Y + 1, color);
            bitmap.SetPixel(p.X + 1, p.Y + 1, color);
            bitmap.SetPixel(p.X - 1, p.Y - 1, color);
            bitmap.SetPixel(p.X, p.Y - 1, color);
            bitmap.SetPixel(p.X + 1, p.Y - 1, color);
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
