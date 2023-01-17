using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HistogramTransform
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private BitmapImage _bitmapImage;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Title = "Wybierz obraz";
            openDialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Portable Network Graphic (*.png)|*.png";
            if (openDialog.ShowDialog() == true)
            {

                _bitmapImage = new BitmapImage(new Uri(openDialog.FileName));
                imagePreview.Source = _bitmapImage;
                fileTextBox.Text = openDialog.FileName;
                fileSizeLabel.Content = $"Rozmiar: {_bitmapImage.PixelWidth}px x {_bitmapImage.PixelHeight}px";
            }

            var histogram = new Histogram(_bitmapImage);
            histogramImage.Source = histogram.GetBitmapImage();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Title = "Zapisz obraz jako";
            saveDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (_bitmapImage != null)
            {
                if (saveDialog.ShowDialog() == true)
                {
                    var jpg = new JpegBitmapEncoder();
                    jpg.Frames.Add(BitmapFrame.Create(_bitmapImage));
                    using (var fileStream = File.Create(saveDialog.FileName))
                    {
                        jpg.Save(fileStream);
                    }
                }
            }
        }
    }
}