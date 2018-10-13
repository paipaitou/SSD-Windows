using Newtonsoft.Json.Linq;
using Shadowsocks.Controller;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public void ArrangeConfig() {
            subscriptions = subscriptions.Distinct(new Subscription()).ToList();
            foreach(var subscription in subscriptions) {
                subscription.configuration = this;
                var count=0;
                foreach(var server in configs) {
                    if(server.subscription_url == subscription.url) {
                        server.SetSubscription(subscription);
                        count++;
                    }
                }
                if(count == 0) {
                    subscription.url = "";
                }
                count = 0;
            }
            subscriptions.RemoveAll(it => it.url == "");
            configs.RemoveAll(it => it.subscription_url != "" && it.Subscription == null);
            subscriptions.Sort();
            configs.Sort();
        }

        public Subscription FindSubscription(string bindUrl, bool createNew) {
            if(bindUrl == null) {
                if(createNew == false) {
                    return null;
                }
                var newSubscription= new Subscription();
                newSubscription.url = bindUrl;
                subscriptions.Add(newSubscription);
                return newSubscription;
            }

            foreach(var subscription in subscriptions) {
                if(subscription.url == bindUrl) {
                    return subscription;
                }
            }

            if(createNew == false) {
                return null;
            }
            var newSubscription2= new Subscription();
            newSubscription2.url = bindUrl;
            subscriptions.Add(newSubscription2);
            return newSubscription2;
        }

        public Subscription ParseBase64WithHead(string textBase64, string bindUrl = null) {
            textBase64 = textBase64.Replace("ssd://", "")
                .Replace('-', '+')
                .Replace('_', '/');
            var mod4 = textBase64.Length % 4;
            if(mod4 > 0) {
                textBase64 += new string('=', 4 - mod4);
            }
            var jsonBuffer = Convert.FromBase64String(textBase64);
            var jsonText = Encoding.UTF8.GetString(jsonBuffer);
            var jsonObject=JObject.Parse(jsonText);
            var subscriptionUrl=jsonObject.GetValue("url");
            var newSubscription= FindSubscription(subscriptionUrl == null ? bindUrl : subscriptionUrl.ToString(),true);
            newSubscription.airport = jsonObject["airport"].ToString();
            newSubscription.port = jsonObject["port"].ToObject<int>();
            newSubscription.encryption = jsonObject["encryption"].ToString();
            newSubscription.password = jsonObject["password"].ToString();

            var subscriptionTrafficUsed=jsonObject.GetValue("traffic_used");
            if(subscriptionTrafficUsed != null) {
                newSubscription.traffic_used = subscriptionTrafficUsed.ToObject<double>();
            }

            var subscriptionTrafficTotal=jsonObject.GetValue("traffic_total");
            if(subscriptionTrafficTotal != null) {
                newSubscription.traffic_total = subscriptionTrafficTotal.ToObject<double>();
            }

            var subscriptionExpiry=jsonObject.GetValue("expiry");
            if(subscriptionExpiry != null) {
                newSubscription.expiry = subscriptionExpiry.ToObject<DateTime>();
            }

            var subscriptionPluginOptions=jsonObject.GetValue("plugin_options");
            if(subscriptionPluginOptions != null) {
                newSubscription.plugin_options = subscriptionPluginOptions.ToString();
            }

            var subscriptionPluginArguments=jsonObject.GetValue("plugin_arguments");
            if(subscriptionPluginArguments != null) {
                newSubscription.plugin_arguments = subscriptionPluginArguments.ToString();
            }

            var subscriptionPlugin=jsonObject.GetValue("plugin");
            if(subscriptionPlugin != null) {
                newSubscription.plugin = subscriptionPlugin.ToString();
            }

            configs.RemoveAll(it => it.subscription_url == newSubscription.url);
            var subscriptionServers=JArray.Parse(jsonObject["servers"].ToString());
            foreach(var subscriptionServer in subscriptionServers) {
                var newServer=new Server();
                newServer.SetSubscription(newSubscription);
                var jsonServerObject=JObject.Parse(subscriptionServer.ToString());
                newServer.id = jsonServerObject["id"].ToObject<int>();
                newServer.server = jsonServerObject["server"].ToString();

                var serverPort=jsonServerObject.GetValue("port");
                if(serverPort != null) {
                    newServer.server_port = serverPort.ToObject<int>();
                }
                else {
                    newServer.server_port = newSubscription.port;
                }

                var serverEncryption=jsonServerObject.GetValue("encryption");
                if(serverEncryption != null) {
                    newServer.method = serverEncryption.ToString();
                }
                else {
                    newServer.method = newSubscription.encryption;
                }

                var serverPassword=jsonServerObject.GetValue("password");
                if(serverPassword != null) {
                    newServer.password = serverPassword.ToString();
                }
                else {
                    newServer.password = newSubscription.password;
                }

                var serverPluginOptions=jsonServerObject.GetValue("plugin_options");
                if(serverPluginOptions != null) {
                    newServer.plugin_opts = serverPluginOptions.ToString();
                }
                else {
                    newServer.plugin_opts = newSubscription.plugin_options;
                }

                var serverPluginArguments=jsonServerObject.GetValue("plugin_arguments");
                if(serverPluginArguments != null) {
                    newServer.plugin_args = serverPluginArguments.ToString();
                }
                else {
                    newServer.plugin_args = newSubscription.plugin_arguments;
                }

                var serverPlugin=jsonServerObject.GetValue("plugin");
                if(serverPlugin != null) {
                    newServer.plugin = serverPlugin.ToString();
                }
                else {
                    newServer.plugin = newSubscription.plugin;
                }

                var serverRatio=jsonServerObject.GetValue("ratio");
                if(serverRatio != null) {
                    newServer.ratio = serverRatio.ToObject<double>();
                }

                var serverRemarks=jsonServerObject.GetValue("remarks");
                if(serverRemarks != null) {
                    newServer.remarks = serverRemarks.ToString();
                }

                configs.Add(newServer);
            }
            newSubscription.configuration = this;
            return newSubscription;
        }

        public Subscription ParseSubscriptionURL(string url, bool useProxy = false) {
            var webSubscribe = new WebClient();
            var foundSubscription=FindSubscription(url, false);
            //优先级：foundSubscription > useProxy参数
            if(foundSubscription != null) {
                useProxy = foundSubscription.use_proxy;
            }
            if(useProxy) {
                webSubscribe.Proxy = new WebProxy(IPAddress.Loopback.ToString(), localPort);
            }
            try {
                var buffer = webSubscribe.DownloadData(url);
                var text = Encoding.GetEncoding("UTF-8").GetString(buffer);
                var newSubscription = ParseBase64WithHead(text,url);
                if(useProxy) {
                    newSubscription.use_proxy = true;
                }
                return newSubscription;
            }
            catch(Exception) {
                return null;
            }
        }

        public void RemoveSubscriptionAt(int subscriptionIndex) {
            configs.RemoveAll(it => it.subscription_url == subscriptions[subscriptionIndex].url);
            subscriptions.RemoveAt(subscriptionIndex);
        }

        public void ResetRegularDetectRunning() {
            Timer_detectRunning = new System.Timers.Timer(1000.0 * 3);
            Timer_detectRunning.Elapsed += RegularDetectRunning;
            Timer_detectRunning.Start();
        }

        public void ResetRegularUpdate() {
            StopRegularUpdate();
            Timer_regularUpdate = new System.Timers.Timer(1000.0 * 3);
            Timer_regularUpdate.Elapsed += RegularUpdate;
            Timer_regularUpdate.Start();
        }

        public void StopRegularUpdate() {
            if(Timer_regularUpdate == null) {
                return;
            }
            Timer_regularUpdate.Stop();
            Timer_regularUpdate.Elapsed -= RegularUpdate;
            Timer_regularUpdate = null;

        }

        public void UpdateAllSubscription(NotifyIcon notifyIcon = null, ShadowsocksController controller = null) {
            if(subscriptions.Count == 0 && notifyIcon != null) {
                notifyIcon.ShowBalloonTip(
                    1000,
                    I18N.GetString("Subscribe Fail"),
                    I18N.GetString("No Subscription"),
                    ToolTipIcon.Error
                );
            }

            var lastSubscriptionUrl="";
            var lastId=-1;
            if(strategy == null) {
                lastSubscriptionUrl = configs[index].subscription_url;
                lastId = configs[index].id;
            }

            foreach(var subscription in subscriptions) {
                if(subscription.ParseURL() != null) {
                    if(notifyIcon != null) {
                        notifyIcon.BalloonTipTitle = I18N.GetString("Subscribe Success");
                        notifyIcon.BalloonTipText = string.Format(I18N.GetString("Successful Airport: {0}"), subscription.airport);
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                        notifyIcon.ShowBalloonTip(0);
                    }
                }
                else {
                    if(notifyIcon != null) {
                        notifyIcon.BalloonTipTitle = I18N.GetString("Subscribe Fail");
                        notifyIcon.BalloonTipText = string.Format(I18N.GetString("Failed Link: {0}"), subscription.url);
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                        notifyIcon.ShowBalloonTip(0);
                    }
                    subscription.url = "";
                    continue;
                }
            }
            if(lastSubscriptionUrl != "") {
                var newIndex = configs.FindIndex(it => it.subscription_url == lastSubscriptionUrl && it.id == lastId);
                if(newIndex < 0) {
                    newIndex = 0;
                }
                if(controller != null && index != newIndex) {
                    controller.SelectServerIndex(newIndex);
                }
            }
            Save(this);
        }
    }
}
