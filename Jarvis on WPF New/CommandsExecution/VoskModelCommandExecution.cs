// Standart usings
using System.Diagnostics;

// Project usings
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;
using Jarvis_on_WPF_New.Perceptron;
using Jarvis_on_WPF_New.JarvisAudioResponses;
using Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.PCStates;
using Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.OpenPrograms;
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

        // Command execution classes
        // Windows Manager
        private readonly IMinimizeWindows? _minimizeWindows;

        // PCStates
        private readonly IPCStates? _pcStates;

        // OpenPrograms
        private readonly IOpenPrograms? _openPrograms;

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

            // Command execution classes
            // Windows Manager
            _minimizeWindows = new MinimizeWindows();

            // PCStates
            _pcStates = new PCStates();

            // OpenPrograms
            _openPrograms = new OpenPrograms();
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

                case CommandsEnum.OpenBrowser:
                    _openPrograms!.OpenBrowser(_voskModelNewsPublisher, false, "");
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSer;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.Weather:
                    _openPrograms!.OpenBrowser(_voskModelNewsPublisher, true, _programConstsClass!.DefaultWebsiteWithWeather!);
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSer;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.OpenVideoHostingWebSite:
                    _openPrograms!.OpenBrowser(_voskModelNewsPublisher, true, _programConstsClass!.DefaultVideoHosting!);
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSer;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.SearchWeb:
                    _openPrograms!.OpenBrowser(_voskModelNewsPublisher, true, "");
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.YesSer;
                    _jarvisAudioResponses!.Play();
                    break;

                case CommandsEnum.Sleep:
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.PowerOff;
                    _jarvisAudioResponses!.Play();
                    Thread.Sleep(3000);
                    _pcStates!.EnterSleepMode(_voskModelNewsPublisher);
                    break;

                case CommandsEnum.MinimizeWindows:
                    _minimizeWindows!.MinimizeAllWindows();
                    _jarvisAudioResponses!.ChangeAudioMode = AudioModes.Invisible;
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
