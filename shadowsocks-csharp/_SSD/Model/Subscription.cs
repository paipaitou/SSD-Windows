using Newtonsoft.Json;
using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Shadowsocks.Model {
    public class Subscription {
        public string url;
        public string airport;
        public int port;
        public string encryption;
        public string password;
        public List<Server> servers;
        public int traffic_used = -1;
        public int traffic_total = -1;
        public DateTime expiry;

        [JsonIgnore()]
        public Configuration configuration;

        public Subscription() {
            servers = new List<Server>();
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

        public string DescribeTraffic() {
            return
            (traffic_used == -1 ? "?" : traffic_used.ToString()) +
            "/" +
            (traffic_total == -1 ? "?" : traffic_total.ToString()) +
            " G";
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

        public string DescribeExpiry() {
            string expiry_description;
            if (expiry == DateTime.MinValue) {
                expiry_description="????-??-?? "+ string.Format(I18N.GetString("{0}d"), "?");
            }else if (expiry == DateTime.MinValue) {
                expiry_description = string.Format(I18N.GetString("{0}d"), "+∞");
            }
            else {
                expiry_description = expiry.ToString("yyyy-MM-dd")+" "+ string.Format(I18N.GetString("{0}d"), (expiry - DateTime.Now).Days);
            }
            return expiry_description;
        }

        public Subscription ParseURL() {
            var new_subscription = configuration.PareseSubscriptionURL(url);
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
