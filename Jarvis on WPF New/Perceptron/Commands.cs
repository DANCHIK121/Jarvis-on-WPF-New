namespace Jarvis_on_WPF_New.Perceptron
{
    public enum CommandsEnum
    {
        Sleep,

        Weather,

        SearchWeb,

        PlayMusic,

        ProgramExit,

        OpenBrowser,

        MinimizeWindows,

        CloseAllWindows,

        OpenVideoHostingWebSite,
    }

    internal static class Commands
    {
        // Convert commands
        public static Dictionary<string, CommandsEnum> CommandsConvertDictionary = new Dictionary<string, CommandsEnum>
        {
            ["sleep"] = CommandsEnum.Sleep,
            ["weather"] = CommandsEnum.Weather,
            ["play_music"] = CommandsEnum.PlayMusic,
            ["search_web"] = CommandsEnum.SearchWeb,
            ["open_browser"] = CommandsEnum.OpenBrowser,
            ["program_exit"] = CommandsEnum.ProgramExit,
            ["minimize_windows"] = CommandsEnum.MinimizeWindows,
            ["close_all_windows"] = CommandsEnum.CloseAllWindows,
            ["open_video_hosting_website"] = CommandsEnum.OpenVideoHostingWebSite,
        };

        // Create data for training
        public static List<CommandData> _trainingData = new List<CommandData>
        {
            // Open video hosting website
            new CommandData { Text = "видео", Label = "open_video_hosting_website" },
            new CommandData { Text = "хочу видео", Label = "open_video_hosting_website" },
            new CommandData { Text = "смотреть видео", Label = "open_video_hosting_website" },
            new CommandData { Text = "открой видеосервис", Label = "open_video_hosting_website" },
            new CommandData { Text = "открой видео сервис", Label = "open_video_hosting_website" },
            new CommandData { Text = "хочу смотреть видео", Label = "open_video_hosting_website" },

            // Open Browser
            new CommandData { Text = "открой браузер", Label = "open_browser" },
            new CommandData { Text = "выйди в интернет", Label = "open_browser" },
            new CommandData { Text = "включи мне интернет", Label = "open_browser" },
            new CommandData { Text = "мне нужно кое-что погуглить", Label = "open_browser" },
            new CommandData { Text = "запусти, пожалуйста, мой браузер", Label = "open_browser" },
            new CommandData { Text = "распахни окно во всемирную паутину", Label = "open_browser" },

            // Search Web
            new CommandData { Text = "яндекс", Label = "search_web" },
            new CommandData { Text = "поисковик", Label = "search_web" },
            new CommandData { Text = "поиск данных", Label = "search_web" },
            new CommandData { Text = "открой поисковик", Label = "search_web" },
            new CommandData { Text = "поиск информации", Label = "search_web" },
            new CommandData { Text = "найди в интернете", Label = "search_web" },

            // Weather
            new CommandData { Text = "погода", Label = "weather" },
            new CommandData { Text = "какая погода", Label = "weather" },
            new CommandData { Text = "погода на завтра", Label = "weather" },
            new CommandData { Text = "посмотреть погоду", Label = "weather" },
            new CommandData { Text = "актуальная погода", Label = "weather" },
            new CommandData { Text = "завтрашняя погода", Label = "weather" },

            // Play Music
            new CommandData { Text = "музыка", Label = "play_music" },
            new CommandData { Text = "включи плеер", Label = "play_music" },
            new CommandData { Text = "включи аудио", Label = "play_music" },
            new CommandData { Text = "включи музыку", Label = "play_music" },
            new CommandData { Text = "включи музыку", Label = "play_music" },
            new CommandData { Text = "хочу слушать музыку", Label = "play_music" },

            // Program Exit
            new CommandData { Text = "стоп", Label = "program_exit" },
            new CommandData { Text = "выход", Label = "program_exit" },
            new CommandData { Text = "выйти", Label = "program_exit" },
            new CommandData { Text = "хватит", Label = "program_exit" },
            new CommandData { Text = "закройся", Label = "program_exit" },
            new CommandData { Text = "выключись", Label = "program_exit" },
            new CommandData { Text = "закрой программу", Label = "program_exit" },
            
            // Minimize windows
            new CommandData { Text = "скрой окна", Label = "minimize_windows" },
            new CommandData { Text = "сверни окна", Label = "minimize_windows" },
            new CommandData { Text = "спрячь окна", Label = "minimize_windows" },
            new CommandData { Text = "скрой все окна", Label = "minimize_windows" },
            new CommandData { Text = "сверни все окна", Label = "minimize_windows" },
            new CommandData { Text = "спрячь все окна", Label = "minimize_windows" },
            new CommandData { Text = "сверни открытые окна", Label = "minimize_windows" },

            // Close all windows
            new CommandData { Text = "закрой окна", Label = "close_all_windows" },
            new CommandData { Text = "заверши окна", Label = "close_all_windows" },
            new CommandData { Text = "закрой лишнее", Label = "close_all_windows" },
            new CommandData { Text = "закрой все окна", Label = "close_all_windows" },
            new CommandData { Text = "заверши все окна", Label = "close_all_windows" },
            new CommandData { Text = "закрой открытые окна", Label = "close_all_windows" },
            new CommandData { Text = "ничего не должно быть открыто", Label = "close_all_windows" },

            // Sleep
            new CommandData { Text = "сон", Label = "sleep" },
            new CommandData { Text = "сонный режим", Label = "sleep" },
            new CommandData { Text = "переход в сон", Label = "sleep" },
            new CommandData { Text = "активируй сон", Label = "sleep" },
            new CommandData { Text = "перейти в сон", Label = "sleep" },
            new CommandData { Text = "активируй сонный режим", Label = "sleep" },
            new CommandData { Text = "перейти в сонный режим", Label = "sleep" },
        };

        public static string[] testCases = new[]
        {
            // Open browser
            "открой браузер",
            "выйди в интернет",

            // Open video hosting website
            "видео",
            "хочу смотреть видео",

            // Weather
            "погода в городе",
            "какая погода сегодня",

            // Play music
            "включи музыку пожалуйста",

            // Program exit
            "стоп",
            "хватит работать",
            "выйти из программы",
            
            // Search web
            "найди информацию о машинах",

            // Minimize windows
            "скрой все окна",
            "сверни все окна",
            "спрячь все окна",
            
            // Close windows
            "закрой все окна",
            "закрой окна",
            "закрой лишнее",

            // Sleep
            "сон",
            "сонный режим"
        };

        public static string PredictCommandFallback(string userInput)
        {
            userInput = userInput.ToLower();

            // Simple pattern matching as fallback
            // Open browser
            if (userInput.Contains("браузер") || userInput.Contains("интернет") || userInput.Contains("всемирная сеть"))
                return "open_browser";

            // Open video hosting website
            if (userInput.Contains("видео") || userInput.Contains("смотреть видео") || userInput.Contains("хочу смотреть видео"))
                return "open_video_hosting_website";

            // Search web
            else if (userInput.Contains("поиск") || userInput.Contains("найди") || userInput.Contains("интернет"))
                return "search_web";

            // Weather
            else if (userInput.Contains("погода"))
                return "weather";

            // Play music
            else if (userInput.Contains("музыка") || userInput.Contains("плеер") || userInput.Contains("аудио"))
                return "play_music";

            // Program exit
            else if (userInput.Contains("выйти") || userInput.Contains("хватит") || userInput.Contains("стоп"))
                return "program_exit";

            // Minimize windows
            else if (userInput.Contains("сверни") || userInput.Contains("спрячь") || userInput.Contains("скрой"))
                return "minimize_windows";

            // Close windows
            else if (userInput.Contains("закрой") || userInput.Contains("заверши"))
                return "close_all_windows";

            // Sleep
            else if (userInput.Contains("сонный") || userInput.Contains("сон"))
                return "sleep";

            // Unknown command
            else
                return "unknown";
        }
    }
}
