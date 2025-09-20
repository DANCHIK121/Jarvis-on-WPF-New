// Standart usings
using System.Windows;

// Project usings
using Jarvis_on_WPF_New.Json;

namespace Jarvis_on_WPF_New
{
    public partial class AvailableCommandsWindow : Window
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _constsClass;

        // Local variables
        private int _commandsIndex = 0;

        public AvailableCommandsWindow()
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

            // Open browser
            CommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.OpenBrowser!.Category + "\n";
            foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.OpenBrowser!.Commands!)
            {
                CommandsLabel.Text += item + "\n";
            }

            // Search web
            SecondCommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.SearchWeb!.Category + "\n";
            foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.SearchWeb!.Commands!)
            {
                SecondCommandsLabel.Text += item + "\n";
            }
        }

        private void Close_Window_Click(object sender, RoutedEventArgs e) => Close();
        private void Hide_Window_Click(object sender, RoutedEventArgs e) => this.Hide();
        private void Exit_Program_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { e.Cancel = true; this.Hide(); }

        private void UpdateLabels(object sender, RoutedEventArgs e)
        {
            switch (_commandsIndex)
            {
                case 0:
                    ResetLabels();

                    // First label
                    CommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.OpenBrowser!.Category + "\n";
                    foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.OpenBrowser!.Commands!)
                    {
                        CommandsLabel.Text += item + "\n";
                    }

                    // Second label
                    SecondCommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.SearchWeb!.Category + "\n";
                    foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.SearchWeb!.Commands!)
                    {
                        SecondCommandsLabel.Text += item + "\n";
                    }
                    break;

                case 1:
                    ResetLabels();

                    // First label
                    CommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.Weather!.Category + "\n";
                    foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.Weather!.Commands!)
                    {
                        CommandsLabel.Text += item + "\n";
                    }

                    // Second label
                    SecondCommandsLabel.Text += _constsClass.AvailibleCommands!.VoiceCommands!.PlayMusic!.Category + "\n";
                    foreach (var item in _constsClass.AvailibleCommands!.VoiceCommands!.PlayMusic!.Commands!)
                    {
                        SecondCommandsLabel.Text += item + "\n";
                    }
                    break;
            }
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            _commandsIndex++;
        }

        private void ResetLabels()
        {
            CommandsLabel.Text = "";
            SecondCommandsLabel.Text = "";
        }
    }
}
