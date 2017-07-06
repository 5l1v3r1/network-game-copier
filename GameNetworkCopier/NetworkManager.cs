﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace GameNetworkCopier
{
    class NetworkManager : MarshalByRefObject
    {
        public NetworkManager(int port)
        {
            ChannelServices.RegisterChannel(new TcpChannel(port), false);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(NetworkManager),
                "NetworkManager",
                WellKnownObjectMode.Singleton);
        }

        public NetworkManager()
        {
        }

        public string ping()
        {
            Console.WriteLine("Ping!");
            return "Pong!";
        }

        public List<String> GetGamesNamesList()
        {
            return BackendSingleton.getInstance()._steam.GetGameNamesList();
        }

        public string GetDirFromGameName(string gameName)
        {
            return BackendSingleton.getInstance()._steam.GetDirFromGameName(gameName);
        }

    }
}
