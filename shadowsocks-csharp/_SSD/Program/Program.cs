using Shadowsocks.Controller;
using Shadowsocks.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Shadowsocks {
    public static partial class Program {
        private static void _ReleasePlugin() {
            var is64 = Environment.Is64BitOperatingSystem;
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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
            if(!is64) {
                File.WriteAllBytes(Path.Combine(directory, "kcptun.exe"), Resources.client_windows_386);
            }
            else {
                File.WriteAllBytes(Path.Combine(directory, "kcptun.exe"), Resources.client_windows_amd64);
            }

            //v2ray
            if(!is64) {
                File.WriteAllBytes(Path.Combine(directory, "v2ray.exe"), Resources.v2ray_plugin_windows_386);
            }
            else {
                File.WriteAllBytes(Path.Combine(directory, "v2ray.exe"), Resources.v2ray_plugin_windows_amd64);
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
