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

            var samplesDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\SampleFiles"));
            var sampleFile = System.IO.Path.Combine(samplesDirectory, "detailedlog_20171015-062009Z.txt");
            using (var reader = Reader.Create(sampleFile))
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
            public string Duration { get; set; }
            public string Message { get; set; }

            public DisplayEntry(Entry x)
            {
                BlockStart = x.BlockStart ? ">" : "";
                LineId = x.LineId;
                Time = x.Time.ToString("HH:mm:ss.ffff");
                Category = x.Category;
                AppDomain = x.AppDomain;
                ThreadId = x.ThreadId;
                OpId = x.OpId;
                Status = x.Status;
                Duration = x.Status != "UNTERMINATED" && x.Status != "End" ? "" : x.Duration.ToString(@"hh\:mm\:ss\.ffffff");
                Message = x.Message;
            }
        }
    }
}