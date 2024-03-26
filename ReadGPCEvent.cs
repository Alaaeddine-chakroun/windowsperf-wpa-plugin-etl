using Microsoft.Performance.SDK.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace wpa_plugin_etl
{
    public class ReadGPCEvent
    {
        public ulong Core { get; set; }
        public uint Event { get; set; }
        public uint GPCIdx { get; set; }
        public ulong Value { get; set; }

        public ReadGPCEvent() { }
    }
}
