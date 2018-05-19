using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Transformation _transformation;
        private double[] _columnWidths;

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
            if (_transformation == null)
                _transformation = new TestMethodTimes();

            _transformation.Input = entries;
            if (_columnWidths == null)
                _columnWidths = new double[_transformation.ColumnNames.Length];

            // https://stackoverflow.com/questions/868204/adding-columns-programatically-to-listview-in-wpf
            DataBind(listView, _transformation, _columnWidths);

            if (_transformation.Output.Any())
                listView.DataContext = _transformation.Output;
        }

        private void DataBind(ListView listView, Transformation transformation, double[] columnWidths)
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

            var colNames = transformation.ColumnNames;
            for (int i = 0; i < colNames.Length; i++)
            {
                var columnName = colNames[i];
                var column = new GridViewColumn();
                column.DisplayMemberBinding = new Binding(columnName);
                column.Header = columnName;
                column.Width = _columnWidths[i] < 1.0d ? 100 : _columnWidths[i];
                ((INotifyPropertyChanged)column).PropertyChanged += OutputWindow_PropertyChanged;

                gridView.Columns.Add(column);
            }

            listView.View = gridView;
        }

        private void OutputWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(sender is GridViewColumn column)
            {
                if (e.PropertyName == "ActualWidth")
                {
                    if(_transformation != null)
                    {
                        var header = column.Header.ToString();
                        var colNames = _transformation.ColumnNames;
                        var i = -1;
                        while (++i < colNames.Length && colNames[i] != header) ;

                        if (i < _columnWidths.Length)
                            _columnWidths[i] = column.ActualWidth;
                    }
                }

            }
        }

    }
}
