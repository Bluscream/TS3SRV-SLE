using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace TS3SRV_SLE.Internal
{
    public class ServerProperties
    {
        [Option("port", Required = false, Default = 9987, HelpText = "Port of the server")]
        public int Port { get; set; }

        [Option('n', "name", Required = false, Default = "TS3SRV-SLE", HelpText = "Name of the server")]
        public string Name { get; set; }

        [Option("slots", Required = false, Default = 512, HelpText = "Number of slots")]
        public int Slots { get; set; }

        [Option("clients", Required = false, Default = 98, HelpText = "Number of connected clients")]
        public int Clients { get; set; }

        [Option('p', "ispasswordprotected", Default = false, Required = false)]
        public bool IsPasswordProtected { get; set; }

        [Option('c', "cancreatechannels", Default = true, Required = false)]
        public bool CanCreateChannels { get; set; }

        [Option("binding", Default = "0.0.0.0", Required = false)]
        public string Binding { get; set; }

        [Option("weblist", Default = "weblist.teamspeak.com:2010", Required = false)]
        public string Weblist { get; set; }
    }
}
