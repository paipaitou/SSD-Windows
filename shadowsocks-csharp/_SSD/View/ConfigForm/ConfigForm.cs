using Shadowsocks.Model;

namespace Shadowsocks.View {
    public partial class ConfigForm {
        private void _SaveSubscriptionServer(Server server) {
            server.id = _modifiedConfiguration.configs[_lastSelectedIndex].id;
            server.subscription_url = _modifiedConfiguration.configs[_lastSelectedIndex].subscription_url;
            server.ratio = _modifiedConfiguration.configs[_lastSelectedIndex].ratio;
        }

        private void _LoadSubscriptionServerNameList(Configuration modifiedConfiguration) {
            ServersListBox.Items.Clear();
            foreach(var server in modifiedConfiguration.configs) {
                var ServerText=server.FriendlyName();
                if(server.subscription_url != null) {
                    ServerText = server.NamePrefix(modifiedConfiguration, Server.PREFIX_AIRPORT) + " " + server.FriendlyName();
                }
                ServersListBox.Items.Add(ServerText);
            }
        }

        private void _SetLastSubscriptionName() {
            var server=_modifiedConfiguration.configs[_lastSelectedIndex];
            if(server.subscription_url != null) {
                ServersListBox.Items[_lastSelectedIndex] = server.NamePrefix(_modifiedConfiguration, Server.PREFIX_AIRPORT) + " " + server.FriendlyName();
            }
        }

        private void _DisableMove() {
            MoveUpButton.Visible = false;
            MoveDownButton.Visible = false;
        }
    }
}
