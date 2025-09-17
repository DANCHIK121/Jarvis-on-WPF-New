// Standart usings
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.OpenPrograms
{
    class OpenPrograms : IOpenPrograms
    {
        public void OpenBrowserWithUrl(string url = "https://yandex.ru")
        {
            try
            {
                Process.Start(GetYandexBrowserPathFromRegistry()!, url);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static string? GetYandexBrowserPathFromRegistry()
        {
            try
            {
                // Checking in the Current User
                string path = GetPathFromRegistry(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\App Paths\browser.exe")!;

                if (!string.IsNullOrEmpty(path)) return path;

                // Checking in the Local Machine
                path = GetPathFromRegistry(Registry.LocalMachine,
                    @"Software\Microsoft\Windows\CurrentVersion\App Paths\browser.exe")!;

                if (!string.IsNullOrEmpty(path)) return path;

                // Alternative Registry Keys
                path = GetPathFromRegistry(Registry.CurrentUser,
                    @"Software\Microsoft\Windows\CurrentVersion\App Paths\yandex.exe")!;

                if (!string.IsNullOrEmpty(path)) return path;

                path = GetPathFromRegistry(Registry.LocalMachine,
                    @"Software\Microsoft\Windows\CurrentVersion\App Paths\yandex.exe")!;

                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении реестра: {ex.Message}");
                return null;
            }
        }

        private static string? GetPathFromRegistry(RegistryKey rootKey, string subKeyPath)
        {
            using (RegistryKey key = rootKey.OpenSubKey(subKeyPath)!)
            {
                if (key != null)
                {
                    string? path = key.GetValue("") as string;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        return path!;
                    }
                }
            }
            return null;
        }
    }
}
