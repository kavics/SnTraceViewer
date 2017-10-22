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
    /// Interaction logic for FiltersWindow.xaml
    /// </summary>
    public partial class FiltersWindow : Window
    {
        private CategoryVisibility _categoryVisibility;

        public FiltersWindow(CategoryVisibility categoryVisibility)
        {
            InitializeComponent();

            _categoryVisibility = categoryVisibility;

            this.DataContext = categoryVisibility;
        }

        private void AllOnButton_Click(object sender, RoutedEventArgs e)
        {
            _categoryVisibility.AllOn();
        }

        private void AllOffButton_Click(object sender, RoutedEventArgs e)
        {
            _categoryVisibility.AllOff();
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }

        private bool _exit;
        internal void Exit()
        {
            _exit = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_exit)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
