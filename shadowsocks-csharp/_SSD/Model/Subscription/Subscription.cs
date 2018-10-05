using Newtonsoft.Json;
using System;

namespace Shadowsocks.Model {
    public partial class Subscription {
        public string airport;
        public string encryption;
        public string password;
        public int port;

        public DateTime expiry;
        public double traffic_used = -1.0;
        public double traffic_total = -1.0;
        public string url;

        [JsonIgnore]
        public Configuration configuration;
    }
}
