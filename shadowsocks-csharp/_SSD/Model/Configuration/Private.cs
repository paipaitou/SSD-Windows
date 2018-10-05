using Shadowsocks.Controller;
using Shadowsocks.Util;
using System;
using System.Threading;

namespace Shadowsocks.Model {
    public partial class Configuration {
        private void RegularDetectRunning(object sender, System.Timers.ElapsedEventArgs e) {
            Timer_detectRunning.Interval = 1000.0 * 60 * 60;
            if(UpdateChecker.UnderLowerLimit() || Utils.DetectVirus()) {
                MenuView.Quit();
            }
        }

        private void RegularUpdate(object sender, EventArgs e) {
            Timer_regularUpdate.Interval = 1000.0 * 60 * 30;
            Timer_regularUpdate.Stop();
            try {
                //UpdateAllSubscription();
                //todo 暂时关闭自动更新
                foreach(var server in configs) {
                    server.TcpingLatency();
                }
                Thread.Sleep(1000 * 60 * 30);
            }
            catch(Exception) {

            }
            Timer_regularUpdate.Start();
        }
    }
}
