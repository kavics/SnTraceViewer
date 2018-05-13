using SenseNet.Diagnostics.Analysis2;
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
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : Window
    {
        //private List<DisplayEntry> _entries;
        //internal List<DisplayEntry> Entries
        //{
        //    get => _entries;
        //    set
        //    {
        //        _entries = value;
        //        var transform = new TestMethodTimes(value);
        //        listView.DataContext = transform.Output;
        //    }
        //}
        
        public OutputWindow()
        {
            InitializeComponent();
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

        public void SetEntries(IEnumerable<Entry> entries)
        {
            var transform = new TestMethodTimes(entries);
            listView.DataContext = transform.Output;
        }
    }
}
