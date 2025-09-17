// Standart usings
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Jarvis_on_WPF_New.CommandsExecution.FilesForCommands.MinimizeWindows
{
    public class MinimizeWindows : IMinimizeWindows
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_HIDE = 0;
        private const byte VK_D = 0x44;
        private const byte VK_LWIN = 0x5B;
        private const uint WM_CLOSE = 0x0010;
        private const uint KEYEVENTF_KEYUP = 0x2;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
        {
            // Проверяем, видимо ли окно и не является ли системным
            if (IsWindowVisible(hWnd))
            {
                GetWindowThreadProcessId(hWnd, out uint processId);

                // Исключаем системные процессы и текущий процесс
                if (processId != 0 && processId != (uint)Process.GetCurrentProcess().Id)
                {
                    // Закрываем окно отправкой сообщения WM_CLOSE
                    PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
            }
            return true;
        }

        public void MinimizeAllWindows()
        {
            // Press Win + D
            keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
            keybd_event(VK_D, 0, 0, UIntPtr.Zero);
            keybd_event(VK_D, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public void CloseAllWindows()
        {
            EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        }
    }
}