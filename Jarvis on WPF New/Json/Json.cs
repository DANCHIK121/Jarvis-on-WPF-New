// Standart usings
using System.IO;
using System.Windows;

// Installed usings
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Jarvis_on_WPF_New.Json
{
    #region Programm Consts
    // Program consts
    class ProgramConstsClass
    {
        // Main settings
        public bool? DebugMode { get; set; }

        // Vosk model consts
        public bool? AccurateRecognitionMode { get; set; }
        public string? AccurateRecognitionModelName { get; set; }
        public string? NotAccurateRecognitionModelName { get; set; }
        public string? URLForDownloadVoskModel { get; set; }
        public int? SampleRate {  get; set; }
        public int? BufferMilliseconds { get; set; }
        public int? NumberOfBuffers { get; set; }
        public int? SilenceTimerTimeoutInMilliSeconds { get; set; }
        public VoiceCommandSystem? AvailibleCommands { get; set; }

        // Default websites
        public string? DefaultSearchEngine { get; set; }
        public string? DefaultVideoHosting { get; set; }
        public string? DefaultWebsiteWithWeather { get; set; }
    }
    #endregion

    #region Availiblie commands
    // Availiblie commands
    public class VoiceCommandSystem
    {
        [JsonProperty("voice_commands")]
        public VoiceCommands? VoiceCommands { get; set; }

        [JsonProperty("metadata")]
        public Metadata? Metadata { get; set; }
    }

    public class VoiceCommands
    {
        [JsonProperty("open_browser")]
        public CommandCategory? OpenBrowser { get; set; }

        [JsonProperty("search_web")]
        public CommandCategory? SearchWeb { get; set; }

        [JsonProperty("weather")]
        public CommandCategory? Weather { get; set; }

        [JsonProperty("play_music")]
        public CommandCategory? PlayMusic { get; set; }

        [JsonProperty("program_exit")]
        public CommandCategory? ProgramExit { get; set; }

        [JsonProperty("minimize_windows")]
        public CommandCategory? MinimizeWindows { get; set; }

        [JsonProperty("close_all_windows")]
        public CommandCategory? CloseAllWindows { get; set; }

        [JsonProperty("sleep")]
        public CommandCategory? Sleep { get; set; }
    }

    public class CommandCategory
    {
        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("commands")]
        public List<string>? Commands { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("total_categories")]
        public int? TotalCategories { get; set; }

        [JsonProperty("total_commands")]
        public int? TotalCommands { get; set; }

        [JsonProperty("version")]
        public string? Version { get; set; }

        [JsonProperty("language")]
        public string? Language { get; set; }
    }
    #endregion

    #region PathConsts
    // Path Consts
    class PathsClass
    {
        public string? DataJsonPathConst { get; set; }
        public string? AccurateModelPathConst { get; set; }
        public string? NotAccurateModelPathConst { get; set; }
    }
    #endregion

    // Serialization and deserialization class
    internal class JsonClass : IJson
    {
        private string _filePath; // Json file path

        public static string _jsonFileWithPathConsts = @"./Json/PathConsts.json"; // Json file path
        public static string _jsonFileWithProgramConsts = @"./Json/ProgramConsts.json"; // Json file path

        public JsonClass()
        {
            _filePath = string.Empty;
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (File.Exists(path: value))
                    _filePath = value; // If file exists in value path
                else
                {
                    FileStream fs = File.Create(value); // Creating new json file
                    fs.Close();
                    _filePath = value;
                }
            }
        }

        public T ReadJson<T>()
        {
            using (FileStream fileStream = new(_filePath, FileMode.Open))
            {
                using (StreamReader file = new(fileStream))
                {
                    try
                    {
                        string json = file.ReadToEnd(); // Reading data from json file

                        var serializerSettings = new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        };

                        // Writing converted data
                        T deserialized = JsonConvert.DeserializeObject<T>(json, serializerSettings)!;
                        return deserialized;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return default!;
                    }
                }
            }
        }

        async public void WriteJson<T>(T deserializeClass)
        {
            using FileStream fileStream = new(_filePath, FileMode.Open);
            using StreamWriter streamWriter = new(fileStream);

            // Writing serializes data to file
            var serializeClass = System.Text.Json.JsonSerializer.Serialize(value: deserializeClass);
            await Task.Run(() => streamWriter.WriteAsync(serializeClass));
        }
    }
}
