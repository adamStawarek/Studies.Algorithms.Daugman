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
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace ImageEditor.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region fields
        private const float degrees = 30.0f;
        private const int countFirstPixelsToSkip = 10;
        private const int countLastPixelsToSkip = 5;
        #endregion

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
        public RelayCommand ClassifyCommand { get; private set; }
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
            EncodeIrisCommand = new RelayCommand(EncodeIris);
            LoadIrisFeaturesCommand = new RelayCommand(LoadIrisFeatures);
            ClassifyCommand = new RelayCommand(Classify);
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
                var filterResult = await ApplyFilterAsync(new Bitmap(item.ProcessedBitmap), filter);
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
            using (var context = new DaugmanContext())
            {
                foreach (var item in ImageViewItems)
                {
                    var irisFeature = context.Photos.Find(item.FilePath);
                    var tmp = new Bitmap(item.ProcessedBitmap);

                    var pupil = new Point(irisFeature.CenterX, irisFeature.CenterY);
                    var radius = irisFeature.Radius;
                    item.Pupil = pupil;
                    item.Radius = radius;
                    item.Bytes = irisFeature.Encoded;

                    MarkPoint(tmp, pupil, Color.Yellow);
                    foreach (var p in pupil.GetCircularPoints(radius, Math.PI / 17.0f))
                    {
                        if (p.Y + 1 >= tmp.Height || p.Y - 1 < 0 || p.X - 1 < 0 || p.X + 1 >= tmp.Width)
                            continue;
                        MarkPoint(tmp, p, Color.Red);
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
                var countPixelsToSkip = countFirstPixelsToSkip + countLastPixelsToSkip;

                Bitmap bmp = new Bitmap(radius - countPixelsToSkip, (int)degrees);
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.White); }

                var cartesian = new byte[radius - countPixelsToSkip][];
                for (int i = 0; i < radius - countPixelsToSkip; i++)
                    cartesian[i] = new byte[(int)degrees];

                var counter = 0;
                var interval = Math.PI / degrees;
                for (double fi = 0; fi < 2 * Math.PI; fi += interval)
                {
                    for (var r = countFirstPixelsToSkip; r < radius - countLastPixelsToSkip; r++)
                    {
                        var point = pupil.PolarToCartesian(r, fi);
                        cartesian[r - countFirstPixelsToSkip][counter] = item.OriginalBitmap.GetPixel(point.X, point.Y).GetGreyscale();
                    }                                     
                    counter++;
                    if (counter==(int)degrees)
                        break;                   
                }

                for (int i = 0; i < radius - countPixelsToSkip; i++)
                {
                    for (int j = 0; j < degrees; j++)
                    {
                        var c = cartesian[i][j];
                        bmp.SetPixel(i, j, Color.FromArgb(c, c, c));
                    }
                }


                item.EncodedBitmap = new Bitmap(bmp);
                var bytes = ChunkEncoding(item.EncodedBitmap);
                item.Bytes = bytes.ToArray();
                using (var context = new DaugmanContext())
                {
                    var photo = context.Photos.Find(item.FilePath);
                    photo.Encoded = bytes.ToArray();
                    context.SaveChanges();
                }
            }
        }

        private IEnumerable<byte> ChunkEncoding(Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;
            double global_deviation = CalculateMean(0, 0, width, height, image);
            double global_variance = CalculateVariance(0, 0, width, height, image, global_deviation);
            var list_of_deviance_variance_tuples = new List<(double, double)>();
            var y_step = 10;
            var x_step = 10;

            Queue<byte> byte_code = new Queue<byte>();

            for (var y = 0; y < height - y_step + 1; y += y_step)
            {
                for (var x = 0; x < width - x_step + 1; x += x_step)
                {
                    var mean = (double)CalculateMean(x, y, x_step, y_step, image);
                    var variance = (double)CalculateVariance(x, y, x_step, y_step, image, mean);
                    list_of_deviance_variance_tuples.Add((mean, variance));                    
                }
            }

            for (int i = 0; i < list_of_deviance_variance_tuples.Count; i++)
            {
                var _tuple = list_of_deviance_variance_tuples[i];
                if (_tuple.Item1 <= global_deviation)
                {
                    byte_code.Enqueue(0);
                }
                else
                {
                    byte_code.Enqueue(1);
                }


                if (_tuple.Item2 <= global_variance)
                {
                    byte_code.Enqueue(0);
                }
                else
                {
                    byte_code.Enqueue(1);
                }

                if (i + 1 < list_of_deviance_variance_tuples.Count)
                {
                    var next_tuple = list_of_deviance_variance_tuples[i + 1];
                    if (_tuple.Item1 <= next_tuple.Item1)
                        byte_code.Enqueue(0);
                    else
                        byte_code.Enqueue(1);

                    if (_tuple.Item2 <= next_tuple.Item2)
                    {
                        byte_code.Enqueue(0);
                    }
                    else
                    {
                        byte_code.Enqueue(1);
                    }
                }
                else
                {
                    byte_code.Enqueue(0);
                    byte_code.Enqueue(0);
                }

            }

            return byte_code;
        }

        private double CalculateVariance(int startX, int startY, int width, int height, Bitmap image, double mean)
        {
            double sum = 0;
            for (int i = startX; i < startX+width; i++)
            {
                for (int j = startY; j < startY+height; j++)
                {
                    sum += Math.Pow(image.GetPixel(i, j).GetGreyscale(), 2);
                }
            }

            return Math.Sqrt((sum / (width * height)) - Math.Pow(mean, 2));
        }

        private double CalculateMean(int startX, int startY, int width, int height, Bitmap image)
        {
            double sum = 0;
            for (int i = startX; i < startX+width - 1; i++)
            {
                for (int j = startY; j < startY+height - 1; j++)
                {
                    sum += image.GetPixel(i, j).GetGreyscale();

                }
            }

            return sum / (width * height);
        }

        private void Classify()
        {
            var distinctByteCounts = ImageViewItems.Select(i=>i.Bytes.Length).Distinct();

            foreach (var byteCount in distinctByteCounts)
            {
                var trainSet = ImageViewItems.Where(d => d.Bytes.Length == byteCount)
                    .Select(i => (i.FilePath.TrimRightFromChar(), i.Bytes));
                foreach (var item in ImageViewItems.Where(d => d.Bytes.Length == byteCount))
                {
                    var itemGroup = item.FilePath.TrimRightFromChar();
                    var predictedGroups = Knn.ClassifyVector(trainSet, item.Bytes, 3);

                    item.IsClassifiedCorrectly = (predictedGroups.Contains(itemGroup));
                }
            }            
            var properCount = ImageViewItems.Count(i => (bool)i.IsClassifiedCorrectly);
            var inproperCount = ImageViewItems.Count(i => (bool)!i.IsClassifiedCorrectly);
            MessageBox.Show($"Classyfied correctly: {properCount}, incorectly: {inproperCount}");
        }

        private List<(Point point, double distance)> GetPointsInsideIris(int radius, Point pupil)
        {
            var points = new List<(Point, double)>();
            for (int i = pupil.X - radius; i < pupil.X + radius; i++)
            {
                for (int j = pupil.Y - radius; j < pupil.Y + radius; j++)
                {
                    var point = new Point(i, j);
                    var distance = Math.Sqrt(Math.Pow(i - pupil.X, 2) + Math.Pow(j - pupil.Y, 2));
                    if (distance > radius) continue;
                    points.Add((point, distance));
                }
            }

            return points;
        }

        private void MarkPoint(Bitmap bitmap, Point p, Color color)
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
