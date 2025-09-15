// Standart usings
using System.Windows;

// Project usings
using Jarvis_on_WPF.Json;

namespace Jarvis_on_WPF_New.VoskModel
{
    public class VoskModelEventsForNews
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        // Event handler
        public event EventHandler<string>? NewsPublished;

        public VoskModelEventsForNews()
        {
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = new ProgramConstsClass(); // Programm const class
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file
        }

        public void PublishNews(string news) => OnNewsPublished(news);
        protected virtual void OnNewsPublished(string news) => NewsPublished?.Invoke(this, news);
    }

    public class VoskModelEventsForTextChattingInThreads
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        // Event handler
        public event EventHandler<string>? TextPublished;

        public VoskModelEventsForTextChattingInThreads()
        {
            // Programm constss
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = new ProgramConstsClass(); // Programm const class
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file
        }

        public void PublishText(string news) => OnNewsPublished(news);
        protected virtual void OnNewsPublished(string news) => TextPublished?.Invoke(this, news);
    }
}
