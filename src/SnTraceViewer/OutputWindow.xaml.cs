using SenseNet.Diagnostics.Analysis;
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
            var transformation = new TestMethodTimes(entries);

            // https://stackoverflow.com/questions/868204/adding-columns-programatically-to-listview-in-wpf
            DataBind(listView, transformation);

            if (transformation.Output.Any())
                listView.DataContext = transformation.Output;
        }

        private void DataBind(ListView listView, ITransformation transformation)
        {
            var firstObject = transformation.Output.FirstOrDefault();

            var gridView = listView.View as GridView ?? new GridView();
            gridView.AllowsColumnReorder = true;
            gridView.ColumnHeaderToolTip = transformation.Name;
            gridView.Columns.Clear();

            if (firstObject == null)
            {
                var column = new GridViewColumn();
                column.DisplayMemberBinding = new Binding("text");
                column.Header = string.Empty;
                column.Width = 300;
                gridView.Columns.Add(column);

                listView.View = gridView;

                listView.DataContext = new[] { new { text = "There are no transformed items." } };
            }

            foreach (var columnName in transformation.ColumnNames)
            {
                var column = new GridViewColumn();
                column.DisplayMemberBinding = new Binding(columnName);
                column.Header = columnName;
                //column.Width = 100;
                gridView.Columns.Add(column);
            }

            listView.View = gridView;
        }
    }
}
