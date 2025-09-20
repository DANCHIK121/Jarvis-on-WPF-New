// Standart usings
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

// Project usings
using Jarvis_on_WPF_New.Json;
using Jarvis_on_WPF_New.VoskModel;

namespace Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.OpenPrograms
{
    class OpenPrograms : IOpenPrograms
    {
        // Json classes
        private readonly IJson _jsonWithProgramConsts;

        // Objects for deserialization
        private readonly ProgramConstsClass _constsClass;

        public OpenPrograms()
        {
            // Programm consts
            _jsonWithProgramConsts = new JsonClass
            {
                FilePath = JsonClass._jsonFileWithProgramConsts,
            };

            // Deserialized class with programm consts
            _constsClass = new ProgramConstsClass(); // Programm const class
            _constsClass = _jsonWithProgramConsts.ReadJson<ProgramConstsClass>(); // Reading data from json file
        }

        public void OpenBrowser(VoskModelEventsForNews? voskModelEventsForNews, bool openWithURL, string url = "")
        {
            try
            {
                if (!openWithURL)
                    Process.Start(GetYandexBrowserPathFromRegistry()!);
                if (openWithURL)
                {
                    if (!string.IsNullOrEmpty(url))
                        Process.Start(GetYandexBrowserPathFromRegistry()!, url);
                    else
                        Process.Start(GetYandexBrowserPathFromRegistry()!, _constsClass.DefaultSearchEngine!);
                }
            }
            catch (Exception ex)
            {
                if (_constsClass.DebugMode! == true)
                {
                    voskModelEventsForNews!.PublishNews($"Ошибка: {ex.Message}");
                }
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
