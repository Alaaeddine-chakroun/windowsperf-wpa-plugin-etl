using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpa_plugin_etl
{
    public class ReadGPCEventApp
    {
        public ulong Core { get; set; }
        public String Event { get; set; }
        public uint EventIdx { get; set; }
        public String EventNote { get; set; }
        public ulong Value { get; set; }

        public ReadGPCEventApp() { }
    }
}
