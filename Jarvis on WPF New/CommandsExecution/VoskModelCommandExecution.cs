// Standart usings
using System.Diagnostics;

// Project usings
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;
using Jarvis_on_WPF_New.Perceptron;
using Jarvis_on_WPF_New.JarvisAudioResponses;
using Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.MinimizeWindows;

namespace Jarvis_on_WPF_New.CommandsExecution
{
    internal class VoskModelCommandExecution
    {
        // Vosk model
        private IVoskModel? _voskModel;

        // Json audio responses
        private readonly IAudio? _jarvisAudioResponses;

        // Json classes
        private readonly IJson? _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass? _programConstsClass;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;
        private VoskModelEventsForTextChattingInThreads? _voskModelEventsForTextChattingInThreads;

        // Windows Manager
        private readonly IMinimizeWindows? _minimizeWindows;

        public VoskModelCommandExecution()
        {
            // Jarvis audio responses
            _jarvisAudioResponses = new Audio(AudioModes.Default);

            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>();

            // Init vosk model
            _voskModel = new VoskModelClass();

            // Init VoskModelEventsForNews
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;
            _voskModelEventsForTextChattingInThreads = _voskModel.GetVoskModelEventsForTextChattingInThreads;

            // Windows Manager
            _minimizeWindows = new MinimizeWindows();
        }

        public void Execute(CommandsEnum command)
        {
            switch (command)
            {
                case CommandsEnum.PlayMusic:
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = "wmplayer.exe",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Normal
                        };

                        Process.Start(startInfo);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        _voskModelEventsForTextChattingInThreads!.PublishText("Windows Media Player не найден. Просьба выбрать приложение по умолчанию.");
                    }
                    catch (Exception ex)
                    {
                        if (_programConstsClass!.DebugMode! == true)
                            _voskModelNewsPublisher!.PublishNews($"Ошибка: {ex.Message}");
                    }
                    
                    break;

                case CommandsEnum.MinimizeWindows:
                    _minimizeWindows!.MinimizeAllWindows();
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSerSecond;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.CloseAllWindows:
                    _minimizeWindows!.CloseAllWindows();
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSer;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.ProgramExit:
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.JarvisGoodbye;
                    _jarvisAudioResponses!.Play();
                    Thread.Sleep(3000);
                    Environment.Exit(0);

                    break;
            }
        }
    }
}
