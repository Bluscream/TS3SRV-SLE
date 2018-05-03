using System;
using System.IO;
using CommandLine;
using TS3SRV_SLE.Internal;
using TS3SRV_SLE.Network;
using IniParser;
using IniParser.Model;

namespace TS3SRV_SLE
{
    class Emulator
    {
        static void Main(string[] args)
        {
            var props = new ServerProperties();
            if (File.Exists("config.ini"))
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("config.ini");
                props.Port = int.Parse(data["general"]["port"].ToString());
                props.Name = data["general"]["name"].ToString();
                props.Slots = int.Parse(data["general"]["slots"].ToString());
                props.Clients = int.Parse(data["general"]["clients"].ToString());
                props.IsPasswordProtected = bool.Parse(data["general"]["is_password_protected"].ToString());
                props.CanCreateChannels = bool.Parse(data["general"]["can_create_channels"].ToString());
                props.Binding = data["general"]["bind_to"].ToString();
                props.Weblist = data["general"]["weblist"].ToString();
            }
            else
            {
                Parser.Default.ParseArguments<ServerProperties>(args).WithParsed(opts => { props = opts; })
                    .WithNotParsed(errors =>
                    {
                        Console.WriteLine("Failed to parse commands, program will now exit!");
                        Console.ReadKey();
                        Environment.Exit(1);

                    });
            }
            Console.SetOut(new PrefixedWriter());
            Console.WriteLine("{0}:{1} \"{2}\" {3}/{4} p:{5} c:{6}",props.Binding,props.Port,props.Name,props.Clients,props.Slots,props.IsPasswordProtected,props.CanCreateChannels);
            if (props.Weblist != "weblist.teamspeak.com:2010")
                Console.WriteLine("Using Weblist: {0}", props.Weblist);
            var emu = new Ts3ServerListEmulator(props);
            Console.WriteLine("Emulator started...");
            for (;;)
                Console.ReadKey(true);
        }
    }
}
