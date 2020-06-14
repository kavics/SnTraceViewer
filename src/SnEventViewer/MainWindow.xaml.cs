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

namespace SnEventViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DisplayEntry> _allEntries;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //var selectedItem = sender as ListViewItem;
            //var selectedEntry = (DisplayEntry)selectedItem.Content;

            //var opId = selectedEntry.OpId;
            //if (opId == 0)
            //    return;

            //var state = selectedEntry.Status;
            //if (state == "ERROR")
            //    return;

            //var appDomain = selectedEntry.AppDomain;
            //SelectByOperationAndAppDomain(selectedItem, opId, appDomain, state);
        }
        private void SelectByOperationAndAppDomain(ListViewItem selectedItem, int opId, string appDomain, string status)
        {
            //var boundaries = _currentlyVisible.Where(x => x.OpId == opId && x.AppDomain == appDomain).ToArray();
            //if (boundaries.Length < 2)
            //    return;

            //var bottom = listView.Items.IndexOf(boundaries[0]);
            //var top = listView.Items.IndexOf(boundaries[1]);

            //listView.SelectedItems.Clear();
            //for (int i = bottom; i <= top; i++)
            //    if (_currentlyVisible[i] != selectedItem.Content)
            //        listView.SelectedItems.Add(_currentlyVisible[i]);
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = sender as ListViewItem;
            var selectedEntry = (DisplayEntry)selectedItem.Content;

            listView.SelectedItem = selectedItem;
        }

        private void directoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog // new Microsoft.Win32.OpenFileDialog();
            {
                FileName = "---",
                //InitialDirectory = this.LastDirectory,
                Title = "Open directory",
                ValidateNames = false
            };
            var result = dialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            var fileName = dialog.FileName;
            var selectedDirectory = System.IO.Path.GetDirectoryName(dialog.FileName);
            directoryTextBox.Text = selectedDirectory;
            //this.LastDirectory = selectedDirectory;

            var rootPath = selectedDirectory; // TraceDirectory.GetFullPath(selectedDirectory);
            var eventDirs = EventLogDirectory.SearchEventLogDirectories(rootPath);

            ShowDirectory(eventDirs.Length == 0 
                ? new EventLogEntry[0] 
                : (IEnumerable<EventLogEntry>)eventDirs.First());
        }
        private void ShowDirectory(IEnumerable<EventLogEntry> session)
        {
            var entries = session.Select(x => new DisplayEntry(x)).ToList();
            _allEntries = entries;
            listView.ItemsSource = _allEntries;
        }

    }
}
