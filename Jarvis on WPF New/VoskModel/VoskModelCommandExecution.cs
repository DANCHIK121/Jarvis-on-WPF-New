// Standart usings
using System.Windows.Forms;

// Project usings
using Jarvis_on_WPF.JarvisAudioResponses;

namespace Jarvis_on_WPF_New.VoskModel
{
    internal class VoskModelCommandExecution
    {
        private readonly IAudio? _jarvisAudioResponses;

        public VoskModelCommandExecution()
        {
            _jarvisAudioResponses = new Audio(AudioModes.JarvisGoodbye);
        }

        public void Execute(string command)
        {
            command = command.ToLower();

            switch (command)
            {
                case "program_exit":
                    _jarvisAudioResponses!.Play();
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
