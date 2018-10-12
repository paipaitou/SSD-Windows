using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Shadowsocks.View {
    public partial class SubscriptionManagementForm :Form {
        private string TextAuto = I18N.GetString("(Auto)");
        private Configuration ConfigurationCopy;
        private ShadowsocksController Controller;
        private bool RefreshSwitch = true;

        public SubscriptionManagementForm(ShadowsocksController controllerSet) {
            InitializeComponent();
            Controller = controllerSet;
            ConfigurationCopy = Controller.GetConfigurationCopy();

            Text = I18N.GetString("Subscription Management");
            Label_url.Text = I18N.GetString("Subscription URL");
            Label_name.Text = I18N.GetString("Airport Name");
            Button_add.Text = I18N.GetString("&Add");
            Button_save.Text = I18N.GetString("&Save");
            Button_delete.Text = I18N.GetString("&Delete");
            CheckBox_use_proxy.Text = I18N.GetString("Use Proxy");
            Label_traffic_used.Text = I18N.GetString("Traffic Used:");
            Label_expiry_date.Text = I18N.GetString("Expiry Date:");
            Label_proxy_tips.Text = I18N.GetString("* Proxy will always be used in Global Mode");
            CheckBox_use_proxy.Checked = ConfigurationCopy.use_proxy;
            ResetShowed();

            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
        }

        private void AddSubscription(object sender, EventArgs e) {
            EnableSwitch();

            foreach(var subscription in ConfigurationCopy.subscriptions) {
                if(subscription.url == TextBox_url.Text) {
                    MessageBox.Show(I18N.GetString("This airport is exist"));
                    EnableSwitch();
                    return;
                }
            }

            var newSubscription = ConfigurationCopy.ParseSubscriptionURL(TextBox_url.Text);
            if(newSubscription == null) {
                MessageBox.Show(I18N.GetString("Subscribe Fail"));
                EnableSwitch();
                return;
            }
            if(TextBox_name.Text != TextAuto) {
                newSubscription.airport = TextBox_name.Text;
            }
            ListBox_subscription.Items.Add(newSubscription.airport);
            ResetShowed();
            EnableSwitch();
        }

        private void DeleteSubscription(object sender, EventArgs e) {
            if(ConfigurationCopy.configs.Count == 0 &&
                ConfigurationCopy.subscriptions.Count == 1) {
                MessageBox.Show(I18N.GetString("Please add at least one server"));
                return;
            }
            var deleteIndex = ListBox_subscription.SelectedIndex;
            if(deleteIndex <= ConfigurationCopy.subscriptions.Count - 1) {
                ConfigurationCopy.RemoveSubscriptionAt(deleteIndex);
                ListBox_subscription.Items.RemoveAt(deleteIndex);
            }
            ResetShowed();
        }

        private void LoadSubscriptionManage(object sender, EventArgs e) {
            SetNameAuto();
            foreach(var subscription in ConfigurationCopy.subscriptions) {
                ListBox_subscription.Items.Add(subscription.airport);
            }
        }

        private void ManagementClosed(object sender, FormClosedEventArgs e) {
            ConfigurationCopy.ArrangeConfig();

            var realConfig=Controller.GetCurrentConfiguration();

            var lastServer=realConfig.configs[realConfig.index];
            var lastSubscriptionUrl="";
            var lastId=-1;
            if(realConfig.strategy == null) {
                lastSubscriptionUrl = lastServer.subscription_url;
                lastId = lastServer.id;
            }

            realConfig.subscriptions = ConfigurationCopy.subscriptions;
            realConfig.configs = ConfigurationCopy.configs;
            realConfig.use_proxy = ConfigurationCopy.use_proxy;

            if(lastSubscriptionUrl != "") {
                var newIndex = realConfig.configs.FindIndex(it => it.subscription_url == lastSubscriptionUrl && it.id == lastId);
                if(newIndex < 0) {
                    newIndex = 0;
                }
                if(realConfig.index != newIndex) {
                    Controller.SelectServerIndex(newIndex);
                }
            }
        }

        private void NameEntered(object sender, EventArgs e) {
            if(TextBox_name.Text.Trim() == TextAuto) {
                TextBox_name.Text = "";
                TextBox_name.ForeColor = SystemColors.WindowText;
            }
        }

        private void NameLeaved(object sender, EventArgs e) {
            if(TextBox_name.Text.Trim() == "") {
                SetNameAuto();
            }
        }

        private void SaveSubscription(object sender, EventArgs e) {
            EnableSwitch();
            var saveSubscription = ConfigurationCopy.subscriptions[ListBox_subscription.SelectedIndex];
            saveSubscription.url = TextBox_url.Text;
            var newSubscription=saveSubscription.ParseURL();
            if(newSubscription == null) {
                MessageBox.Show(I18N.GetString("Subscribe Fail"));
                RefreshSubscriptionAndSwitch();
                return;
            }
            if(TextBox_name.Text != TextAuto) {
                newSubscription.airport = TextBox_name.Text;
            }
            ConfigurationCopy.subscriptions[ListBox_subscription.SelectedIndex] = newSubscription;
            RefreshSubscriptionAndSwitch();
        }

        private void SubscriptionSelected(object sender, EventArgs e) {
            var index = ListBox_subscription.SelectedIndex;
            if(index == -1) {
                return;
            }
            var subscription = ConfigurationCopy.subscriptions[index];
            if(subscription.url != "") {
                TextBox_url.Text = subscription.url;
            }
            else {
                TextBox_url.Text = "(error)";
            }
            TextBox_name.Text = subscription.airport;
            TextBox_name.ForeColor = SystemColors.WindowText;
            Label_traffic.Text = subscription.DescribeTraffic();
            Label_expiry.Text = subscription.DescribeExpiry();
            CheckSelected();
        }

        private void UseProxyChanged(object sender, EventArgs e) {
            ConfigurationCopy.use_proxy = CheckBox_use_proxy.Checked;
        }
    }
}
