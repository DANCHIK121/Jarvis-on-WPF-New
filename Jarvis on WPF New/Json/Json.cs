// Standart usings
using System.IO;
using System.Windows;

// Installed usings
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace Jarvis_on_WPF.Json
{
    #region Programm Consts
    // Main serialization class
    class DataClass
    {
        public bool? DebugMode { get; set; }
        public bool? AccurateRecognitionMode { get; set; }
    }
    #endregion

    #region PathConsts
    // Path Consts
    class PathConstsClass
    {
        public string? DataJsonPathConst { get; set; }
        public string? AccurateModelPathConst { get; set; }
        public string? NotAccurateModelPathConst { get; set; }
    }

    // Main serialization class
    class PathsClass
    {
        public List<PathConstsClass>? PathConsts { get; set; }
    }
    #endregion

    // Serialization and deserialization class
    internal class JsonClass : IJson
    {
        private string _filePath; // Json file path
        public static string jsonFileWithPathConsts = @"./Json/PathConsts.json"; // Json file path

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

        async public void WriteJson(DataClass deserializeClass)
        {
            using FileStream fileStream = new(_filePath, FileMode.Open);
            using StreamWriter streamWriter = new(fileStream);

            // Writing serializes data to file
            var serializeClass = System.Text.Json.JsonSerializer.Serialize(value: deserializeClass);
            await Task.Run(() => streamWriter.WriteAsync(serializeClass));
        }
    }
}
