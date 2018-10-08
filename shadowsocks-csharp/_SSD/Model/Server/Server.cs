using Newtonsoft.Json;

namespace Shadowsocks.Model {
    public partial class Server {
        public int id=-1;
        public double ratio=0;
        public string subscription_url="";

        [JsonIgnore]
        public Subscription Subscription=null;

        [JsonIgnore]
        public int Latency = LATENCY_PENDING;

        public const int LATENCY_ERROR = -3;
        public const int LATENCY_PENDING = -2;
        public const int LATENCY_TESTING = -1;

        public const int PREFIX_LATENCY = 0;
        public const int PREFIX_AIRPORT = 1;

        private void _InitServer() {
            server = "www.baidu.com";
            server_port = -1;
            method = "";
        }
    }
}
