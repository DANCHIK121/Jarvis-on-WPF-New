// Standart usings
using System.IO;
using System.Text;
using System.Windows;
using System.Diagnostics;

// Installed usings
using Vosk;
using NAudio.Wave;
using Newtonsoft.Json;

// Project usings
using Jarvis_on_WPF.Json;
using Jarvis_on_WPF.JarvisAudioResponses;


namespace Jarvis_on_WPF_New.VoskModel
{
    partial class VoskModel : IVoskModel
    {
        // Vosk
        private string _modelPath;
        private static Model? _model;
        private static VoskRecognizer? _recognizer;

        // Get audio from microphone
        private static WaveInEvent? _waveIn;

        // Diagnostic tools
        private static Stopwatch? _silenceTimer;
        private static Stopwatch? _voskModelDownloadTimer;

        private static StringBuilder? currentText;

        // Play Jarvis responses
        private static Audio? _audioJarvisResponses;

        // Json classes
        private readonly IJson _json;
        private readonly IJson _jsonWithPathConsts;

        // Objects for deserialization
        private readonly PathsClass _pathsClass;
        private readonly ProgramConstsClass _constsClass;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;
        private VoskModelEventsForTextChattingInThreads? _voskModelEventsForTextChattingInThreads;

        public VoskModel()
        {
            currentText = new StringBuilder();

            // Init events handler
            _voskModelNewsPublisher = new VoskModelEventsForNews();
            _voskModelEventsForTextChattingInThreads = new VoskModelEventsForTextChattingInThreads();

            // Path consts
            _jsonWithPathConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithPathConsts,
            };

            // Programm consts
            _json = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with path consts
            _pathsClass = new PathsClass(); // Path const class
            _pathsClass = _jsonWithPathConsts.ReadJson<PathsClass>(); // Reading data from json file

            // Deserialized class with programm consts
            _constsClass = new ProgramConstsClass(); // Programm const class
            _constsClass = _json.ReadJson<ProgramConstsClass>(); // Reading data from json file

            // Vosk model
            _modelPath = string.Empty;

            // Select vosk model path
            switch (_constsClass.AccurateRecognitionMode!)
            {
                case true:
                    _modelPath = _pathsClass.PathConsts![0].AccurateModelPathConst!;
                    break;
                case false:
                    _modelPath = _pathsClass.PathConsts![0].NotAccurateModelPathConst!;
                    break;
            }

            // Diagnostic
            _silenceTimer = new Stopwatch();
            _voskModelDownloadTimer = new Stopwatch();

            // Init Jarvis responses 
            _audioJarvisResponses = new Audio();

            // Init wave capture
            _waveIn = new WaveInEvent()
            {
                WaveFormat = new WaveFormat((int)_constsClass.SampleRate!, 1),
                BufferMilliseconds = (int)_constsClass.BufferMilliseconds!,
                NumberOfBuffers = (int)_constsClass.NumberOfBuffers!,
            };

            // Write listened data to result string
            _waveIn.DataAvailable += WaveIn_DataAvailable!;
            if (_constsClass!.DebugMode == true)
            {
                _waveIn.RecordingStopped += (s, e) => _voskModelNewsPublisher.PublishNews("Запись остановлена");
            }
        }

        ~VoskModel()
        {
            // Cleanup
            _waveIn?.StopRecording();
            _waveIn?.Dispose();
            _recognizer?.Dispose();
            _model?.Dispose();
            if (_constsClass.DebugMode! == true)
                _voskModelNewsPublisher!.PublishNews("Ресурсы освобождены. Выход.");
        }

        public ref VoskModelEventsForNews GetVoskModelEventsForNews
        {
            get { return ref _voskModelNewsPublisher!; }
        }

        public ref VoskModelEventsForTextChattingInThreads GetVoskModelEventsForTextChattingInThreads
        {
            get { return ref _voskModelEventsForTextChattingInThreads!; }
        }

        public void DownloadModel()
        {
            // Check vock model exists in path
            if (!Directory.Exists(_modelPath))
            {
                if (_constsClass.DebugMode! == true)
                {
                    var temp = string.Empty;

                    if (_constsClass.AccurateRecognitionMode! == true)
                        temp = _constsClass.AccurateRecognitionModelName!;
                    if (_constsClass.AccurateRecognitionMode! == false)
                        temp = _constsClass.NotAccurateRecognitionModelName!;

                    var resultString = ($"Ошибка: Модель не найдена по пути: {_modelPath}\n" +
                                        $"Убедитесь, что модель {temp} скачана и распакована\n" +
                                        $"Скачайте её с сайта {_constsClass.URLForDownloadVoskModel!}");

                    _voskModelNewsPublisher!.PublishNews(resultString);
                    MessageBox.Show(resultString);
                }
                return;
            }

            // Model not downloaded
            _voskModelNewsPublisher!.PublishNews("Начата загрузка модели.");
            _voskModelDownloadTimer?.Start();

            // Init Vosk model
            _model = new Model(_modelPath);
            _recognizer = new VoskRecognizer(_model, 16000.0f);

            // Model downloaded
            _voskModelDownloadTimer?.Stop();
            _voskModelNewsPublisher!.PublishNews("Модель загружена.");

            // Calculate elapsed time
            if (_constsClass.DebugMode! == true)
            {
                _voskModelNewsPublisher!.PublishNews($"Модель загрузилась за {_voskModelDownloadTimer?.ElapsedMilliseconds / 1000} секунд.");
            }
        }

        public void StartListening()
        {
            try
            {
                if (_model == null || _recognizer == null)
                {
                    _voskModelEventsForTextChattingInThreads!.PublishText("❌ Модель не загружена!");
                    return;
                }

                // Count wave in devices
                int waveInDevices = WaveInEvent.DeviceCount;

                _voskModelEventsForTextChattingInThreads!.PublishText("=== Jarvis Speech Recognition ===");

                if (_constsClass.DebugMode! == true)
                {
                    _voskModelNewsPublisher!.PublishNews($"Настройка микрофона...\nДоступных аудиоустройств: {waveInDevices}");

                    for (int i = 0; i < waveInDevices; i++)
                    {
                        var capabilities = WaveInEvent.GetCapabilities(i);
                        _voskModelNewsPublisher!.PublishNews($"Устройство {i}: {capabilities.ProductName}");
                    }

                    if (waveInDevices == 0)
                    {
                        _voskModelEventsForTextChattingInThreads!.PublishText("❌ Микрофон не найден!");
                        return;
                    }
                }

                // Start recording
                _voskModelEventsForTextChattingInThreads.PublishText("🎤 Начинаю запись... Говорите!");
                _waveIn!.StartRecording();

                _voskModelEventsForTextChattingInThreads.PublishText("\n⚡ Режимы работы:");
                _voskModelEventsForTextChattingInThreads.PublishText("• Говорите четко в микрофон");
                _voskModelEventsForTextChattingInThreads.PublishText("• Пауза 2 секунды - финализация фразы");

                // Main loop
                while (true)
                {
                    // Check silence timer in main loop
                    if (_silenceTimer!.IsRunning && _silenceTimer!.ElapsedMilliseconds > _constsClass.SilenceTimerTimeoutInMilliSeconds!)
                    {
                        FinalizeRecognition();
                        _silenceTimer!.Restart();
                    }

                    Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                // Print critical error
                _voskModelEventsForTextChattingInThreads!.PublishText($"❌ Критическая ошибка!");
                if (_constsClass.DebugMode! == true)
                    _voskModelNewsPublisher!.PublishNews($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                // Cleanup
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _recognizer?.Dispose();
                _model?.Dispose();
                if (_constsClass.DebugMode! == true)
                    _voskModelNewsPublisher!.PublishNews("Ресурсы освобождены. Выход.");
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                byte[] processedAudio = EnhanceAudioQuality(e.Buffer, e.BytesRecorded);

                if (!IsAudioLoudEnough(processedAudio))
                {
                    // Silence - start timer
                    if (!_silenceTimer!.IsRunning)
                        _silenceTimer.Start();
                    return;
                }

                // Sound - reset timer
                _silenceTimer!.Restart();

                // Send audio to recognizer
                if (_recognizer!.AcceptWaveform(processedAudio, processedAudio.Length))
                {
                    // Get final result
                    string resultJson = _recognizer.Result();
                    ProcessResult(resultJson, isFinal: true);
                }
                else
                {
                    // Get partial result
                    string partialJson = _recognizer.PartialResult();
                    ProcessResult(partialJson, isFinal: false);
                }
            }
            catch (Exception ex)
            {
                if (_constsClass!.DebugMode == true) _voskModelNewsPublisher!.PublishNews($"Ошибка обработки аудио: {ex.Message}");
                MessageBox.Show($"Ошибка обработки аудио: {ex.Message}");
            }
        }

        private void ProcessResult(string json, bool isFinal)
        {
            // Check json data
            if (string.IsNullOrEmpty(json) || json == "{\"partial\" : \"\"}")
                return;

            try
            {
                if (isFinal)
                {
                    var result = JsonConvert.DeserializeObject<VoskFinalResult>(json);
                    if (!string.IsNullOrEmpty(result?.text))
                    {
                        // Send final text
                        _voskModelEventsForTextChattingInThreads!.PublishText($"\n🎯 ФИНАЛЬНО: {result.text}");
                        VoskModelCommandExecution.Execute(result.text);
                        currentText!.Clear();
                    }
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<VoskPartialResult>(json);
                    if (!string.IsNullOrEmpty(result?.partial))
                    {
                        // Send partial text
                        _voskModelEventsForTextChattingInThreads!.PublishText($"\r🔍 Распознаю: {result.partial}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode! == true)
                    _voskModelNewsPublisher!.PublishNews($"Ошибка парсинга JSON: {ex.Message}");
                MessageBox.Show($"Ошибка парсинга JSON: {ex.Message}");
            }
        }

        private void FinalizeRecognition()
        {
            try
            {
                // Forced get final result
                string finalResult = _recognizer!.FinalResult();
                ProcessResult(finalResult, isFinal: true);

                // Reset recognizer
                _recognizer.Reset();

                _voskModelEventsForTextChattingInThreads!.PublishText("\n--- Готов к новой фразе ---");
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode! == true)
                    _voskModelNewsPublisher!.PublishNews($"Ошибка парсинга JSON: {ex.Message}");
            }
        }
    }
}