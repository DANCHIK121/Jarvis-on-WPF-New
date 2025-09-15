
using System.Windows.Forms;

namespace Jarvis_on_WPF_New.VoskModel
{
    internal class VoskModelCommandExecution
    {
        public static void Execute(string command)
        {
            command = command.ToLower();

            switch (command)
            {
                case "выход":
                    MessageBox.Show("ff");
                    Application.Exit();
                    break;
            }
        }
    }
}
