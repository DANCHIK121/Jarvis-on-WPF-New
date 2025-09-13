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


namespace WorkWithVoskModel
{
    class VoskModel : IVoskModel
    {
        // Vosk
        private string _modelPath;
        private static Model? model;
        private static VoskRecognizer? recognizer;

        // Get audio from microphone
        private static WaveInEvent? waveIn;

        // Diagnostic tools
        private static Stopwatch? silenceTimer;
        private static StringBuilder? currentText;

        // Play Jarvis responses
        private static Audio? audio;

        // Json classes
        private readonly IJson _json;
        private readonly IJson _jsonWithPathConsts;

        // Objects for deserialization
        private readonly PathsClass _pathsClass;
        private readonly DataClass _constsClass;

        public VoskModel()
        {
            // Path consts
            _jsonWithPathConsts = new JsonClass
            {
                FilePath = JsonClass.jsonFileWithPathConsts
            };

            // Deserialized class with path consts
            _pathsClass = new PathsClass(); // Path const class
            _pathsClass = _jsonWithPathConsts.ReadJson<PathsClass>(); // Reading data from json file

            // Programm consts
            _json = new JsonClass
            {
                FilePath = _pathsClass.PathConsts![0].DataJsonPathConst!
            };

            // Deserialized class with programm consts
            _constsClass = new DataClass(); // Programm const class
            _constsClass = _json.ReadJson<DataClass>(); // Reading data from json file

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
        }

        public void StartListening()
        {
            try
            {
                // Проверяем существование модели
                if (!Directory.Exists(_modelPath))
                {
                    if (_constsClass.DebugMode! == true)
                    {
                        MessageBox.Show($"Ошибка: Модель не найдена по пути: {modelPath}");
                    }
                    MessageBox.Show($"Ошибка: Модель не найдена по пути: {modelPath}");
                    Console.WriteLine("Убедитесь, что модель vosk-model-small-ru-0.22 скачана и распакована");
                    return;
                }

                silenceTimer = new Stopwatch();

                silenceTimer.Start();
                // Init Vosk model
                model = new Model(modelPath);
                recognizer = new VoskRecognizer(model, 16000.0f);
                silenceTimer.Stop();
                Console.WriteLine(silenceTimer.ElapsedMilliseconds / 1000);

                // Init wave capture
                waveIn = new WaveInEvent()
                {
                    WaveFormat = new WaveFormat(16000, 1),
                    BufferMilliseconds = 50,
                    NumberOfBuffers = 2
                };

                // Init audio playing
                // audio = new Audio.Audio();

                // Init silence timer
                //silenceTimer = new Stopwatch();

                // Console settings
                // Console.Clear();
                Console.Title = "Jarvis - Распознавание речи";
                Console.OutputEncoding = Encoding.UTF8;

                Console.WriteLine("=== Jarvis Speech Recognition ===");
                Console.WriteLine("Настройка микрофона...");

                // Получаем список доступных устройств
                int waveInDevices = WaveInEvent.DeviceCount;
                Console.WriteLine($"Доступных аудиоустройств: {waveInDevices}");

                for (int i = 0; i < waveInDevices; i++)
                {
                    var capabilities = WaveInEvent.GetCapabilities(i);
                    Console.WriteLine($"Устройство {i}: {capabilities.ProductName}");
                }

                // Write listened data to result string
                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += (s, e) => Console.WriteLine("Запись остановлена");

                // Start recording
                Console.WriteLine("\n🎤 Начинаю запись... Говорите!");
                waveIn.StartRecording();

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
                    if (silenceTimer.IsRunning && silenceTimer.ElapsedMilliseconds > 2000)
                    {
                        FinalizeRecognition();
                        silenceTimer.Restart();
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
                waveIn?.StopRecording();
                waveIn?.Dispose();
                recognizer?.Dispose();
                model?.Dispose();
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
                    if (!silenceTimer.IsRunning)
                        silenceTimer.Start();
                    return;
                }

                // Есть звук - сбрасываем таймер
                silenceTimer.Restart();

                // Передаем аудио в распознаватель
                if (recognizer.AcceptWaveform(processedAudio, processedAudio.Length))
                {
                    // Получаем финальный результат
                    string resultJson = recognizer.Result();
                    ProcessResult(resultJson, isFinal: true);
                }
                else
                {
                    // Получаем частичный результат
                    string partialJson = recognizer.PartialResult();
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
                string finalResult = recognizer.FinalResult();
                ProcessResult(finalResult, isFinal: true);

                // Reset recognizer
                recognizer.Reset();

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

    // JSON classes
    public class VoskFinalResult
    {
        public string text { get; set; }
    }

    public class VoskPartialResult
    {
        public string partial { get; set; }
    }
}