using Newtonsoft.Json;
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

        public Subscription ParseURL(bool proxy=false) {
            var new_subscription= configuration.PareseSubscriptionURL(url, proxy, false);
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
