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
    class VoskModel : IVoskModel
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
            // Init events handler
            _voskModelNewsPublisher = new VoskModelEventsForNews();

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
        }

        public ref VoskModelEventsForNews GetVoskModelEventsForNews
        {
            get { return ref _voskModelNewsPublisher!; }
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
                // Count wave in devices
                int waveInDevices = WaveInEvent.DeviceCount;

                _voskModelEventsForTextChattingInThreads!.PublishText("=== Jarvis Speech Recognition ===");

                if (_constsClass.DebugMode! == true)
                {
                    _voskModelNewsPublisher!.PublishNews("Настройка микрофона...");

                    // Получаем список доступных устройств
                    Console.WriteLine($"Доступных аудиоустройств: {waveInDevices}");

                    for (int i = 0; i < waveInDevices; i++)
                    {
                        var capabilities = WaveInEvent.GetCapabilities(i);
                        Console.WriteLine($"Устройство {i}: {capabilities.ProductName}");
                    }
                }

                // Write listened data to result string
                _waveIn.DataAvailable += WaveIn_DataAvailable;
                _waveIn.RecordingStopped += (s, e) => Console.WriteLine("Запись остановлена");

                // Start recording
                Console.WriteLine("\n🎤 Начинаю запись... Говорите!");
                _waveIn.StartRecording();

                Console.WriteLine("\n⚡ Режимы работы:");
                Console.WriteLine("• Говорите четко в микрофон");
                Console.WriteLine("• Пауза 2 секунды - финализация фразы");
                Console.WriteLine("• Press 'Q' to quit\n");

                // Main loop
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Q)
                            break;
                    }

                    // Проверяем таймер тишины в основном цикле
                    if (_silenceTimer.IsRunning && _silenceTimer.ElapsedMilliseconds > 2000)
                    {
                        FinalizeRecognition();
                        _silenceTimer.Restart();
                    }

                    Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Критическая ошибка: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            finally
            {
                // Cleanup
                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _recognizer?.Dispose();
                _model?.Dispose();
                Console.WriteLine("Ресурсы освобождены. Выход.");
            }
        }

        private static void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                byte[] processedAudio = EnhanceAudioQuality(e.Buffer, e.BytesRecorded);

                if (!IsAudioLoudEnough(processedAudio))
                {
                    // Тишина - запускаем таймер
                    if (!_silenceTimer.IsRunning)
                        _silenceTimer.Start();
                    return;
                }

                // Есть звук - сбрасываем таймер
                _silenceTimer.Restart();

                // Передаем аудио в распознаватель
                if (_recognizer!.AcceptWaveform(processedAudio, processedAudio.Length))
                {
                    // Получаем финальный результат
                    string resultJson = _recognizer.Result();
                    ProcessResult(resultJson, isFinal: true);
                }
                else
                {
                    // Получаем частичный результат
                    string partialJson = _recognizer.PartialResult();
                    ProcessResult(partialJson, isFinal: false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки аудио: {ex.Message}");
            }
        }

        private static void ProcessResult(string json, bool isFinal)
        {
            if (string.IsNullOrEmpty(json) || json == "{\"partial\" : \"\"}")
                return;

            try
            {
                if (isFinal)
                {
                    var result = JsonConvert.DeserializeObject<VoskFinalResult>(json);
                    if (!string.IsNullOrEmpty(result?.text))
                    {
                        Console.WriteLine($"\n🎯 ФИНАЛЬНО: {result.text}");
                        currentText.Clear();
                    }
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<VoskPartialResult>(json);
                    if (!string.IsNullOrEmpty(result?.partial))
                    {
                        Console.Write($"\r🔍 Распознаю: {result.partial}          ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка парсинга JSON: {ex.Message}");
            }
        }

        private static void FinalizeRecognition()
        {
            try
            {
                // Принудительно получаем финальный результат
                string finalResult = _recognizer.FinalResult();
                ProcessResult(finalResult, isFinal: true);

                // Reset recognizer
                _recognizer.Reset();

                Console.WriteLine("\n--- Готов к новой фразе ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при финализации: {ex.Message}");
            }
        }

        private static byte[] EnhanceAudioQuality(byte[] buffer, int length)
        {
            if (length == 0) return buffer;

            short[] samples = new short[length / 2];
            Buffer.BlockCopy(buffer, 0, samples, 0, length);

            // Шумоподавление
            for (int i = 0; i < samples.Length; i++)
            {
                if (Math.Abs(samples[i]) < 300) // Более агрессивное шумоподавление
                    samples[i] = 0;
            }

            NormalizeAudio(samples);

            byte[] processed = new byte[length];
            Buffer.BlockCopy(samples, 0, processed, 0, length);
            return processed;
        }

        private static void NormalizeAudio(short[] samples)
        {
            short maxAmplitude = 0;
            foreach (var sample in samples)
            {
                if (Math.Abs(sample) > maxAmplitude)
                    maxAmplitude = Math.Abs(sample);
            }

            if (maxAmplitude > 1000 && maxAmplitude < 10000)
            {
                float gain = 8000f / maxAmplitude;
                for (int i = 0; i < samples.Length; i++)
                {
                    int amplified = (int)(samples[i] * gain);
                    samples[i] = (short)Math.Clamp(amplified, short.MinValue, short.MaxValue);
                }
            }
        }

        private static bool IsAudioLoudEnough(byte[] audio)
        {
            if (audio.Length == 0) return false;

            short[] samples = new short[audio.Length / 2];
            Buffer.BlockCopy(audio, 0, samples, 0, audio.Length);

            double sum = 0;
            int count = 0;
            foreach (var sample in samples)
            {
                if (Math.Abs(sample) > 100) // Игнорируем совсем тихие samples
                {
                    sum += sample * sample;
                    count++;
                }
            }

            if (count == 0) return false;

            double rms = Math.Sqrt(sum / count);
            return rms > 200; // Пониженный порог для лучшей чувствительности
        }
    }
}