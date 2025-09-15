// Standart usings
using Jarvis_on_WPF.Json;
// Project usings
using Jarvis_on_WPF_New.VoskModel;
using System.Windows;

namespace Jarvis_on_WPF
{
    public partial class MainWindow : Window
    {
        // Vosk model
        private IVoskModel? _voskModel;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;
        private VoskModelEventsForTextChattingInThreads? _voskModelEventsForTextChattingInThreads;

        // Json classes
        private readonly IJson _jsonForProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _constsClass;

        public MainWindow()
        {
            InitializeComponent();

            // Programm consts
            _jsonForProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _constsClass = new ProgramConstsClass(); // Programm const class
            _constsClass = _jsonForProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file

            // Init vosk model
            _voskModel = new VoskModel();

            // Init VoskModelEventsForNews
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;
            _voskModelEventsForTextChattingInThreads = _voskModel.GetVoskModelEventsForTextChattingInThreads;

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
        private void Exit_Program_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

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
