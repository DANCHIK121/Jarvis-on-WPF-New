namespace Jarvis_on_WPF_New.Perceptron
{
    public enum CommandsEnum
    {
        OpenBrowser,
        SearchWeb,
        Weather,
        PlayMusic,
        ProgramExit,
        MinimizeWindows,
        CloseAllWindows,
    }

    internal static class Commands
    {
        // Convert commands
        public static Dictionary<string, CommandsEnum> CommandsConvertDictionary = new Dictionary<string, CommandsEnum>
        {
            ["open_browser"] = CommandsEnum.OpenBrowser,
            ["search_web"] = CommandsEnum.SearchWeb,
            ["weather"] = CommandsEnum.Weather,
            ["play_music"] = CommandsEnum.PlayMusic,
            ["program_exit"] = CommandsEnum.ProgramExit,
            ["minimize_windows"] = CommandsEnum.MinimizeWindows,
            ["close_all_windows"] = CommandsEnum.CloseAllWindows,
        };

        // Create data for training
        public static List<CommandData> _trainingData = new List<CommandData>
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
            new CommandData { Text = "завтрашняя погода", Label = "weather" },

            // Play Music
            new CommandData { Text = "включи музыку", Label = "play_music" },
            new CommandData { Text = "музыка", Label = "play_music" },
            new CommandData { Text = "включи плеер", Label = "play_music" },
            new CommandData { Text = "хочу слушать музыку", Label = "play_music" },
            new CommandData { Text = "включи аудио", Label = "play_music" },
            new CommandData { Text = "включи музыку", Label = "play_music" },

            // Program Exit
            new CommandData { Text = "выйти", Label = "program_exit" },
            new CommandData { Text = "хватит", Label = "program_exit" },
            new CommandData { Text = "закрой программу", Label = "program_exit" },
            new CommandData { Text = "закройся", Label = "program_exit" },
            new CommandData { Text = "выключись", Label = "program_exit" },
            new CommandData { Text = "стоп", Label = "program_exit" },
            new CommandData { Text = "выход", Label = "program_exit" },

            // Minimize windows
            new CommandData { Text = "Сверни все окна", Label = "minimize_windows" },
            new CommandData { Text = "Сверни окна", Label = "minimize_windows" },
            new CommandData { Text = "Сверни открытые окна", Label = "minimize_windows" },
            new CommandData { Text = "Спрячь окна", Label = "minimize_windows" },
            new CommandData { Text = "Спрячь все окна", Label = "minimize_windows" },
            new CommandData { Text = "Скрой все окна", Label = "minimize_windows" },
            new CommandData { Text = "Скрой окна", Label = "minimize_windows" },

            // Close all windows
            new CommandData { Text = "Закрой все окна", Label = "close_all_windows" },
            new CommandData { Text = "Закрой окна", Label = "close_all_windows" },
            new CommandData { Text = "Закрой открытые окна", Label = "close_all_windows" },
            new CommandData { Text = "Заверши окна", Label = "close_all_windows" },
            new CommandData { Text = "Заверши все окна", Label = "close_all_windows" },
            new CommandData { Text = "Закрой лишнее", Label = "close_all_windows" },
            new CommandData { Text = "Ничего не должно быть открыто.", Label = "close_all_windows" }
        };

        public static string[] testCases = new[]
        {
            "открой ютуб",

            "какая погода сегодня",
            "погода в москве",

            "включи музыку пожалуйста",

            "выйти из программы",
            "стоп",
            "хватит работать",

            "найди информацию о машинах",

            "сверни все окна",
            "спрячь все окна",
            "скрой все окна",

            "закрой все окна",
            "закрой окна",
            "закрой лишнее",
        };

        public static string PredictCommandFallback(string userInput)
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
            else if (userInput.Contains("сверни") || userInput.Contains("спрячь") || userInput.Contains("скрой"))
                return "minimize_windows";
            else if (userInput.Contains("закрой") || userInput.Contains("заверши"))
                return "close_all_windows";
            else
                return "unknown";
        }
    }
}
