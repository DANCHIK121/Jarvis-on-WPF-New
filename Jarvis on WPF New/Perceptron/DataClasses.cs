// Installed usings
using Microsoft.ML.Data;

namespace Jarvis_on_WPF_New.Perceptron
{
    public class CommandData
    {
        [LoadColumn(0)]
        public string? Text { get; set; }

        [LoadColumn(1)]
        public string? Label { get; set; }
    }

    public class CommandPrediction
    {
        [ColumnName("PredictedLabel")]
        public string? PredictedLabel { get; set; }

        [ColumnName("Score")]
        public float[]? Score { get; set; }
    }
}
