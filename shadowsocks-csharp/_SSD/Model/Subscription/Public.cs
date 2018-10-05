using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model {
    public partial class Subscription {
        public string DescribeExpiry() {
            if(expiry == DateTime.MinValue) {
                return "????-??-?? " + string.Format(I18N.GetString("{0}d"), "?");
            }
            else if(expiry == DateTime.MaxValue) {
                return string.Format(I18N.GetString("{0}d"), "+∞");
            }

            else {
                var day = string.Format(I18N.GetString("{0}d"), 0);
                if(expiry > DateTime.Now) {
                    day = string.Format(I18N.GetString("{0}d"), (expiry - DateTime.Now).Days);
                }
                return expiry.ToString("yyyy-MM-dd") + " " + day;
            }
        }

        public string DescribeTraffic() {
            return
            (traffic_used == -1.0 ? "?" : traffic_used.ToString("0.00")) +
            "/" +
            (traffic_total == -1.0 ? "?" : traffic_total.ToString("0.00")) +
            " G";
            //ToString会自动四舍五入
        }

        public List<Server> GetServers() {
            var servers=new List<Server>();
            foreach(var config in configuration.configs) {
                if(config.subscription_url == null) {
                    continue;
                }
                if(config.subscription_url == url) {
                    servers.Add(config);
                }
            }
            return servers;
        }

        public string NamePrefix() {
            string expiryDescription;
            if(expiry == DateTime.MinValue) {
                expiryDescription = string.Format(I18N.GetString("{0}d"), "?");
            }
            else if(expiry == DateTime.MaxValue) {
                expiryDescription = string.Format(I18N.GetString("{0}d"), "+∞");
            }
            else if(expiry < DateTime.Now) {
                expiryDescription = string.Format(I18N.GetString("{0}d"), 0);
            }
            else {
                expiryDescription = string.Format(I18N.GetString("{0}d"), (expiry - DateTime.Now).Days);
            }
            return "[" + DescribeTraffic() + " " + expiryDescription + "]";
        }

        public Subscription ParseURL() {
            var newSubscription = configuration.ParseSubscriptionURL(url);
            if(newSubscription == null) {
                return null;
            }
            return this;
        }
    }
}
