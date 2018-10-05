using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowsocks.Model {
    public partial class Subscription :IComparable<Subscription>, IEqualityComparer<Subscription> {
        public int CompareTo(Subscription compared) {
            var airportCompare= airport.CompareTo(compared.airport);
            if(airportCompare != 0) {
                return airportCompare;
            }
            return url.CompareTo(compared.url);
        }

        public bool Equals(Subscription left, Subscription right) {
            return left.url == right.url;
        }

        public int GetHashCode(Subscription compared) {
            return compared.url.GetHashCode();
        }
    }
}
