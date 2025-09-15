// Project usings
using Jarvis_on_WPF.Json;

namespace Jarvis_on_WPF_New.Perceptron
{
    public class ContextNeuralAssistant
    {
        // Neural classes
        private NeuralNetwork network;
        private TextProcessor textProcessor;

        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        public ContextNeuralAssistant()
        {
            // Initialize classes
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = new ProgramConstsClass(); // Programm const class
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file

            textProcessor = new TextProcessor();
            network = new NeuralNetwork(
                textProcessor.GetVocabularySize(),
                textProcessor.GetCommandCount()
            );

            TrainNetwork();
        }

        private void TrainNetwork()
        {
            // Training examples
            var trainingData = new[]
            {
                ("хочу смотреть видео", "open_browser"),
                ("открой браузер", "open_browser"),
                ("открой ютуб", "open_browser"),
                ("найди в интернете", "search_web"),
                ("поиск информации", "search_web"),
                ("какая погода", "weather"),
                ("включи музыку", "play_music")
            };

            // Train
            foreach (var (text, command) in trainingData)
            {
                var input = textProcessor.TextToVector(text);
                var target = textProcessor.GetTargetVector(command);

                for (int i = 0; i < _programConstsClass.EpochCount!; i++) // Epoch count
                {
                    network.Train(input, target, 0.1);
                }
            }
        }

        public string ProcessCommand(string userInput)
        {
            var inputVector = textProcessor.TextToVector(userInput);
            var output = network.Forward(inputVector);
            var command = textProcessor.GetCommandFromOutput(output);

            return ExecuteCommand(command, userInput);
        }

        private string ExecuteCommand(string command, string input)
        {
            return command switch
            {
                "open_browser" => input.Contains("youtube") ?
                    "Запускаю YouTube!" : "Открываю браузер!",
                "search_web" => "Ищу в интернете...",
                "weather" => "Погода: солнечно, +20°C",
                "play_music" => "Включаю музыку!",
                _ => "Не понял команду"
            };
        }
    }
}
