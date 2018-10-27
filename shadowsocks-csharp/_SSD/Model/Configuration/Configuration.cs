using Newtonsoft.Json;
using Shadowsocks.View;
using System.Collections.Generic;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();
       
        [JsonIgnore()]
        public System.Timers.Timer Timer_detectRunning;

        [JsonIgnore()]
        public System.Timers.Timer Timer_regularUpdate;

        private static void _LoadSubscriptionAndPlugin(Configuration configHandled) {
            //Subscription
            var configSubscriptions = configHandled.subscriptions;
            if(configSubscriptions == null) {
                configSubscriptions = new List<Subscription>();
            }
            configHandled.ArrangeConfig();
        }

        private static void _ArrangeBeforeSave(Configuration config) {
            config.ArrangeConfig();
        }
    }
}
