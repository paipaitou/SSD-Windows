using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Shadowsocks.Model {
    public partial class Server {
        public int id;
        public double ratio;
        public Subscription subscription = null;

        [JsonIgnore()]
        public int latency = LATENCY_PENDING;

        public const int LATENCY_ERROR = -2;
        public const int LATENCY_PENDING = -1;
        public const int LATENCY_TESTING = 0;

        public const int PREFIX_LATENCY = 0;
        public const int PREFIX_AIRPORT = 1;


        //region SSD

        private void InitServer() {
            server = "www.baidu.com";
            server_port = -1;
            method = null;
            password = null;
        }

        //endregion

        public string NamePrefix(int PREFIX_FLAG) {
            string prefix = "[";
            if (PREFIX_FLAG == PREFIX_LATENCY) {

                if (latency == LATENCY_TESTING) {
                    prefix += "testing";
                }
                else if (latency == LATENCY_ERROR) {
                    prefix += "error";
                }
                else if (latency == LATENCY_PENDING) {
                    prefix += "pending";
                }
                else {
                    prefix += latency.ToString() + "ms";
                }
            }
            else if (PREFIX_FLAG == PREFIX_AIRPORT) {
                prefix += subscription.airport;
            }

            if (subscription == null) {
                prefix += "]";
            }
            else {
                prefix += " " + ratio + "x]";
            }
            return prefix;
        }

        public void TcpingLatency() {
            var latencies = new List<double>();
            var sock = new TcpClient();
            var stopwatch = new Stopwatch();
            try {
                Dns.GetHostAddresses(server);
                stopwatch.Start();
                var result = sock.BeginConnect(server, server_port, null, null);
                if (result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2))) {
                    stopwatch.Stop();
                    latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
                }
                else {
                    stopwatch.Stop();
                }
                sock.Close();
            }
            catch (Exception) {
                latency = LATENCY_ERROR;
                return;
            }


            if (latencies.Count != 0) {
                latency = (int)latencies.Average();
            }
            else {
                latency = LATENCY_ERROR;
            }
        }
    }
}
