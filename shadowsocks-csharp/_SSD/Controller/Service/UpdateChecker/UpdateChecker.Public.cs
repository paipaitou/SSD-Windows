using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Shadowsocks.Controller {
    public partial class UpdateChecker {
        public static bool UnderLowerLimit() {
            Logging.Debug("Checking low limit...");
            var webCheck = new WebClient();
            webCheck.Headers.Add("User-Agent", UserAgent);
            try {
                var buffer = webCheck.DownloadData(UPDATE_URL_SSD);
                var text = Encoding.GetEncoding("UTF-8").GetString(buffer);
                var match = Regex.Match(text, @"(?<=Limit:\s)\d+\.\d+\.\d+");
                if(match.Success) {
                    var textVersion = match.Value;
                    var versionCurrent = new Version(VERSION_SSD);
                    var versionWeb = new Version(textVersion);
                    if(versionCurrent < versionWeb) {
                        MessageBox.Show(I18N.GetString("Current SSD version is too old"));
                        //检查更新但不退出
                        //Process.Start(RELEASE_URL_SSD);                        
                        //return true;
                    }
                }
            }
            catch(Exception) {
            }
            return false;
        }
    }
}
