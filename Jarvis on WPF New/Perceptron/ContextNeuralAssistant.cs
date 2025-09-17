// Project usings
using Jarvis_on_WPF_New.CommandsExecution;
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;

namespace Jarvis_on_WPF_New.Perceptron
{
    public class ContextNeuralAssistant
    {
        // ML and Vosk variables
        private MLCommandClassifier _mlClassifier;
        private VoskModelCommandExecution _voskModelCommandExecution;

        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        // Vosk model
        private VoskModel.IVoskModel _voskModel;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;

        public ContextNeuralAssistant()
        {
            _mlClassifier = new MLCommandClassifier();
            _voskModelCommandExecution = new VoskModelCommandExecution();

            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = new ProgramConstsClass(); // Programm const class
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file

            // Init vosk
            _voskModel = new VoskModel.VoskModelClass();

            // Init event
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;

            // Test model in class initialization
            _mlClassifier.TestModel();
        }

        public string ProcessCommand(string userInput)
        {
            var command = _mlClassifier.PredictCommand(userInput);

            // Command execution
            _voskModelCommandExecution.Execute(Commands.CommandsConvertDictionary[command]);

            return command;
        }

        // Method for add new examples for training
        public void AddTrainingExample(string text, string command)
        {
            if (_programConstsClass.DebugMode! == true)
            {
                _voskModelNewsPublisher!.PublishNews($"Добавлен пример: '{text}' -> '{command}'");
            }
        }
    }
}
