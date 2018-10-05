using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shadowsocks.Controller;
using Shadowsocks.Util;
using Shadowsocks.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();
        public bool use_proxy = false;

        [JsonIgnore()]
        public MenuViewController MenuView;

        [JsonIgnore()]
        public System.Timers.Timer Timer_detectRunning;

        [JsonIgnore()]
        public System.Timers.Timer Timer_regularUpdate;

        private static void LoadSubscription(Configuration configSubscription) {
            var configSubscriptions = configSubscription.subscriptions;
            if(configSubscriptions == null) {
                configSubscriptions = new List<Subscription>();
            }
            configSubscription.ArrangeConfig();
        }
    }
}
