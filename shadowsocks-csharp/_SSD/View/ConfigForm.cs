using Shadowsocks.Controller;
using Shadowsocks.Model;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Shadowsocks.View {
    public partial class ConfigForm {
        private Dictionary<string, Server> SubscriptionServerMap = new Dictionary<string, Server>();

        //region SSD

        private void LoadSelectedSubscriptionServerDetails() {
            if(ServersListBox.SelectedIndex >= _modifiedConfiguration.configs.Count) {
                SetServerDetailsToUI(SubscriptionServerMap[(string)ServersListBox.Items[ServersListBox.SelectedIndex]]);
            }
        }

        private void LoadSubscriptionServerNameList(Configuration modifiedConfiguration) {
            SubscriptionServerMap.Clear();
            foreach(var subscription in modifiedConfiguration.subscriptions) {
                foreach(var server in subscription.servers) {
                    var ServerText = server.NamePrefix(Server.PREFIX_AIRPORT) + " " + server.FriendlyName();
                    ServersListBox.Items.Add(ServerText);
                    SubscriptionServerMap.Add(ServerText, server);
                }
            }
        }

        private bool ChechListCount() {
            if(ServersListBox.Items.Count == 0) {
                MessageBox.Show(I18N.GetString("Please add at least one server"));
                return false;
            }
            else {
                return true;
            }
        }

        private void DisableMove() {
            MoveUpButton.Visible = false;
            MoveDownButton.Visible = false;
        }

        //endregion
    }
}
