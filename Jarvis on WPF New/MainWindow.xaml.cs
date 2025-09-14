// Standart usings
// Project usings
using Jarvis_on_WPF_New.VoskModel;
using System.Security.Policy;
using System.Windows;

namespace Jarvis_on_WPF
{
    public partial class MainWindow : Window
    {
        // Vosk model
        private IVoskModel? _voskModel;

        // Event publisher
        private VoskModelEventsForNews? _voskModelNewsPublisher;

        public MainWindow()
        {
            InitializeComponent();

            // Init vosk model
            _voskModel = new VoskModel();

            // Init VoskModelEventsForNews
            _voskModelNewsPublisher = _voskModel.GetVoskModelEventsForNews;

            // Update TextBlock
            Thread thread = new Thread(() =>
            {
                _voskModelNewsPublisher?.NewsPublished += (s, news) =>
                {
                    UpdateTextBlockAsync(news);
                };

                _voskModel.DownloadModel();
            });
            thread.Start();
        }

        private void Close_Window_Click(object sender, RoutedEventArgs e) => Close();
        private void Exit_Program_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void UpdateTextBlockAsync(string text)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                RecognizedText.Text += text + "\n";
            }));
        }
    }
}
