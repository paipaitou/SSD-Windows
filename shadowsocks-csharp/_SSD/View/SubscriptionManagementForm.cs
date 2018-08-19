using Shadowsocks.Controller;
using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Shadowsocks.Properties;

namespace Shadowsocks.View {
    public partial class SubscriptionManagementForm : Form {
        private string text_auto = I18N.GetString("(Auto)");

        private ShadowsocksController controller;
        private List<Subscription> subscriptions;

        private bool refresh_switch = true;

        public SubscriptionManagementForm(ShadowsocksController controller_set) {
            InitializeComponent();
            controller = controller_set;
            subscriptions = controller.GetCurrentConfiguration().subscriptions;

            Text = I18N.GetString("Subscription Management");
            Label_url.Text = I18N.GetString("Subscription URL");
            Label_name.Text = I18N.GetString("Airport Name");
            Button_add.Text = I18N.GetString("&Add");
            Button_save.Text = I18N.GetString("&Save");
            Button_delete.Text = I18N.GetString("&Delete");

            Icon = Icon.FromHandle(Resources.ssw128.GetHicon());
        }

        private void NameEntered(object sender, EventArgs e) {
            if (TextBox_name.Text.Trim() == text_auto) {
                TextBox_name.Text = "";
                TextBox_name.ForeColor = SystemColors.WindowText;
            }
        }

        private void SetNameAuto() {
            TextBox_name.ForeColor = Color.Gray;
            TextBox_name.Text = text_auto;
        }

        private void NameLeaved(object sender, EventArgs e) {
            if (TextBox_name.Text.Trim() == "") {
                SetNameAuto();
            }
        }

        private void LoadSubscriptionManage(object sender, EventArgs e) {
            SetNameAuto();
            foreach (var subscription in subscriptions) {
                ListBox_subscription.Items.Add(subscription.airport);
            }
        }

        private void RefreshSubscriptionAndSwitch() {
            TextBox_url.Text = "";
            SetNameAuto();
            controller.GetCurrentConfiguration().UpdateAllSubscription();
            ListBox_subscription.Items.Clear();
            foreach (var subscription in subscriptions) {
                ListBox_subscription.Items.Add(subscription.airport);
            }
            EnableSwitch();
        }

        private void EnableSwitch() {
            ListBox_subscription.Enabled = !refresh_switch;
            TextBox_url.Enabled = !refresh_switch;
            TextBox_name.Enabled = !refresh_switch;
            Button_add.Enabled = !refresh_switch;
            Button_save.Enabled = false;
            Button_delete.Enabled = false;
            refresh_switch = !refresh_switch;
        }

        private void AddSubscription(object sender, EventArgs e) {
            EnableSwitch();
            var new_subscription = controller.GetCurrentConfiguration().PareseSubscriptionURL(TextBox_url.Text,false,false);
            if (new_subscription == null) {
                MessageBox.Show("Subscribe Fail");
                EnableSwitch();
                Button_save.Enabled = true;
                Button_delete.Enabled = true;
                return;
            }
            if (TextBox_name.Text != text_auto) {
                new_subscription.airport = TextBox_name.Text;
            }
            foreach (var subscription in subscriptions) {
                if (subscription.airport == new_subscription.airport) {
                    MessageBox.Show("This airport is exist");
                    EnableSwitch();
                    Button_save.Enabled = true;
                    Button_delete.Enabled = true;
                    return;
                }
            }
            subscriptions.Add(new_subscription);
            ListBox_subscription.Items.Add(new_subscription.airport);
            EnableSwitch();
            Button_save.Enabled = true;
            Button_delete.Enabled = true;
        }

        private void SaveSubscription(object sender, EventArgs e) {
            EnableSwitch();
            var new_subscription = controller.GetCurrentConfiguration().PareseSubscriptionURL(
                TextBox_url.Text,
                false,
                false
            );
            if (new_subscription == null) {
                MessageBox.Show("Subscribe Fail");
                RefreshSubscriptionAndSwitch();
                return;
            }
            if (TextBox_name.Text != text_auto) {
                new_subscription.airport = TextBox_name.Text;
            }
            subscriptions[ListBox_subscription.SelectedIndex]=new_subscription;            
            RefreshSubscriptionAndSwitch();
        }

        private void DeleteSubscription(object sender, EventArgs e) {
            var delete_index = ListBox_subscription.SelectedIndex;
            if (delete_index <= subscriptions.Count - 1) {
                subscriptions.RemoveAt(delete_index);
                ListBox_subscription.Items.RemoveAt(delete_index);
            }
        }

        private void SubscriptionSelected(object sender, EventArgs e) {
            var index = ListBox_subscription.SelectedIndex;
            if (index == -1) {
                return;
            }
            var subscription = subscriptions[index];
            TextBox_url.Text = subscription.url;
            TextBox_name.Text = subscription.airport;
            TextBox_name.ForeColor = SystemColors.WindowText;
            Button_save.Enabled = true;
            Button_delete.Enabled = true;
        }

        private void ManagementExit(object sender, FormClosedEventArgs e) {
            Configuration.Save(controller.GetCurrentConfiguration());
        }
    }
}
