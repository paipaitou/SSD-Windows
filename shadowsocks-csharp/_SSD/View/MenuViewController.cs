using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Shadowsocks.Controller;
using Shadowsocks.Model;
using Shadowsocks.Util;

namespace Shadowsocks.View {
    public partial class MenuViewController {
        private Configuration configuration_current;

        private MenuItem MenuGroup_subscribe;
        private MenuItem MenuItem_subscribe_Manage;
        private MenuItem MenuItem_subscribe_Update;

        private SubscriptionManagementForm ManageForm;

        //region SSD
        private void DisableFirstRun() {

        }

        private void InitOther() {
            configuration_current = controller.GetCurrentConfiguration();
            configuration_current.ResetDetectRunning();
            configuration_current.ResetRegularUpdate();
            contextMenu1.Popup += PreloadMenu;
        }

        private MenuItem CreateAirportSeperator() {
            return new MenuItem("-");
        }

        private MenuItem CreateSubscribeGroup() {
            MenuGroup_subscribe = CreateMenuGroup("Subscription", new MenuItem[] {
                    MenuItem_subscribe_Manage = CreateMenuItem("Manage", new EventHandler(SubscriptionManagement)),
                    MenuItem_subscribe_Update = CreateMenuItem("Update", new EventHandler(UpdateSubscription))
                });
            return MenuGroup_subscribe;
        }

        private Configuration GetConfigurationCurrent() {
            configuration_current = controller.GetCurrentConfiguration();
            return configuration_current;
        }

        private MenuItem AdjustServerName(Server server) {
            return new MenuItem(server.NamePrefix(Server.PREFIX_LATENCY) + " " + server.FriendlyName());
        }

        private void UpdateAirportMenu() {
            var items = ServersItem.MenuItems;
            var index_airport = 0;
            var count_seperator = 0;
            for (; index_airport <= items.Count - 1; index_airport++) {
                if (items[index_airport].Text == "-") {
                    count_seperator++;
                    if (count_seperator == 2) {
                        break;
                    }
                }
            }

            index_airport++;
            while (items[index_airport].Text != "-") {
                items.RemoveAt(index_airport);
            }

            var subscription_server_index = configuration_current.configs.Count;
            foreach (var subscription in configuration_current.subscriptions) {
                var MenuItem_airport = new MenuItem(subscription.NamePrefix() + " " + subscription.airport);
                foreach (var server in subscription.servers) {
                    var server_text = server.NamePrefix(Server.PREFIX_LATENCY) + " " + server.FriendlyName();
                    var server_item = new MenuItem(server_text);
                    server_item.Tag = subscription_server_index;
                    server_item.Click += AServerItem_Click;
                    MenuItem_airport.MenuItems.Add(server_item);
                    if (configuration_current.index == subscription_server_index) {
                        server_item.Checked = true;
                        MenuItem_airport.Text = "● " + MenuItem_airport.Text;
                    }
                    subscription_server_index++;
                }
                items.Add(index_airport, MenuItem_airport);
                index_airport++;
            }
        }
        
        private void SetRegularUpdateNull() {
            configuration_current.Timer_regular_update = null;
        }

        private void ResetRegularUpdate() {
            configuration_current.ResetRegularUpdate();
        }

        private void AboutSSD() {
            Process.Start("https://github.com/SoDa-GitHub/SSD-Windows");
        }

        private void ImportURL() {
            var clipboard = Clipboard.GetText(TextDataFormat.Text).Trim();
            if (clipboard.IndexOf("ss://") != -1) {
                var count_old = configuration_current.configs.Count;
                var success = controller.AddServerBySSURL(clipboard);
                var count_new = configuration_current.configs.Count;
                if (success) {
                    ShowBalloonTip(
                        I18N.GetString("Import Success"),
                        string.Format(I18N.GetString("Import Count: {0}"), count_new - count_old),
                        ToolTipIcon.Info,
                        1000
                    );
                }
                else {
                    ShowBalloonTip(
                        I18N.GetString("Import Fail"),
                        string.Format(I18N.GetString("Import URL: {0}"), clipboard),
                        ToolTipIcon.Error,
                        1000
                    );
                }
            }
            else {
                clipboard = clipboard.Replace("ssd://", "");
                try {
                    var new_subscription = controller.GetCurrentConfiguration().ParseBase64(clipboard);
                    Configuration.Save(controller.GetCurrentConfiguration());
                    ShowBalloonTip(
                        I18N.GetString("Import Success"),
                        string.Format(I18N.GetString("Import Airport: {0}"), new_subscription.airport),
                        ToolTipIcon.Info,
                        1000
                    );
                }
                catch (Exception) {
                    ShowBalloonTip(
                        I18N.GetString("Import Fail"),
                        string.Format(I18N.GetString("Import URL: {0}"), clipboard),
                        ToolTipIcon.Error,
                        1000
                    );
                }
            }

            configuration_current.ResetRegularUpdate();
        }

        //endregion

        //public

        public void Quit() {
            Quit_Click(null,null);
        }

        //endpublic

        private void PreloadMenu(object sender, EventArgs e) {
            UpdateServersMenu();
        }
        
        private void SubscriptionManagement(object sender, EventArgs e) {
            configuration_current.Timer_regular_update = null;
            Configuration.Save(configuration_current);
            if (ManageForm == null) {
                ManageForm = new SubscriptionManagementForm(controller);
                ManageForm.Show();
                ManageForm.FormClosed += SubscriptionSettingsRecycled;
            }
            ManageForm.Activate();
        }

        private void SubscriptionSettingsRecycled(object sender, EventArgs e) {
            ManageForm.Dispose();
            ManageForm = null;
            Configuration.Save(configuration_current);
            controller.SelectServerIndex(0);
            configuration_current.ResetRegularUpdate();
        }

        private void UpdateSubscription(object sender, EventArgs e) {
            configuration_current.UpdateAllSubscription(_notifyIcon);
        }
    }
}
