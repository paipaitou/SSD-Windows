using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Shadowsocks.Model {
    public partial class Server {
        public string NamePrefix(Configuration config, int PREFIX_FLAG) {
            string prefix = "[";
            if(PREFIX_FLAG == PREFIX_LATENCY) {
                if(Latency == LATENCY_TESTING) {
                    prefix += I18N.GetString("Testing");
                }
                else if(Latency == LATENCY_ERROR) {
                    prefix += I18N.GetString("Error");
                }
                else if(Latency == LATENCY_PENDING) {
                    prefix += I18N.GetString("Pending");
                }
                else {
                    prefix += Latency.ToString() + "ms";
                }
            }
            else if(PREFIX_FLAG == PREFIX_AIRPORT) {
                foreach(var subscription in config.subscriptions) {
                    if(subscription.url == subscription_url) {
                        prefix += subscription.airport;
                        break;
                    }
                }
            }

            if(subscription_url == null) {
                prefix += "]";
            }
            else {
                prefix += " " + ratio + "x]";
            }
            return prefix;
        }

        public void SetSubscription(Subscription subscriptionSet) {
            Subscription = subscriptionSet;
            subscription_url = subscriptionSet.url;
        }

        public void TcpingLatency() {
            Latency = LATENCY_TESTING;
            var latencies = new List<double>();
            var sock = new TcpClient();
            var stopwatch = new Stopwatch();
            try {
                var ip=Dns.GetHostAddresses(server);
                stopwatch.Start();
                var result = sock.BeginConnect(ip[0], server_port, null, null);
                if(result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2))) {
                    stopwatch.Stop();
                    latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
                }
                else {
                    stopwatch.Stop();
                }
                sock.Close();
            }
            catch(Exception) {
                Latency = LATENCY_ERROR;
                return;
            }

            if(latencies.Count != 0) {
                Latency = (int)latencies.Average();
            }
            else {
                Latency = LATENCY_ERROR;
            }
        }
    }
}