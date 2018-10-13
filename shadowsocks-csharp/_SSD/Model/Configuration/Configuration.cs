using Newtonsoft.Json;
using Shadowsocks.View;
using System.Collections.Generic;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();

        [JsonIgnore()]
        public MenuViewController MenuView;

        [JsonIgnore()]
        public System.Timers.Timer Timer_detectRunning;

        [JsonIgnore()]
        public System.Timers.Timer Timer_regularUpdate;

        private static void _LoadSubscription(Configuration configSubscription) {
            var configSubscriptions = configSubscription.subscriptions;
            if(configSubscriptions == null) {
                configSubscriptions = new List<Subscription>();
            }
            configSubscription.ArrangeConfig();
        }

        private static void _ArrangeBeforeSave(Configuration config) {
            config.ArrangeConfig();
        }
    }
}
