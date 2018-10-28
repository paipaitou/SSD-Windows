using Microsoft.Win32;
using Shadowsocks.Controller;
using Shadowsocks.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Shadowsocks {
    public static partial class Program {

        private static void _ReleasePlugin() {
            var directory=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //simple-obfs
            File.WriteAllBytes(
                Path.Combine(directory, "simple-obfs.exe"),
                Resources.obfs_local
            );
            File.WriteAllBytes(
                Path.Combine(directory, "libwinpthread-1.dll"),
                Resources.libwinpthread_1
            );

            //kcptun
            var registryCPUKey=Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var registryCPUValue=registryCPUKey.GetValue("ProcessorNameString").ToString();
            registryCPUKey.Close();
            byte[] kcptunResource=null;
            if(registryCPUValue.IndexOf("Intel") != -1) {
                kcptunResource = Resources.client_windows_386;
            }
            else if(registryCPUValue.IndexOf("AMD") != -1) {
                kcptunResource = Resources.client_windows_amd64;
            }
            else {
                MessageBox.Show(I18N.GetString("Cannot recognize the type of this device's CPU. If SSD is not running in virtual environment, you can submit a issue to help us improve SSD."));
            }
            if(kcptunResource != null) {
                File.WriteAllBytes(Path.Combine(directory, "kcptun.exe"), kcptunResource);
            }
        }
        private static void _UnexpectedError(bool UI, string message) {
            string textUI = UI ? "UI" : "non-UI";
            MessageBox.Show(
                $"{I18N.GetString("Unexpected error, shadowsocks will exit. Please report to")} https://github.com/CGDF-Github/SSD-Windows/issues {Environment.NewLine}{message}",
                "Shadowsocks " + textUI + " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
