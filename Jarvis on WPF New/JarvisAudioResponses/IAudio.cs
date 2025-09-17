namespace Jarvis_on_WPF_New.JarvisAudioResponses
{
    public interface IAudio
    {
        public AudioModes ChangeAudioMode { get; set; }

        public void Play();
        public void Stop();
        public string GetFilePathByAudioMode(AudioModes audioMode = AudioModes.Default);
    }
}