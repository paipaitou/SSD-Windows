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

            if(subscription_url == "") {
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
            var stopwatch = new Stopwatch();
            for(var testTime = 0; testTime <= 1; testTime++) {
                try {
                    var socket = new TcpClient();
                    var ip=Dns.GetHostAddresses(server);
                    stopwatch.Start();
                    var result = socket.BeginConnect(ip[0], server_port, null, null);
                    if(result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2))) {
                        stopwatch.Stop();
                        latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
                    }
                    else {
                        stopwatch.Stop();
                    }
                    socket.Close();
                }
                catch(Exception) {

                }
                stopwatch.Reset();
            }

            if(latencies.Count != 0) {
                Latency = ( int ) latencies.Average();
            }
            else {
                Latency = LATENCY_ERROR;
            }
        }
    }
}