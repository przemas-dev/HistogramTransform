using System;
using System.Collections.Generic;
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
    public partial class OperationBlock : UserControl
    {
        public OperationStep Operation;
        private readonly Action<OperationBlock> _removeOperationBlockAction;

        public OperationBlock(OperationStep operation, Action<OperationBlock> removeOperationBlockAction)
        {
            Operation = operation;
            _removeOperationBlockAction = removeOperationBlockAction;
            InitializeComponent();
            mainGrid.Background = operation switch
            {
                Equalization equalization => Brushes.Aquamarine,
                Stretching stretching => Brushes.Green,
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
            operationNameLabel.Content = operation switch
            {
                Equalization equalization => "Wyrównanie histogramu",
                Stretching stretching => "Rozciągnięcie histogramu",
                _ => throw new ArgumentOutOfRangeException(nameof(operation))
            };
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _removeOperationBlockAction.Invoke(this);
        }
    }
}
