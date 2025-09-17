// Installed usings
using Microsoft.ML;

// Project usings
using Jarvis_on_WPF_New.Json;
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
            _voskModel = new VoskModel.VoskModelClass();

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
            _trainingDataView = _mlContext.Data.LoadFromEnumerable(Commands._trainingData);
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
                    return Commands.PredictCommandFallback(userInput);
                }
            }
            catch (Exception ex)
            {
                if (_programConstsClass.DebugMode == true)
                    _voskModelNewsPublisher?.PublishNews($"Ошибка предсказания: {ex.Message}");

                return Commands.PredictCommandFallback(userInput);
            }
        }

        public void TestModel()
        {
            if (_programConstsClass.DebugMode == true)
                _voskModelNewsPublisher?.PublishNews("=== ТЕСТИРОВАНИЕ МОДЕЛИ ===");

            foreach (var testCase in Commands.testCases)
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