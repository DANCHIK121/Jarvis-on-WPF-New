namespace Jarvis_on_WPF_New.Perceptron
{
    public class Perceptron
    {
        //public static void Main(string[] args)
        //{
        //    var assistant = new NeuralVoiceAssistant();

        //    Console.WriteLine("Нейросеть-помощник готова!");
        //    Console.WriteLine("Попробуйте: 'хочу смотреть youtube', 'открой браузер', 'какая погода'");

        //    while (true)
        //    {
        //        Console.Write("\nВы: ");
        //        var input = Console.ReadLine();

        //        if (input?.ToLower() == "выход") break;

        //        var response = assistant.ProcessCommand(input);
        //        Console.WriteLine($"Помощник: {response}");
        //    }
        //}

        
    }

    

    public class TextProcessor
    {
        private List<string> vocabulary;
        private Dictionary<string, int> commandMap;

        public TextProcessor()
        {
            vocabulary = new List<string>
            {
                "хочу", "смотреть", "youtube", "ютуб", "открой", "браузер",
                "найди", "поиск", "погода", "музыка", "включи", "видео"
            };

            commandMap = new Dictionary<string, int>
            {
                ["open_browser"] = 0,
                ["search_web"] = 1,
                ["weather"] = 2,
                ["play_music"] = 3
            };
        }

        // Преобразование текста в вектор
        public double[] TextToVector(string text)
        {
            text = text.ToLower();
            var words = text.Split(' ');
            var vector = new double[vocabulary.Count];

            for (int i = 0; i < vocabulary.Count; i++)
            {
                vector[i] = words.Contains(vocabulary[i]) ? 1.0 : 0.0;
            }

            return vector;
        }

        // Целевой вектор для обучения
        public double[] GetTargetVector(string command)
        {
            var vector = new double[commandMap.Count];
            if (commandMap.ContainsKey(command))
            {
                vector[commandMap[command]] = 1.0;
            }
            return vector;
        }

        public string GetCommandFromOutput(double[] output)
        {
            int maxIndex = 0;
            double maxValue = output[0];

            for (int i = 1; i < output.Length; i++)
            {
                if (output[i] > maxValue)
                {
                    maxValue = output[i];
                    maxIndex = i;
                }
            }

            return commandMap.FirstOrDefault(x => x.Value == maxIndex).Key;
        }

        public int GetVocabularySize() => vocabulary.Count;
        public int GetCommandCount() => commandMap.Count;

        public void AddWordToVocabulary(string word)
        {
            if (!vocabulary.Contains(word.ToLower()))
            {
                vocabulary.Add(word.ToLower());
            }
        }

        public void AddCommand(string commandName)
        {
            if (!commandMap.ContainsKey(commandName))
            {
                commandMap[commandName] = commandMap.Count;
            }
        }

    }

    
}
