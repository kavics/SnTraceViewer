using SenseNet.Diagnostics.Analysis2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnTraceViewer
{
    public class DisplayEntry
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
                    statusWeight = "Bold";
                    break;
                case "End":
                    status = "End";
                    statusColor = "#FFFFFF";
                    statusWeight = "Bold";
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
