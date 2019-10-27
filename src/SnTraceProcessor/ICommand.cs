using System.IO;

namespace SnTraceProcessor
{
    public interface ICommand
    {
        string ShortInfo { get; }
        TextReader In { get; set; }
        TextWriter Out { get; set; }
        CommandContext Context { get; set; }

        void Execute();
    }
}
