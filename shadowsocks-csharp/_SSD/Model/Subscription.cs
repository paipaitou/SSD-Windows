using Newtonsoft.Json;
using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Shadowsocks.Model {
    public class Subscription {
        public string airport;
        public string encryption;
        public DateTime expiry;
        public string password;
        public int port;
        public List<Server> servers;
        public double traffic_total = -1.0;
        public double traffic_used = -1.0;
        public string url;

        [JsonIgnore()]
        public Configuration configuration;

        public Subscription() {
            servers = new List<Server>();
        }

        public string DescribeExpiry() {
            if (expiry == DateTime.MinValue) {
                return "????-??-?? " + string.Format(I18N.GetString("{0}d"), "?");
            }
            else if (expiry == DateTime.MaxValue) {
                return string.Format(I18N.GetString("{0}d"), "+∞");
            }
            else {
                return expiry.ToString("yyyy-MM-dd") + " " + string.Format(I18N.GetString("{0}d"), (expiry - DateTime.Now).Days);
            }
        }

        public string DescribeTraffic() {
            return
            (traffic_used == -1.0 ? "?" : traffic_used.ToString("0.00")) +
            "/" +
            (traffic_total == -1.0 ? "?" : traffic_total.ToString("0.00")) +
            " G";
            //ToString会自动四舍五入
        }

        public void HandleServers() {
            foreach (var server in servers) {
                server.subscription = this;
                if (server.method == null) {
                    server.method = encryption;
                }
                if (server.server_port == -1) {
                    server.server_port = port;
                }
                if (server.password == null) {
                    server.password = password;
                }
            }
        }

        public string NamePrefix() {
            string expiry_description;
            if (expiry == DateTime.MinValue) {
                expiry_description = string.Format(I18N.GetString("{0}d"), "?");
            }
            else if (expiry == DateTime.MaxValue) {
                expiry_description = string.Format(I18N.GetString("{0}d"), "+∞");
            }
            else {
                expiry_description = string.Format(I18N.GetString("{0}d"), (expiry - DateTime.Now).Days);
            }
            return "[" + DescribeTraffic() + " " + expiry_description + "]";
        }

        public Subscription ParseURL() {
            var new_subscription = configuration.ParseSubscriptionURL(url);
            if (new_subscription == null) {
                return null;
            }
            port = new_subscription.port;
            encryption = new_subscription.encryption;
            password = new_subscription.password;
            servers = new_subscription.servers;
            HandleServers();
            return this;
        }
    }
}
