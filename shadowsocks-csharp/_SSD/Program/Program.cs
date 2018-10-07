using Shadowsocks.Controller;
using System;
using System.Windows.Forms;

namespace Shadowsocks {
    public partial class Program {
        private static void _UnexpectedError(bool UI, string message) {
            string textUI = UI ? "UI" : "non-UI";
            MessageBox.Show(
                $"{I18N.GetString("Unexpected error, shadowsocks will exit. Please report to")} https://github.com/CGDF-Github/SSD-Windows/issues {Environment.NewLine}{message}",
                "Shadowsocks " + textUI + " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
