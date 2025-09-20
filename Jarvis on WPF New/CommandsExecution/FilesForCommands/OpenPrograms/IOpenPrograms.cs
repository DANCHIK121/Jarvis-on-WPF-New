using Jarvis_on_WPF_New.VoskModel;

namespace Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.OpenPrograms
{
    interface IOpenPrograms
    {
        public void OpenBrowser(VoskModelEventsForNews? voskModelEventsForNews, bool openWithURL, string url = "");
    }
}
