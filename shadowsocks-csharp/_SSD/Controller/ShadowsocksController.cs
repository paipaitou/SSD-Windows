using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Controller {
    public partial class ShadowsocksController {
        //region
        private void ResetRegularUpdate() {
            _config.ResetRegularUpdate();
        }
        //endregion
    }
}
