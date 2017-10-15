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
            var fileNames = files.Select(x => System.IO.Path.GetFileName(x)).ToArray();

            fileNamesComboBox.ItemsSource = fileNames;
            fileNamesComboBox.SelectedIndex = 0;
            var selectedFile = fileNamesComboBox.SelectedItem;

            var file = System.IO.Path.Combine(directory, selectedFile.ToString());
            using (var reader = Reader.Create(file))
                entries = reader.Select(x => new DisplayEntry(x)).ToList();

            listView.ItemsSource = entries;
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