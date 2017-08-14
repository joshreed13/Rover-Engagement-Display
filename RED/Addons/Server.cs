﻿using RED.Contexts;
using System.Net;

namespace RED.Addons
{
    public class Server
    {
        public string Name { get; set; }
        public IPAddress Address { get; set; }

        public Server(string name, IPAddress address)
        {
            Name = name;
            Address = address;
        }

        public Server(MetadataServerContext context)
        {
            Name = context.Name;
            IPAddress ip;
            IPAddress.TryParse(context.Address, out ip);
            Address = ip ?? null;
        }
    }
}
