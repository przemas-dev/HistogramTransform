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
    }
}