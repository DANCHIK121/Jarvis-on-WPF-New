// Standart usings
using System.Windows;

// Project usings
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;
using Jarvis_on_WPF_New.JarvisAudioResponses;

namespace Jarvis_on_WPF_New
{
    public partial class MainWindow : Window
    {
        // Vosk model
        private IVoskModel? _voskModel;

        // Json audio responses
        private readonly IAudio? _jarvisAudioResponses;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;
        private VoskModelEventsForTextChattingInThreads? _voskModelEventsForTextChattingInThreads;

        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _constsClass;

        public MainWindow()
        {
            InitializeComponent();

            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _constsClass = new ProgramConstsClass(); // Programm const class
            _constsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file

            // Init vosk model
            _voskModel = new VoskModelClass();

            // Init VoskModelEventsForNews
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;
            _voskModelEventsForTextChattingInThreads = _voskModel.GetVoskModelEventsForTextChattingInThreads;

            // Jarvis audio responses
            _jarvisAudioResponses = new Audio(AudioModes.JarvisGreeting);
            _jarvisAudioResponses.Play();

            // Update TextBlock
            Thread thread = new Thread(() =>
            {
                _voskModelEventsForTextChattingInThreads?.TextPublished += (s, text) =>
                {
                    UpdateTextBlockAsync(text);
                };

                if (_constsClass.DebugMode! == true)
                {
                    _voskModelNewsPublisher?.NewsPublished += (s, news) =>
                    {
                        UpdateTextBlockAsync(news);
                    };
                }

                // Download model
                _voskModel.DownloadModel();

                // Start listening
                while (true)
                {
                    _voskModel.StartListening();
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void Close_Window_Click(object sender, RoutedEventArgs e) => Close();
        private void Hide_Window_Click(object sender, RoutedEventArgs e) => this.Hide();
        private void Exit_Program_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
        private void Clear_Recognized_TextBlock_Click(object sender, RoutedEventArgs e) => RecognizedText.Text = "";
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { e.Cancel = true; this.Hide(); }

        private void UpdateTextBlockAsync(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!RecognizedText.Text.Contains(text))
                    RecognizedText.Text += text + "\n";
            }));
        }
    }
}
