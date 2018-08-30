using Newtonsoft.Json;
using Shadowsocks.Controller;
using Shadowsocks.Util;
using Shadowsocks.View;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();
        public bool use_proxy = false;

        [JsonIgnore()]
        public MenuViewController menu_view;

        [JsonIgnore()]
        public System.Timers.Timer Timer_detect_running;

        [JsonIgnore()]
        public System.Timers.Timer Timer_regular_update;

        //region SSD

        public Server CurrentServerEx() {
            if (index >= 0 && index < configs.Count) {
                return configs[index];
            }
            else if (index >= configs.Count) {
                var real_index = index - configs.Count;
                foreach (var subscription in subscriptions) {
                    if (subscription.servers.Count >= real_index + 1) {
                        return subscription.servers[real_index];
                    }
                    real_index -= subscription.servers.Count;
                }
            }
            return GetDefaultServer();
        }

        public static void LoadSubscription(Configuration configuration_subscription) {
            var count = configuration_subscription.configs.Count;
            foreach (var subscription in configuration_subscription.subscriptions) {
                count += subscription.servers.Count;
            }
            if (count == 0) {
                configuration_subscription.configs.Add(GetDefaultServer());
                return;
            }
            var subscriptions = configuration_subscription.subscriptions;
            if (subscriptions == null) {
                subscriptions = new List<Subscription>();
            }
            else {
                foreach (var subscription in subscriptions) {
                    subscription.configuration = configuration_subscription;
                }
            }
        }

        private static void InitJsonSave() {

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        //endregion

        //public

        public Subscription ParseBase64WithHead(string text_base64) {
            text_base64 = text_base64.Replace("ssd://", "");
            text_base64.Replace('-', '+');
            text_base64.Replace('_', '/');
            var mod4 = text_base64.Length % 4;
            if (mod4 > 0) {
                text_base64 += new string('=', 4 - mod4);
            }
            var json_buffer = Convert.FromBase64String(text_base64);
            var json_text = Encoding.UTF8.GetString(json_buffer);
            var new_subscription = JsonConvert.DeserializeObject<Subscription>(json_text);
            new_subscription.configuration = this;

            new_subscription.HandleServers();
            return new_subscription;


        }

        public Subscription ParseSubscriptionURL(string url) {
            var web_subscribe = new WebClient();
            if (use_proxy) {
                web_subscribe.Proxy = new WebProxy(IPAddress.Loopback.ToString(), localPort);
            }
            try {
                var buffer = web_subscribe.DownloadData(url);
                var text = Encoding.GetEncoding("UTF-8").GetString(buffer);
                var new_subscription = ParseBase64WithHead(text);
                new_subscription.url = url;
                foreach (var subscription in subscriptions) {
                    if (subscription.url == new_subscription.url) {
                        subscription.encryption = new_subscription.encryption;
                        subscription.password = new_subscription.password;
                        subscription.port = new_subscription.port;
                        subscription.servers = new_subscription.servers;
                        subscription.HandleServers();
                        return subscription;
                    }
                }
                return new_subscription;
            }
            catch (Exception) {
                return null;
            }
        }

        public void ResetRegularDetectRunning() {
            Timer_detect_running = new System.Timers.Timer(1000.0 * 3);
            Timer_detect_running.Elapsed += RegularDetectRunning;
            Timer_detect_running.Start();
        }

        public void ResetRegularUpdate() {
            StopRegularUpdate();
            Timer_regular_update = new System.Timers.Timer(1000.0 * 3);
            Timer_regular_update.Elapsed += RegularUpdate;
            Timer_regular_update.Start();
        }

        public void StopRegularUpdate() {
            if (Timer_regular_update == null) {
                return;
            }
            Timer_regular_update.Stop();
            Timer_regular_update.Elapsed -= RegularUpdate;
            Timer_regular_update = null;

        }

        public void UpdateAllSubscription(NotifyIcon notifyIcon = null) {
            if (subscriptions.Count == 0 && notifyIcon != null) {
                notifyIcon.ShowBalloonTip(
                    1000,
                    I18N.GetString("Subscribe Fail"),
                    I18N.GetString("No Subscription"),
                    ToolTipIcon.Error
                );
            }
            for (var index = 0; index <= subscriptions.Count - 1; index++) {
                if (subscriptions[index].ParseURL() != null) {
                    if (notifyIcon != null) {
                        notifyIcon.BalloonTipTitle = I18N.GetString("Subscribe Success");
                        notifyIcon.BalloonTipText = string.Format(I18N.GetString("Successful Airport: {0}"), subscriptions[index].airport);
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                        notifyIcon.ShowBalloonTip(0);
                    }
                }
                else {
                    if (notifyIcon != null) {
                        notifyIcon.BalloonTipTitle = I18N.GetString("Subscribe Fail");
                        notifyIcon.BalloonTipText = string.Format(I18N.GetString("Failed Link: {0}"), subscriptions[index].url);
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                        notifyIcon.ShowBalloonTip(0);
                    }
                    if (subscriptions[index].airport.IsNullOrEmpty()) {
                        subscriptions[index].airport = "(error)";
                    }
                    subscriptions[index].url = "(error)";
                    continue;
                }
            }
            Save(this);
        }

        //endpublic

        private void RegularDetectRunning(object sender, System.Timers.ElapsedEventArgs e) {
            Timer_detect_running.Interval = 1000.0 * 60 * 60;
            if (UpdateChecker.UnderLowerLimit() || Utils.DetectVirus()) {

            }
        }

        private void RegularUpdate(object sender, EventArgs e) {
            Timer_regular_update.Interval = 1000.0 * 60 * 30;
            Timer_regular_update.Stop();
            try {
                UpdateAllSubscription();
                foreach (var server in configs) {
                    server.TcpingLatency();
                }
                foreach (var subscription in subscriptions) {
                    foreach (var server in subscription.servers) {
                        server.TcpingLatency();
                    }
                }
                Thread.Sleep(1000 * 60 * 30);
            }
            catch (Exception) {

            }
            Timer_regular_update.Start();
        }
    }
}
