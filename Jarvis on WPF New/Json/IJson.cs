namespace Jarvis_on_WPF_New.Json
{
    internal interface IJson
    {
        string FilePath { get; set; }

        public T ReadJson<T>();
        public void WriteJson<T>(T deserializeClass);
    }
}