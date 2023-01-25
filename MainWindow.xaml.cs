using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        private Histogram _histogram;
        private Scale SelectedScale;
        private HistogramDisplayChannel SelectedHistogramChannel;
        public int SelectedOperation { get; set; }

        private Stretching _stretching;

        private ImageData StartParams;
        private BitmapSource StartImage;

        private BitmapSource ActiveImage =>
            OperationSteps.Count != 0 ? OperationSteps.Last().Output.GetBitmapSource() : StartImage;
        private ImageData ActiveParams =>
            OperationSteps.Count != 0 ? OperationSteps.Last().Output : StartParams;

        private List<OperationStep> OperationSteps = new List<OperationStep>();

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Title = "Wybierz obraz";
            openDialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.bmp|" +
                                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                                "Portable Network Graphic (*.png)|*.png|" +
                                "TIFF|*.tif;*.tiff|" +
                                "Bitmap file|*.bmp";
            if (openDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(openDialog.FileName, FileMode.Open))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    var frame = decoder.Frames[0];
                    StartImage = new FormatConvertedBitmap(frame, PixelFormats.Bgra32,null, 0);
                }
                imagePreview.Source = StartImage;
                imagePreview.RenderTransform = new MatrixTransform();
                fileTextBox.Text = openDialog.FileName;
                fileSizeLabel.Content = $"Rozmiar: {StartImage.PixelWidth}px x {StartImage.PixelHeight}px";
                StartParams = StartImage.GetOperationParams();
                RefreshHistogramPreview();
                OperationSteps = new List<OperationStep>();
            }
        }


        private void RefreshHistogramPreview()
        {
            histogramImage.Source = ActiveParams?.GetHistogramImage(SelectedHistogramChannel, SelectedScale);
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveImage == null) return;
            var saveDialog = new SaveFileDialog
            {
                Title = "Zapisz obraz jako",
                Filter = "PNG|*.png|" +
                         "JPEG|*.jpg;*.jpeg|" +
                         "TIFF|*.tif;*.tiff"
            };
            if (saveDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = saveDialog.FilterIndex switch
                {
                    1 => new PngBitmapEncoder(),
                    2 => new JpegBitmapEncoder(),
                    3 => new TiffBitmapEncoder(),
                    _ => throw new ArgumentOutOfRangeException(nameof(saveDialog.FilterIndex),saveDialog.FilterIndex, "Cannot find encoder for given filter index")
                };
                encoder.Frames.Add(BitmapFrame.Create(ActiveImage));
                try
                {
                    using var fileStream = File.Create(saveDialog.FileName);
                    encoder.Save(fileStream);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message, "Błąd zapisu pliku", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveImage == null) return;
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
            RefreshHistogramPreview();
        }

        private void HistogramChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            SelectedHistogramChannel = comboBox.SelectedIndex switch
            {
                0 => HistogramDisplayChannel.Value,
                1 => HistogramDisplayChannel.Red,
                2 => HistogramDisplayChannel.Green,
                3 => HistogramDisplayChannel.Blue,
                4 => HistogramDisplayChannel.RelativeLuminance,
                _ => throw new ArgumentOutOfRangeException()
            };
            RefreshHistogramPreview();
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

        private void AddOperation_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveImage == null) return;
            OperationStep newOperation = SelectedOperation switch
            {
                0 => new Stretching(ActiveParams),
                1 => new Equalization(ActiveParams),
                2 => new Shift(ActiveParams),
                _ => throw new ArgumentOutOfRangeException(nameof(SelectedOperation), SelectedOperation, "")
            };
            operationsStackPanel.Children.Add(new OperationBlock(newOperation, RemoveOperationBlock, RefreshFromOperation));
            OperationSteps.Add(newOperation);
            RefreshHistogramPreview();
            imagePreview.Source = ActiveImage;
        }

        private void NewOperationComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            SelectedOperation = comboBox.SelectedIndex;
        }

        public void RemoveOperationBlock(OperationBlock operationBlock)
        {
            operationsStackPanel.Children.Remove(operationBlock);
            var operdationToRemove = operationBlock.Operation;
            var index = OperationSteps.IndexOf(operdationToRemove);
            OperationSteps.Remove(operdationToRemove);
            
            if (OperationSteps.Count != index)
            {
                OperationSteps[index].Input = operdationToRemove.Input;
                OperationSteps.RefreshOperations(index);
            }
            RefreshHistogramPreview();
            imagePreview.Source = ActiveImage;
        }

        private void RefreshFromOperation(OperationStep operationStep)
        {
            OperationSteps.RefreshOperations(operationStep);
            RefreshHistogramPreview();
            imagePreview.Source = ActiveImage;
        }
    }
}