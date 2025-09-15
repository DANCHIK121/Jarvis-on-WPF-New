namespace Jarvis_on_WPF_New.Perceptron
{
    public class NeuralNetwork
    {
        private int _inputSize;
        private int _outputSize;

        private double[] _biases;
        private double[,] _weights;

        public NeuralNetwork(int inputSize, int outputSize)
        {
            // Init all variables
            this._inputSize = inputSize;
            this._outputSize = outputSize;

            _biases = new double[_outputSize];
            _weights = new double[_inputSize, _outputSize];

            // Init variables with random values
            Random random = new Random();
            for (int i = 0; i < _inputSize; i++)
            {
                for (int j = 0; j < _outputSize; j++)
                {
                    _weights[i, j] = random.NextDouble() - 0.5;
                }
            }

            for (int i = 0; i < _outputSize; i++)
            {
                _biases[i] = random.NextDouble() - 0.5;
            }
        }

        // Direct distribution
        public double[] Forward(double[] input)
        {
            double[] output = new double[_outputSize];

            for (int j = 0; j < _outputSize; j++)
            {
                double sum = _biases[j];
                for (int i = 0; i < _inputSize; i++)
                {
                    sum += input[i] * _weights[i, j];
                }
                output[j] = Sigmoid(sum);
            }

            return output;
        }

        // Train neural network
        public void Train(double[] input, double[] target, double learningRate = 0.1)
        {
            double[] output = Forward(input);
            double[] error = new double[_outputSize];

            // Error calculate
            for (int i = 0; i < _outputSize; i++)
            {
                error[i] = target[i] - output[i];
            }

            // Update weights and biases
            for (int j = 0; j < _outputSize; j++)
            {
                for (int i = 0; i < _inputSize; i++)
                {
                    _weights[i, j] += learningRate * error[j] * input[i] * output[j] * (1 - output[j]);
                }
                _biases[j] += learningRate * error[j] * output[j] * (1 - output[j]);
            }
        }

        private double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }
    }
}
