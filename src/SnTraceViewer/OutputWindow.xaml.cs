using SenseNet.Diagnostics.Analysis;
using SnTraceViewer.Transformations;
using SnTraceViewer.Transformations.Builtin;
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
        private Transformation __transformation;
        private Transformation Transformation
        {
            get { return __transformation; }
            set
            {
                var entries = __transformation == null ? null : this.Transformation.Input;

                if (value != null)
                {
                    __transformation = value;
                    SelectedTransformationLabel.Content = __transformation.Name;
                    if (entries != null)
                        SetEntries(entries);
                }
            }
        }

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
            if (this.Transformation == null)
            {
                this.Transformation = new TestMethodTimes(); // new SaveContentTimes(); // new TestMethodTimes();
            }

            this.Transformation.Input = entries;
            _columnWidths = new double[this.Transformation.ColumnNames.Length];

            // https://stackoverflow.com/questions/868204/adding-columns-programatically-to-listview-in-wpf
            DataBind(listView, this.Transformation, _columnWidths);

            if (this.Transformation.Output.Any())
                listView.DataContext = this.Transformation.Output;
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
                    if(this.Transformation != null)
                    {
                        var header = column.Header.ToString();
                        var colNames = this.Transformation.ColumnNames;
                        var i = -1;
                        while (++i < colNames.Length && colNames[i] != header) ;

                        if (i < _columnWidths.Length)
                            _columnWidths[i] = column.ActualWidth;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new TransformationsWindow();
            window.ShowDialog();
            this.Transformation = window.SelectedTransformation;
        }
    }
}
