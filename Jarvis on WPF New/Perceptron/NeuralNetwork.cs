// Installed usings
using Microsoft.ML;

// Project usings
using Jarvis_on_WPF.Json;
using Jarvis_on_WPF_New.VoskModel;
using System.Windows;

namespace Jarvis_on_WPF_New.Perceptron
{
    public class MLCommandClassifier
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _programConstsClass;

        // Vosk model
        private VoskModel.IVoskModel _voskModel;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;

        // ML variables
        private MLContext? _mlContext;
        private ITransformer? _model;
        private PredictionEngine<CommandData, CommandPrediction>? _predictionEngine;
        private IDataView? _trainingDataView;

        // Create data for training
        private List<CommandData> _trainingData = new List<CommandData>
        {
            // Open Browser
            new CommandData { Text = "хочу смотреть видео", Label = "open_browser" },
            new CommandData { Text = "открой браузер", Label = "open_browser" },
            new CommandData { Text = "открой ютуб", Label = "open_browser" },
            new CommandData { Text = "видео", Label = "open_browser" },
            new CommandData { Text = "хочу видео", Label = "open_browser" },
            new CommandData { Text = "смотреть видео", Label = "open_browser" },

            // Search Web
            new CommandData { Text = "найди в интернете", Label = "search_web" },
            new CommandData { Text = "поиск информации", Label = "search_web" },
            new CommandData { Text = "поиск данных", Label = "search_web" },
            new CommandData { Text = "открой поисковик", Label = "search_web" },
            new CommandData { Text = "поисковик", Label = "search_web" },
            new CommandData { Text = "яндекс", Label = "search_web" },

            // Weather
            new CommandData { Text = "какая погода", Label = "weather" },
            new CommandData { Text = "погода", Label = "weather" },
            new CommandData { Text = "погода на завтра", Label = "weather" },
            new CommandData { Text = "посмотреть погоду", Label = "weather" },
            new CommandData { Text = "актуальная погода", Label = "weather" },

            // Play Music
            new CommandData { Text = "включи музыку", Label = "play_music" },
            new CommandData { Text = "музыка", Label = "play_music" },
            new CommandData { Text = "включи плеер", Label = "play_music" },
            new CommandData { Text = "хочу слушать музыку", Label = "play_music" },
            new CommandData { Text = "включи аудио", Label = "play_music" },

            // Program Exit
            new CommandData { Text = "выйти", Label = "program_exit" },
            new CommandData { Text = "хватит", Label = "program_exit" },
            new CommandData { Text = "закрой программу", Label = "program_exit" },
            new CommandData { Text = "выключись", Label = "program_exit" },
            new CommandData { Text = "стоп", Label = "program_exit" }
        };

        public MLCommandClassifier()
        {
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _programConstsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>();

            // Init vosk
            _voskModel = new VoskModel.VoskModel();

            // Init event
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;

            InitializeML();
        }

        public void Initialize()
        {
            TrainModel();
            CreatePredictionEngine();

            if (_programConstsClass.DebugMode == true)
            {
                TestModel();
            }
        }

        private void InitializeML()
        {
            // Init ML context with seed
            _mlContext = new MLContext(seed: 0);
            _trainingDataView = _mlContext.Data.LoadFromEnumerable(_trainingData);
        }

        private void TrainModel()
        {
            try
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews("Начало обучения модели...");

                // Split data on training and test array (80/20)
                var trainTestSplit = _mlContext!.Data.TrainTestSplit(_trainingDataView, testFraction: 0.2);
                var trainData = trainTestSplit.TrainSet;
                var testData = trainTestSplit.TestSet;

                // Create simplified pipeline
                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                        outputColumnName: "LabelKey",
                        inputColumnName: "Label")
                    .Append(_mlContext.Transforms.Text.FeaturizeText(
                        outputColumnName: "Features",
                        inputColumnName: "Text"))
                    .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                        labelColumnName: "LabelKey",
                        featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: "PredictedLabel",
                        inputColumnName: "PredictedLabel"));

                // Train model
                _model = pipeline.Fit(trainData);

                // Model evaluation
                var testMetrics = _mlContext.MulticlassClassification.Evaluate(
                    _model.Transform(testData),
                    labelColumnName: "LabelKey");

                if (_programConstsClass.DebugMode == true)
                {
                    _voskModelNewsPublisher?.PublishNews($"Обучение завершено!\n" +
                        $"Точность модели: {testMetrics.MacroAccuracy:P2}\n" +
                        $"LogLoss: {testMetrics.LogLoss:F4}");
                }
            }
            catch (Exception ex)
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Ошибка при обучении: {ex.Message}");

                // Fallback to simple pattern matching
                _model = null;
            }
        }

        private void CreatePredictionEngine()
        {
            if (_model != null)
            {
                _predictionEngine = _mlContext!.Model.CreatePredictionEngine<CommandData, CommandPrediction>(_model);
            }
        }

        public string PredictCommand(string userInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userInput))
                    return "unknown";

                // Use ML model if available
                if (_predictionEngine != null)
                {
                    var prediction = _predictionEngine.Predict(new CommandData { Text = userInput });

                    if (_programConstsClass.DebugMode == true)
                    {
                        _voskModelNewsPublisher?.PublishNews($"Передано: '{userInput}'\n" +
                            $"Предсказание: {prediction.PredictedLabel}");
                        MessageBox.Show($"Передано: '{userInput}'\n" +
                            $"Предсказание: {prediction.PredictedLabel}");
                    }

                    return prediction.PredictedLabel!;
                }
                else
                {
                    // Fallback to simple pattern matching
                    return PredictCommandFallback(userInput);
                }
            }
            catch (Exception ex)
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Ошибка предсказания: {ex.Message}");

                return PredictCommandFallback(userInput);
            }
        }

        private string PredictCommandFallback(string userInput)
        {
            userInput = userInput.ToLower();

            // Simple pattern matching as fallback
            if (userInput.Contains("ютуб") || userInput.Contains("видео") || userInput.Contains("браузер"))
                return "open_browser";
            else if (userInput.Contains("поиск") || userInput.Contains("найди") || userInput.Contains("интернет"))
                return "search_web";
            else if (userInput.Contains("погода"))
                return "weather";
            else if (userInput.Contains("музык") || userInput.Contains("плеер") || userInput.Contains("аудио"))
                return "play_music";
            else if (userInput.Contains("выйти") || userInput.Contains("хватит") || userInput.Contains("стоп"))
                return "program_exit";
            else
                return "unknown";
        }

        public void TestModel()
        {
            if (_programConstsClass.DebugMode == true)
                _voskModelNewsPublisher?.PublishNews("=== ТЕСТИРОВАНИЕ МОДЕЛИ ===");

            var testCases = new[]
            {
                "открой ютуб",
                "какая погода сегодня",
                "включи музыку пожалуйста",
                "выйти из программы",
                "найди информацию о машинах",
                "стоп",
                "хватит работать",
                "погода в москве"
            };

            foreach (var testCase in testCases)
            {
                var result = PredictCommand(testCase);
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Тест: '{testCase}' -> '{result}'");
            }
        }

        // Method for saving the model
        public void SaveModel(string filePath)
        {
            try
            {
                if (_model != null)
                {
                    _mlContext!.Model.Save(_model, _trainingDataView!.Schema, filePath);

                    if (_programConstsClass.DebugMode == true)
                        _voskModelNewsPublisher?.PublishNews($"Модель сохранена: {filePath}");
                }
            }
            catch (Exception ex)
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Ошибка при сохранении модели: {ex.Message}");
            }
        }

        // The method for loading the model
        public void LoadModel(string filePath)
        {
            try
            {
                DataViewSchema schema;
                _model = _mlContext!.Model.Load(filePath, out schema);
                CreatePredictionEngine();

                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Модель загружена: {filePath}");
            }
            catch (Exception ex)
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Ошибка при загрузке модели: {ex.Message}");
            }
        }
    }
}