namespace Jarvis_on_WPF.Json
{
    internal interface IJson
    {
        string FilePath { get; set; }

        public T ReadJson<T>();
        public void WriteJson(DataClass deserializeClass);
    }
}