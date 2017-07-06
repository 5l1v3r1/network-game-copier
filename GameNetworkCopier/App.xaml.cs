﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameNetworkCopier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        private SteamOperations steam = new SteamOperations();
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Perform tasks at application exit
            steam.Stop();
        }

    }
    

}
