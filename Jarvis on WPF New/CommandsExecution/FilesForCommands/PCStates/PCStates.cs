// Standart usings
using System.Windows.Forms;

// Project usings
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;

namespace Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.PCStates
{
    public class PCStates : IPCStates
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _constsClass;

        public PCStates()
        {
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _constsClass = new ProgramConstsClass(); // Programm const class
            _constsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file
        }

        public void EnterSleepMode(VoskModelEventsForNews? voskModelNewsPublisher)
        {
            try
            {
                // force = false - soft program end
                // disableWakeEvent = true - disable wake up events
                Application.SetSuspendState(PowerState.Suspend, false, true);
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode! == true)
                    voskModelNewsPublisher!.PublishNews($"Ошибка: {ex.Message}");
            }
        }
    }
}
