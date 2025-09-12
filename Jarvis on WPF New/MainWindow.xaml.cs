// Standart usings
using System;
using System.Windows;

namespace Jarvis_on_WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_Window_Click(object sender, RoutedEventArgs e) => Close();
        private void Exit_Program_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
    }
}
