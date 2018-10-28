using Newtonsoft.Json;
using System.Collections.Generic;

namespace Shadowsocks.Model {
    public partial class Configuration {
        public List<Subscription> subscriptions = new List<Subscription>();
       
        [JsonIgnore]
        public System.Timers.Timer Timer_detectRunning;
        
        private static void _LoadSubscription(Configuration configHandled) {
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
