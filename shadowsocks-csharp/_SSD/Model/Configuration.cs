using Newtonsoft.Json;
using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();

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

        public Server CurrentServerEx() {
            if (index >= 0 && index < configs.Count) {
                return configs[index];
            }
            else if (index >= configs.Count) {
                var real_index = index - configs.Count;
                foreach (var subscription in subscriptions) {
                    if (subscription.servers.Count >= real_index - 1) {
                        return subscription.servers[real_index];
                    }
                    real_index -= subscription.servers.Count;
                }
            }
            return GetDefaultServer();
        }

        public void UpdateAllSubscription(NotifyIcon notifyIcon = null, bool proxy = false) {
            if (subscriptions.Count == 0 && notifyIcon != null) {
                notifyIcon.ShowBalloonTip(
                    1000,
                    I18N.GetString("Subscribe Fail"),
                    I18N.GetString("No Subscription"),
                    ToolTipIcon.Error
                );
            }
            for (var index = 0; index <= subscriptions.Count - 1; index++) {
                if (subscriptions[index].ParseURL(proxy) != null) {
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

        public Subscription PareseSubscriptionURL(string url, bool proxy = false, bool merge = true) {
            var web_subscribe = new WebClient();
            if (proxy) {
                web_subscribe.Proxy = new WebProxy(IPAddress.Loopback.ToString(), localPort);
            }
            try {
                var buffer = web_subscribe.DownloadData(url);
                var text = Encoding.GetEncoding("UTF-8").GetString(buffer);
                var new_subscription = ParseBase64(text, merge);
                new_subscription.url = url;
                return new_subscription;
            }
            catch (Exception) {
                return null;
            }
        }

        public Subscription ParseBase64(string text_base64, bool merge = true) {
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
            if (!merge) {
                new_subscription.HandleServers();
                return new_subscription;
            }
            foreach (var subscription in subscriptions) {
                if (subscription.airport == new_subscription.airport) {
                    subscription.encryption = new_subscription.encryption;
                    subscription.password = new_subscription.password;
                    subscription.port = new_subscription.port;
                    subscription.servers = new_subscription.servers;
                    subscription.HandleServers();
                    return subscription;
                }
            }
            //未找到同名订阅，被迫创建
            new_subscription.HandleServers();
            subscriptions.Add(new_subscription);
            return new_subscription;
        }
    }
}
