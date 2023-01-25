using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HistogramTransform
{
    public partial class OperationBlock : UserControl
    {
        public OperationStep Operation;
        private readonly Action<OperationBlock> _removeOperationBlockAction;
        private readonly Action<OperationStep> _refreshFromOperation;

        public OperationBlock(
            OperationStep operation,
            Action<OperationBlock> removeOperationBlockAction,
            Action<OperationStep> refreshFromOperation)
        {
            Operation = operation;
            _removeOperationBlockAction = removeOperationBlockAction;
            _refreshFromOperation = refreshFromOperation;
            InitializeComponent();
            mainGrid.Background = operation switch
            {
                Equalization equalization => Brushes.Aquamarine,
                Stretching stretching => Brushes.Green,
                Shift shift => Brushes.Goldenrod,
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
            operationNameLabel.Content = operation switch
            {
                Equalization equalization => "Wyrównanie histogramu",
                Stretching stretching => "Rozciągnięcie histogramu",
                Shift shift => "Przesunięcie histogramu",
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
            if (operation is Shift)
            {
                slider.Visibility = Visibility.Visible;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _removeOperationBlockAction.Invoke(this);
        }

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            (Operation as Shift).ShiftValue = (int)slider.Value;
            _refreshFromOperation.Invoke(Operation);
        }
    }
}
