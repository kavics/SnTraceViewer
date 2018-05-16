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
using System.Windows.Interop;
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
        private FiltersWindow _filtersWindow;
        private OutputWindow _outputWindow;

        public string LastDirectory
        {
            get
            {
                var lastDirectory = Properties.Settings.Default.LastDirectory;
                if (string.IsNullOrEmpty(lastDirectory) || !System.IO.Directory.Exists(lastDirectory))
                {
                    lastDirectory = Environment.GetEnvironmentVariable("USERPROFILE");
                    LastDirectory = lastDirectory;
                }
                return lastDirectory;
            }
            set
            {
                Properties.Settings.Default.LastDirectory = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _isSessionChangeEnabled = false;

            directoryTextBox.Text = "";

            this.CategoryVisibility = new CategoryVisibility(this);
            _filtersWindow = new FiltersWindow(this.CategoryVisibility);
            _outputWindow = new OutputWindow();

            this.DataContext = this;

            _isSessionChangeEnabled = true;
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
            //if (!CategoryVisibility.Other)
            //    filtered = filtered.Where(e => e.Category != "Other");

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
                if (_currentlyVisible[i] != selectedItem.Content)
                    listView.SelectedItems.Add(_currentlyVisible[i]);
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = sender as ListViewItem;
            var selectedEntry = (DisplayEntry)selectedItem.Content;

            listView.SelectedItem = selectedItem;
        }

        private void filterButton_Click(object sender, RoutedEventArgs e)
        {
            _filtersWindow.Show();
        }
        private void transformButton_Click(object sender, RoutedEventArgs e)
        {
            _outputWindow.Show();
        }
        private void directoryButton_Click(object sender, RoutedEventArgs e)
        {
            _isSessionChangeEnabled = false;

            var dialog = new Microsoft.Win32.SaveFileDialog // new Microsoft.Win32.OpenFileDialog();
            {
                FileName = "---",
                InitialDirectory = this.LastDirectory,
                Title = "Open directory",
                ValidateNames = false
            };
            var result = dialog.ShowDialog();

            if (!result.HasValue || !result.Value)
                return;

            var fileName = dialog.FileName;
            var selectedDirectory = System.IO.Path.GetDirectoryName(dialog.FileName);
            directoryTextBox.Text = selectedDirectory;
            this.LastDirectory = selectedDirectory;

            var rootPath = selectedDirectory; // TraceDirectory.GetFullPath(selectedDirectory);
            var traceDirs = TraceDirectory.SearchTraceDirectories(rootPath);
            var sessions = TraceSession.Create(traceDirs);

            fileNamesComboBox.ItemsSource = sessions;
            fileNamesComboBox.SelectedIndex = sessions.Length - 1;

            RefreshSessionNavigatorPanel();

            var selectedFile = fileNamesComboBox.SelectedItem;

            ChangeSession(sessions.Length == 0 ? new Entry[0] : (IEnumerable<Entry>)sessions.Last());
            _isSessionChangeEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _filtersWindow.Exit();
            _filtersWindow.Close();
            _outputWindow.Exit();
            _outputWindow.Close();
        }

        private bool _isSessionChangeEnabled;
        private void fileNamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isSessionChangeEnabled)
                return;

            RefreshSessionNavigatorPanel();

            var selectedSession = (TraceSession)fileNamesComboBox.SelectedItem;
            ChangeSession(selectedSession);
        }

        private void ChangeSession(IEnumerable<Entry> session)
        {
            var entries = session.Select(x => new DisplayEntry(x)).ToList();
            _allEntries = entries;
            _currentlyVisible = entries;
            ApplyCategoryFilters();
            _outputWindow.SetEntries(session);
        }

        private void firstSessionButton_Click(object sender, RoutedEventArgs e)
        {
            fileNamesComboBox.SelectedIndex = 0;
        }
        private void previousSessionButton_Click(object sender, RoutedEventArgs e)
        {
            fileNamesComboBox.SelectedIndex--;
        }
        private void nextSessionButton_Click(object sender, RoutedEventArgs e)
        {
            fileNamesComboBox.SelectedIndex++;
        }
        private void lastSessionButton_Click(object sender, RoutedEventArgs e)
        {
            fileNamesComboBox.SelectedIndex = fileNamesComboBox.Items.Count - 1;
        }
        private void RefreshSessionNavigatorPanel()
        {
            var index = fileNamesComboBox.SelectedIndex;
            var count = fileNamesComboBox.Items.Count;

            sessionsTextBlock.Text = $"{index + 1} / {count}";

            firstSessionButton.IsEnabled = previousSessionButton.IsEnabled = (index > 0);
            nextSessionButton.IsEnabled = lastSessionButton.IsEnabled = (index < count - 1);
        }
    }
}