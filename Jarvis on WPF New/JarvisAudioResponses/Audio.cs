// System usings
using System.Media;
using System.Diagnostics.CodeAnalysis;

namespace Jarvis_on_WPF_New.JarvisAudioResponses
{
    public enum AudioModes
    {
        Default,

        YesSer,
        PowerOff,
        Invisible,
        YesSerSecond,
        JarvisGoodbye,
        JarvisGreeting,
    }

    [SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы")]
    public class Audio : IAudio
    {
        private string _filePath;

        private AudioModes _audioMode;
        private SoundPlayer _soundPlayer;

        public Audio()
        {
            _filePath = string.Empty;
            _audioMode = AudioModes.Default;
            _soundPlayer = new SoundPlayer();
        }

        public Audio(AudioModes audioMode)
        {
            this._filePath = string.Empty;
            this._audioMode = audioMode;
            this._soundPlayer = new();
        }

        public Audio(AudioModes audioMode, SoundPlayer soundPlayer)
        {
            _filePath = string.Empty;
            this._audioMode = audioMode;
            this._soundPlayer = new();
        }

        public Audio(AudioModes audioMode, SoundPlayer soundPlayer, string filePath)
        {
            this._filePath = filePath;
            this._audioMode = audioMode;
            this._soundPlayer = new();
        }

        ~Audio()
        {
            _soundPlayer.Dispose();
        }

        public AudioModes ChangeAudioMode
        {
            get { return _audioMode; }
            set { _audioMode = value; }
        }

        public string GetFilePathByAudioMode(AudioModes audioMode = AudioModes.Default)
        {
            switch (_audioMode)
            {
                case AudioModes.JarvisGreeting:
                    _filePath = @"./Audio/Джарвис - приветствие.wav";
                    break;

                case AudioModes.Invisible:
                    _filePath = @"./Audio/Да, это поможет вам оставаться незамеченным.wav";
                    break;

                case AudioModes.PowerOff:
                    _filePath = @"./Audio/Отключаю питание.wav";
                    break;

                case AudioModes.JarvisGoodbye:
                    _filePath = @"./Audio/Как пожелаете.wav";
                    break;

                case AudioModes.YesSer:
                    _filePath = @"./Audio/Да сэр.wav";
                    break;

                case AudioModes.YesSerSecond:
                    _filePath = @"./Audio/Да сэр(второй).wav";
                    break;

                case AudioModes.Default:
                    _filePath = string.Empty;
                    break;
            }

            return _filePath;
        }

        public void Play()
        {
            _soundPlayer = new SoundPlayer(GetFilePathByAudioMode(_audioMode));
            _soundPlayer.Play();
        }

        public void Stop()
        {
            _soundPlayer = new SoundPlayer(GetFilePathByAudioMode(_audioMode));
            _soundPlayer.Stop();
        }
    }
}