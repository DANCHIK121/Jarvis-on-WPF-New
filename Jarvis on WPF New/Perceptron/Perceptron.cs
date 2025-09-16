// Project usings
using Jarvis_on_WPF.Json;
using Jarvis_on_WPF_New.VoskModel;
using System.Windows;

namespace Jarvis_on_WPF_New.Perceptron
{ 
    class Perceptron : IPerceptron
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;

        // Vosk model
        private VoskModel.IVoskModel _voskModel;

        public Perceptron()
        {
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = new ProgramConstsClass(); // Programm const class
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file

            // Init vosk
            _voskModel = new VoskModel.VoskModel();

            // Init event
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;
        }

        public void ContextAnalyze(string input)
        {
            ContextNeuralAssistant assistant = new ContextNeuralAssistant();

            string command = assistant.ProcessCommand(input);

            MessageBox.Show(command);

            if (_programConstsClass.DebugMode! == true)
                _voskModelNewsPublisher!.PublishNews($"Выполнена команда: {command}");
        }
    }
}