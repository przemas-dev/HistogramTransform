using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HistogramTransform
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private Point imageOffset; 
        private Point startMouse;
        
        private BitmapImage _bitmapImage;
        private Histogram _histogram;
        private Scale _selectedScale;

        public Scale SelectedScale
        {
            get => _selectedScale;
            private set
            {
                if (value == _selectedScale) return;
                _selectedScale = value;
                if (_histogram != null)
                {
                    _histogram.ChangeScale(_selectedScale);
                    histogramImage.Source = _histogram.GetBitmapImage();
                }
            }

}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Title = "Wybierz obraz";
            openDialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphic (*.png)|*.png";
            if (openDialog.ShowDialog() == true)
            {

                _bitmapImage = new BitmapImage(new Uri(openDialog.FileName));
                imagePreview.Source = _bitmapImage;
                imagePreview.RenderTransform = new MatrixTransform();
                fileTextBox.Text = openDialog.FileName;
                fileSizeLabel.Content = $"Rozmiar: {_bitmapImage.PixelWidth}px x {_bitmapImage.PixelHeight}px";
                _histogram = new Histogram(_bitmapImage,_selectedScale);
                histogramImage.Source = _histogram.GetBitmapImage();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (_bitmapImage == null) return;
            var saveDialog = new SaveFileDialog
            {
                Title = "Zapisz obraz jako",
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp"
            };
            if (saveDialog.ShowDialog() == true)
            {
                var jpg = new JpegBitmapEncoder();
                jpg.Frames.Add(BitmapFrame.Create(_bitmapImage));
                try
                {
                    using var fileStream = File.Create(saveDialog.FileName);
                    jpg.Save(fileStream);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message, "Błąd zapisu pliku", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (_bitmapImage == null) return;
            var saveDialog = new SaveFileDialog
            {
                Title = "Zapisz obraz jako",
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp"
            };
            if (saveDialog.ShowDialog() == true)
            {
                _histogram.SaveHistogramToFile(saveDialog.FileName);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            SelectedScale = comboBox.SelectedIndex switch
            {
                0 => Scale.Logarithmic,
                1 => Scale.Linear,
                _ => Scale.Logarithmic
            };
            
        }
        
        private void ImagePreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (imagePreview.IsMouseCaptured) return;
            imagePreview.CaptureMouse();

            startMouse = e.GetPosition(border);
            imageOffset.X = imagePreview.RenderTransform.Value.OffsetX;
            imageOffset.Y = imagePreview.RenderTransform.Value.OffsetY;
        }
        private void ImagePreview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!imagePreview.IsMouseCaptured) return;
            imagePreview.ReleaseMouseCapture();
        }

        private void ImagePreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imagePreview.IsMouseCaptured) return;
            var point = e.MouseDevice.GetPosition(border);

            var matrix = imagePreview.RenderTransform.Value;
            matrix.OffsetX = imageOffset.X + (point.X - startMouse.X);
            matrix.OffsetY = imageOffset.Y + (point.Y - startMouse.Y);

            imagePreview.RenderTransform = new MatrixTransform(matrix);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var point = e.MouseDevice.GetPosition(imagePreview);
            var matrix = imagePreview.RenderTransform.Value;
            if (e.Delta > 0)
            {
                matrix.ScaleAtPrepend(1.1, 1.1, point.X, point.Y);
            }
            else
            {
                matrix.ScaleAtPrepend(1 / 1.1, 1 / 1.1, point.X, point.Y);
            }
            imagePreview.RenderTransform = new MatrixTransform(matrix);
        }
    }
}