using Shadowsocks.Controller;
using Shadowsocks.Util;
using Shadowsocks.View;

namespace Shadowsocks.Model {
    public partial class Configuration {
        private void RegularDetectRunning(object sender, System.Timers.ElapsedEventArgs e) {
            Timer_detectRunning.Interval = 1000.0 * 60 * 60;
            //if(UpdateChecker.UnderLowerLimit() || Utils.DetectVirus()) {
            if (UpdateChecker.UnderLowerLimit()) {
                MenuViewController.StaticMenuView.Quit();
            }
        }
    }
}
