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
        private Configuration configuration_copy;

        private bool refresh_switch = true;

        public SubscriptionManagementForm(ShadowsocksController controller_set) {
            InitializeComponent();
            controller = controller_set;
            configuration_copy = controller.GetConfigurationCopy();

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
            foreach (var subscription in configuration_copy.subscriptions) {
                ListBox_subscription.Items.Add(subscription.airport);
            }
        }

        private void RefreshSubscriptionAndSwitch() {
            TextBox_url.Text = "";
            SetNameAuto();
            configuration_copy.UpdateAllSubscription();
            ListBox_subscription.Items.Clear();
            foreach (var subscription in configuration_copy.subscriptions) {
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
            CheckSelected();
        }

        private void CheckSelected() {
            if (ListBox_subscription.SelectedIndex == -1) {
                Button_save.Enabled = false;
                Button_delete.Enabled = false;
            }
            else {
                Button_save.Enabled = true;
                Button_delete.Enabled = true;
            }
        }

        private void AddSubscription(object sender, EventArgs e) {
            EnableSwitch();
            var new_subscription = configuration_copy.PareseSubscriptionURL(TextBox_url.Text, false, false);
            if (new_subscription == null) {
                MessageBox.Show(I18N.GetString("Subscribe Fail"));
                EnableSwitch();
                return;
            }
            if (TextBox_name.Text != text_auto) {
                new_subscription.airport = TextBox_name.Text;
            }
            foreach (var subscription in configuration_copy.subscriptions) {
                if (subscription.airport == new_subscription.airport) {
                    MessageBox.Show(I18N.GetString("This airport is exist"));
                    EnableSwitch();
                    return;
                }
            }
            configuration_copy.subscriptions.Add(new_subscription);
            ListBox_subscription.Items.Add(new_subscription.airport);
            TextBox_url.Text = "";
            SetNameAuto();
            ListBox_subscription.SelectedIndex = -1;
            EnableSwitch();
        }


        private void SaveSubscription(object sender, EventArgs e) {
            EnableSwitch();
            var new_subscription = configuration_copy.PareseSubscriptionURL(
                TextBox_url.Text,
                false,
                false
            );
            if (new_subscription == null) {
                MessageBox.Show(I18N.GetString("Subscribe Fail"));
                RefreshSubscriptionAndSwitch();
                return;
            }
            if (TextBox_name.Text != text_auto) {
                new_subscription.airport = TextBox_name.Text;
            }
            configuration_copy.subscriptions[ListBox_subscription.SelectedIndex] = new_subscription;
            RefreshSubscriptionAndSwitch();
        }

        private void DeleteSubscription(object sender, EventArgs e) {
            if (configuration_copy.configs.Count == 0 &&
                configuration_copy.subscriptions.Count == 1) {
                MessageBox.Show(I18N.GetString("Please add at least one server"));
                return;
            }
            var delete_index = ListBox_subscription.SelectedIndex;
            if (delete_index <= configuration_copy.subscriptions.Count - 1) {
                configuration_copy.subscriptions.RemoveAt(delete_index);
                ListBox_subscription.Items.RemoveAt(delete_index);
            }
            TextBox_url.Text = "";
            SetNameAuto();
            CheckSelected();
        }

        private void SubscriptionSelected(object sender, EventArgs e) {
            var index = ListBox_subscription.SelectedIndex;
            if (index == -1) {
                return;
            }
            var subscription = configuration_copy.subscriptions[index];
            TextBox_url.Text = subscription.url;
            TextBox_name.Text = subscription.airport;
            TextBox_name.ForeColor = SystemColors.WindowText;
            CheckSelected();
        }

        private void ManagementClosed(object sender, FormClosedEventArgs e) {
            controller.GetCurrentConfiguration().subscriptions = configuration_copy.subscriptions;
        }
    }
}
