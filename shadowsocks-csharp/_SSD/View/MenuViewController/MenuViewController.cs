using Shadowsocks.Controller;
using Shadowsocks.Model;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Shadowsocks.View {
    public partial class MenuViewController {
        private Configuration ConfigurationCurrent;

        private MenuItem MenuGroup_subscribe;
        private MenuItem MenuItem_subscribeManage;
        private MenuItem MenuItem_subscribeUpdate;
        private SubscriptionManagementForm Form_subscriptionManagement;

        private void _DisableFirstRun() {

        }

        private void _InitOther() {
            ConfigurationCurrent = controller.GetCurrentConfiguration();
            ConfigurationCurrent.MenuView = this;
            ConfigurationCurrent.ResetRegularDetectRunning();
            ConfigurationCurrent.ResetRegularUpdate();
            contextMenu1.Popup += PreloadMenu;
        }

        private MenuItem _CreateAirportSeperator() {
            return new MenuItem("-");
        }

        private MenuItem _CreateSubscribeGroup() {
            MenuGroup_subscribe = CreateMenuGroup("Subscription", new MenuItem[] {
                    MenuItem_subscribeManage = CreateMenuItem("Manage", new EventHandler(SubscriptionManagement)),
                    MenuItem_subscribeUpdate = CreateMenuItem("Update", new EventHandler(UpdateSubscription))
                });
            return MenuGroup_subscribe;
        }

        private void _OpenUpdateInfo() {
            Process.Start(UpdateChecker.RELEASE_URL_SSD);
        }

        private Configuration _GetConfigurationCurrent() {
            ConfigurationCurrent = controller.GetCurrentConfiguration();
            return ConfigurationCurrent;
        }

        private MenuItem _AdjustServerName(Server server) {
            return new MenuItem(server.NamePrefix(ConfigurationCurrent, Server.PREFIX_LATENCY) + " " + server.FriendlyName());
        }

        private void _UpdateAirportMenu(int inherit_index) {
            var items = ServersItem.MenuItems;
            var indexAirport = 0;
            var countSeperator = 0;
            //todo ssd:to switch foreach
            for(; indexAirport <= items.Count - 1; indexAirport++) {
                if(items[indexAirport].Text == "-") {
                    countSeperator++;
                    if(countSeperator == 2) {
                        break;
                    }
                }
            }

            indexAirport++;
            while(items[indexAirport].Text != "-") {
                items.RemoveAt(indexAirport);
            }

            var subscriptions = ConfigurationCurrent.subscriptions;
            for(var index = 0; index <= subscriptions.Count - 1; index++) {
                var MenuItem_airport = new MenuItem(subscriptions[index].NamePrefix() + " " + subscriptions[index].airport);
                foreach(var server in subscriptions[index].GetServers()) {
                    var serverText = server.NamePrefix(ConfigurationCurrent,Server.PREFIX_LATENCY) + " " + server.FriendlyName();
                    var serverItem = new MenuItem(serverText);
                    serverItem.Tag = inherit_index;
                    serverItem.Click += AServerItem_Click;
                    MenuItem_airport.MenuItems.Add(serverItem);
                    if(ConfigurationCurrent.index == inherit_index) {
                        serverItem.Checked = true;
                        MenuItem_airport.Text = "● " + MenuItem_airport.Text;
                    }
                    inherit_index++;
                }
                items.Add(indexAirport, MenuItem_airport);
                indexAirport++;
            }
        }

        private void _StopRegularUpdate() {
            ConfigurationCurrent.StopRegularUpdate();
        }

        private void _ResetRegularUpdate() {
            ConfigurationCurrent.ResetRegularUpdate();
        }

        private void _AboutSSD() {
            Process.Start("https://github.com/SoDa-GitHub/SSD-Windows");
        }

        private void _ImportURL() {
            var clipboard = Clipboard.GetText(TextDataFormat.Text).Trim();
            if(clipboard.IndexOf("ss://") != -1) {
                var count_old = ConfigurationCurrent.configs.Count;
                var success = controller.AddServerBySSURL(clipboard);
                var count_new = ConfigurationCurrent.configs.Count;
                if(success) {
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
                try {
                    var new_subscription = ConfigurationCurrent.ParseBase64WithHead(clipboard);
                    ConfigurationCurrent.subscriptions.Add(new_subscription);
                    Configuration.Save(ConfigurationCurrent);
                    ShowBalloonTip(
                        I18N.GetString("Import Success"),
                        string.Format(I18N.GetString("Import Airport: {0}"), new_subscription.airport),
                        ToolTipIcon.Info,
                        1000
                    );
                }
                catch(Exception) {
                    ShowBalloonTip(
                        I18N.GetString("Import Fail"),
                        string.Format(I18N.GetString("Import URL: {0}"), clipboard),
                        ToolTipIcon.Error,
                        1000
                    );
                }
            }

            ConfigurationCurrent.ResetRegularUpdate();
        }
    }
}
