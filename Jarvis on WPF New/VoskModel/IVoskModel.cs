namespace Jarvis_on_WPF_New.VoskModel
{
    public interface IVoskModel
    {
        public ref VoskModelEventsForNews GetVoskModelEventsForNews { get; }
        public ref VoskModelEventsForTextChattingInThreads GetVoskModelEventsForTextChattingInThreads { get; }

        public void DownloadModel();
        public void StartListening();
    }
}