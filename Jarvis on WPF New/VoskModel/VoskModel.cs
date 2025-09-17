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
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.Perceptron;
using Jarvis_on_WPF_New.CommandsExecution;

namespace Jarvis_on_WPF_New.VoskModel
{
    partial class VoskModelClass : IVoskModel
    {
        // Vosk
        private string _modelPath;
        private Model? _model;
        private VoskRecognizer? _recognizer;

        // Get audio from microphone
        private WaveInEvent _waveIn;

        // Diagnostic tools
        private Stopwatch _silenceTimer;
        private Stopwatch _voskModelDownloadTimer;

        private StringBuilder currentText;

        // Json classes
        private readonly IJson _json;
        private readonly IJson _jsonWithPathConsts;

        // Objects for deserialization
        private readonly PathsClass _pathsClass;
        private readonly ProgramConstsClass _constsClass;

        // Event publisher
        private VoskModelEventsForNews _voskModelNewsPublisher;
        private VoskModelEventsForTextChattingInThreads _voskModelEventsForTextChattingInThreads;

        // Neural context network
        private IPerceptron? _perceptron;

        public VoskModelClass()
        {
            try
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
                _pathsClass = _jsonWithPathConsts.ReadJson<PathsClass>();

                // Deserialized class with programm consts
                _constsClass = _json.ReadJson<ProgramConstsClass>();

                // Vosk model path
                _modelPath = _constsClass!.AccurateRecognitionMode! == true
                    ? _pathsClass!.AccurateModelPathConst!
                    : _pathsClass!.NotAccurateModelPathConst!;

                // Diagnostic
                _silenceTimer = new Stopwatch();
                _voskModelDownloadTimer = new Stopwatch();

                // Init wave capture (но не запускаем сразу)
                _waveIn = new WaveInEvent()
                {
                    WaveFormat = new WaveFormat((int)_constsClass.SampleRate!, 1),
                    BufferMilliseconds = (int)_constsClass.BufferMilliseconds!,
                    NumberOfBuffers = (int)_constsClass.NumberOfBuffers!,
                };

                // Neural context network
                _perceptron = null;

                // Write listened data to result string
                _waveIn.DataAvailable += WaveIn_DataAvailable!;

                if (_constsClass.DebugMode == true)
                {
                    _waveIn.RecordingStopped += (s, e) => _voskModelNewsPublisher.PublishNews("Запись остановлена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации VoskModel: {ex.Message}");
                throw;
            }
        }

        public void InitializePerceptron()
        {
            // Инициализируем перцептрон только когда нужно
            if (_perceptron == null)
            {
                _perceptron = new Perceptron.Perceptron();
            }
        }

        ~VoskModelClass()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            try
            {
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _recognizer?.Dispose();
                _model?.Dispose();

                if (_constsClass?.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews("Ресурсы освобождены. Выход.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при очистке: {ex.Message}");
            }
        }

        public ref VoskModelEventsForNews GetVoskModelEventsForNews
        {
            get { return ref _voskModelNewsPublisher; }
        }

        public ref VoskModelEventsForTextChattingInThreads GetVoskModelEventsForTextChattingInThreads
        {
            get { return ref _voskModelEventsForTextChattingInThreads; }
        }

        public void DownloadModel()
        {
            try
            {
                // Check vock model exists in path
                if (!Directory.Exists(_modelPath))
                {
                    if (_constsClass.DebugMode == true)
                    {
                        var temp = (bool)_constsClass.AccurateRecognitionMode!
                            ? _constsClass.AccurateRecognitionModelName
                            : _constsClass.NotAccurateRecognitionModelName;

                        var resultString = ($"Ошибка: Модель не найдена по пути: {_modelPath}\n" +
                                            $"Убедитесь, что модель {temp} скачана и распакована\n" +
                                            $"Скачайте её с сайта {_constsClass.URLForDownloadVoskModel}");

                        _voskModelNewsPublisher.PublishNews(resultString);
                        MessageBox.Show(resultString);
                    }
                    return;
                }

                // Model not downloaded
                _voskModelNewsPublisher.PublishNews("Начата загрузка модели.");
                _voskModelDownloadTimer.Start();

                // Init Vosk model
                _model = new Model(_modelPath);
                _recognizer = new VoskRecognizer(_model, 16000.0f);

                // Model downloaded
                _voskModelDownloadTimer.Stop();
                _voskModelNewsPublisher.PublishNews("Модель загружена.");

                // Calculate elapsed time
                if (_constsClass.DebugMode == true)
                {
                    _voskModelNewsPublisher.PublishNews($"Модель загрузилась за {_voskModelDownloadTimer.ElapsedMilliseconds / 1000} секунд.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки модели: {ex.Message}");
            }
        }

        public void StartListening()
        {
            try
            {
                if (_model == null || _recognizer == null)
                {
                    _voskModelEventsForTextChattingInThreads.PublishText("❌ Модель не загружена!");
                    return;
                }

                // Инициализируем перцептрон при начале прослушивания
                InitializePerceptron();

                // Count wave in devices
                int waveInDevices = WaveInEvent.DeviceCount;

                _voskModelEventsForTextChattingInThreads.PublishText("=== Jarvis Speech Recognition ===");

                if (_constsClass.DebugMode == true)
                {
                    _voskModelNewsPublisher.PublishNews($"Настройка микрофона...\nДоступных аудиоустройств: {waveInDevices}");

                    for (int i = 0; i < waveInDevices; i++)
                    {
                        var capabilities = WaveInEvent.GetCapabilities(i);
                        _voskModelNewsPublisher.PublishNews($"Устройство {i}: {capabilities.ProductName}");
                    }

                    if (waveInDevices == 0)
                    {
                        _voskModelEventsForTextChattingInThreads.PublishText("❌ Микрофон не найден!");
                        return;
                    }
                }

                // Start recording
                _voskModelEventsForTextChattingInThreads.PublishText("🎤 Начинаю запись... Говорите!");
                _waveIn.StartRecording();

                _voskModelEventsForTextChattingInThreads.PublishText("\n⚡ Режимы работы:");
                _voskModelEventsForTextChattingInThreads.PublishText("• Говорите четко в микрофон");
                _voskModelEventsForTextChattingInThreads.PublishText("• Пауза 2 секунды - финализация фразы");

                // Main loop
                while (true)
                {
                    // Check silence timer in main loop
                    if (_silenceTimer.IsRunning && _silenceTimer.ElapsedMilliseconds > _constsClass.SilenceTimerTimeoutInMilliSeconds)
                    {
                        FinalizeRecognition();
                        _silenceTimer.Restart();
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                // Print critical error
                _voskModelEventsForTextChattingInThreads.PublishText($"❌ Критическая ошибка!");
                if (_constsClass.DebugMode == true)
                    _voskModelNewsPublisher.PublishNews($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                Cleanup();
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
                    if (!_silenceTimer.IsRunning)
                        _silenceTimer.Start();
                    return;
                }

                // Sound - reset timer
                _silenceTimer.Restart();

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
                if (_constsClass.DebugMode == true)
                    _voskModelNewsPublisher.PublishNews($"Ошибка обработки аудио: {ex.Message}");
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
                        _voskModelEventsForTextChattingInThreads.PublishText($"\n🎯 ФИНАЛЬНО: {result.text}");

                        // Используем перцептрон для анализа
                        if (_perceptron != null)
                        {
                            _perceptron.ContextAnalyze(result.text);
                        }

                        currentText.Clear();
                        _recognizer!.Reset();
                    }
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<VoskPartialResult>(json);
                    if (!string.IsNullOrEmpty(result?.partial))
                    {
                        // Send partial text
                        _voskModelEventsForTextChattingInThreads.PublishText($"\r🔍 Распознаю: {result.partial}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode == true)
                    _voskModelNewsPublisher.PublishNews($"Ошибка парсинга JSON: {ex.Message}");
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

                _voskModelEventsForTextChattingInThreads.PublishText("\n--- Готов к новой фразе ---");
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode == true)
                    _voskModelNewsPublisher.PublishNews($"Ошибка парсинга JSON: {ex.Message}");
            }
        }
    }
}