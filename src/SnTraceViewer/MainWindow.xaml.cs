using SenseNet.Diagnostics;
using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SnTraceViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<DisplayEntry> entries;
            var directory = directoryTextBox.Text;
            if (!System.IO.Path.IsPathRooted(directory))
                directory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory));
            var files = System.IO.Directory.GetFiles(directory);
            var fileNames = files
                //.OrderByDescending(x => x)
                .Select(x => System.IO.Path.GetFileName(x))
                .ToArray();

            fileNamesComboBox.ItemsSource = fileNames;
            fileNamesComboBox.SelectedIndex = 0;
            var selectedFile = fileNamesComboBox.SelectedItem;

            var file = System.IO.Path.Combine(directory, selectedFile.ToString());
            using (var reader = Reader.Create(file))
                entries = reader.Select(x => new DisplayEntry(x)).ToList();

            _allEntries = entries;
            _currentlyVisible = entries;
            listView.ItemsSource = entries;

            this.CategoryVisibility = new CategoryVisibility(this);

            this.DataContext = this;
        }

        public CategoryVisibility CategoryVisibility { get; }

        private List<DisplayEntry> _allEntries;
        private List<DisplayEntry> _currentlyVisible;
        public void ApplyCategoryFilters()
        {
            var filtered = (IEnumerable<DisplayEntry>)_allEntries;
            if (!CategoryVisibility.SystemVisible)
                filtered = filtered.Where(e => e.Category != "System");
            if (!CategoryVisibility.Web)
                filtered = filtered.Where(e => e.Category != "Web");
            if (!CategoryVisibility.Event)
                filtered = filtered.Where(e => e.Category != "Event");
            if (!CategoryVisibility.Repository)
                filtered = filtered.Where(e => e.Category != "Repository");
            if (!CategoryVisibility.ContentOperation)
                filtered = filtered.Where(e => e.Category != "ContentOperation");
            if (!CategoryVisibility.Query)
                filtered = filtered.Where(e => e.Category != "Query");
            if (!CategoryVisibility.Index)
                filtered = filtered.Where(e => e.Category != "Index");
            if (!CategoryVisibility.IndexQueue)
                filtered = filtered.Where(e => e.Category != "IndexQueue");
            if (!CategoryVisibility.Database)
                filtered = filtered.Where(e => e.Category != "Database");
            if (!CategoryVisibility.Messaging)
                filtered = filtered.Where(e => e.Category != "Messaging");
            if (!CategoryVisibility.Workflow)
                filtered = filtered.Where(e => e.Category != "Workflow");
            if (!CategoryVisibility.Security)
                filtered = filtered.Where(e => e.Category != "Security");
            if (!CategoryVisibility.SecurityQueue)
                filtered = filtered.Where(e => e.Category != "SecurityQueue");
            if (!CategoryVisibility.TaskManagement)
                filtered = filtered.Where(e => e.Category != "TaskManagement");
            if (!CategoryVisibility.Custom)
                filtered = filtered.Where(e => e.Category != "Custom");
            if (!CategoryVisibility.Test)
                filtered = filtered.Where(e => e.Category != "Test");
            if (!CategoryVisibility.Other)
                filtered = filtered.Where(e => e.Category != "Other");

            _currentlyVisible = filtered.ToList();
            listView.ItemsSource = _currentlyVisible;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = sender as ListViewItem;
            var selectedEntry = (DisplayEntry)selectedItem.Content;

            var opId = selectedEntry.OpId;
            if (opId == 0)
                return;

            var state = selectedEntry.Status;
            if (state == "ERROR")
                return;

            var appDomain = selectedEntry.AppDomain;
            SelectByOperationAndAppDomain(selectedItem, opId, appDomain, state);
        }
        private void SelectByOperationAndAppDomain(ListViewItem selectedItem, int opId, string appDomain, string status)
        {
            var boundaries = _currentlyVisible.Where(x => x.OpId == opId && x.AppDomain == appDomain).ToArray();
            if (boundaries.Length < 2)
                return;

            var bottom = listView.Items.IndexOf(boundaries[0]);
            var top = listView.Items.IndexOf(boundaries[1]);

            listView.SelectedItems.Clear();
            for (int i = bottom; i <= top; i++)
                listView.SelectedItems.Add(_currentlyVisible[i]);
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = sender as ListViewItem;
            var selectedEntry = (DisplayEntry)selectedItem.Content;

            listView.SelectedItem = selectedItem;
        }

        private class DisplayEntry
        {
            public string BlockStart { get; set; }
            public int LineId { get; set; }
            public string Time { get; set; }
            public string Category { get; set; }
            public string AppDomain { get; set; }
            public int ThreadId { get; set; }
            public int OpId { get; set; }
            public string Status { get; set; }
            public string StatusColor { get; set; }
            public string StatusWeight { get; set; }
            public string Duration { get; set; }
            public string Message { get; set; }

            public DisplayEntry(Entry x)
            {
                string status;
                string statusColor;
                string statusWeight;
                switch (x.Status)
                {
                    default:
                        status = string.Empty;
                        statusColor = "#FFFFFF";
                        statusWeight = "Normal";
                        break;
                    case "Start":
                        status = "Start";
                        statusColor = "#FFFFFF";
                        statusWeight = "Normal";
                        break;
                    case "End":
                        status = "End";
                        statusColor = "#FFFFBB";
                        statusWeight = "Normal";
                        break;
                    case "ERROR":
                        status = "ERROR";
                        statusColor = "#FFBB99";
                        statusWeight = "Bold";
                        break;
                    case "UNTERMINATED":
                        status = "unterminated";
                        statusColor = "#FFFFBB";
                        statusWeight = "Bold";
                        break;
                }

                BlockStart = x.BlockStart ? ">" : "";
                LineId = x.LineId;
                Time = x.Time.ToString("HH:mm:ss.ffff");
                Category = x.Category;
                AppDomain = x.AppDomain;
                ThreadId = x.ThreadId;
                OpId = x.OpId;
                Status = status;
                StatusColor = statusColor;
                StatusWeight = statusWeight;
                Duration = x.Status != "UNTERMINATED" && x.Status != "End" ? "" : x.Duration.ToString(@"hh\:mm\:ss\.ffffff");
                Message = x.Message;
            }
        }
    }
}