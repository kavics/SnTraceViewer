using SnTraceViewer.Transformations;
using SnTraceViewer.Transformations.Builtin;
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
using System.Windows.Shapes;

namespace SnTraceViewer
{
    /// <summary>
    /// Interaction logic for TransformationsWindow.xaml
    /// </summary>
    public partial class TransformationsWindow : Window
    {
        public Transformation SelectedTransformation { get; private set; }

        public TransformationsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (sender == Button1)
                SelectedTransformation = new TestMethodTimes();
            if (sender == Button2)
                SelectedTransformation = new SaveContentTimes();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTransformation = null;
            Close();
        }
    }
}
