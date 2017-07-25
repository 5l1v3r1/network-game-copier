﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reactive.Linq;

namespace NetworkGameCopier
{

    public delegate void DelAddComputer(string computer);
    public delegate void DelProgress(double percentage);
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int NetworkManagerPort = 8086;

        private NetworkManager _server;
        private NetworkManager _client;
        private string _targetClientIp;
        private NetworkPerformanceReporter _network;

        public MainWindow()
        {
            Init();
            InitializeComponent();
            GameProviderSingleton.GetInstance();
            Refresh_Button_Click(null, null);
            _network = NetworkPerformanceReporter.Create();
            //Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(v =>
            //{
            //    Console.WriteLine(_network.GetNetworkPerformanceData().BytesReceived);
            //});
        }

       

        private void Init()
        {
            ConfigLog();
            DiscoverService.GetInstance().StartListening();
            _server = new NetworkManager(NetworkManagerPort);
        }

        private NameSizePair SizeFromBytesToMBytes(NameSizePair pair)
        {
            string size = pair.Size;
            long parsed;
            if (Int64.TryParse(size, out parsed))
            {
                long sizeInMb = parsed / 1024 / 1024;
                return new NameSizePair {Name = pair.Name, Size = sizeInMb.ToString()};
            }
            else
                return new NameSizePair {Name = pair.Name, Size = "0"};
        }

        private void ConfigLog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
            fileTarget.FileName = "${basedir}/file.txt";
            fileTarget.Layout = "${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Example usage
            Logger logger = LogManager.GetLogger("Example");
            logger.Trace("trace log message");
            logger.Debug("debug log message");
            logger.Info("info log message");
            logger.Warn("warn log message");
            logger.Error("error log message");
            logger.Fatal("fatal log message");
        }

        private void GamesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("Selected: {0}", e.AddedItems[0]);
            NameSizePair gamePair = e.AddedItems[0] as NameSizePair;
            if (gamePair != null)
            {
                Progress(0);
                    GameProviderSingleton.GetInstance().Active
                        .RetrieveGame(gamePair.Name, _client, _targetClientIp,
                            new AsyncPack {ToExecute = new DelProgress(Progress), Window = this});
            }
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            ComputerComboBox.Items.Clear();
            DiscoverService.GetInstance().RetrieveLiveServers(this, AddComputer);
        }

        public void AddComputer(string computer)
        {
            if(!ComputerComboBox.IsEditable) ComputerComboBox.IsEnabled = true;
            ComputerComboBox.Items.Add(computer);
        }

        public void Progress(double percentage)
        {
            if (Math.Abs(percentage - 100) < 0.1)
            {
                ProgressBar.Opacity = 0;
                Percentage.Opacity = 0;
                StateText.Text = "Idle";
            }
            else
            {
                if (Math.Abs(ProgressBar.Opacity - 100) > 1)
                {
                    ProgressBar.Opacity = 100;
                    Percentage.Opacity = 100;
                    StateText.Text = "Listing";

                }else
                    StateText.Text = "Downloading";
                ProgressBar.Value = percentage;
                Percentage.Text = percentage.ToString("0.00") + "%";
            }
        }

        private void ComputerComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _targetClientIp = e.AddedItems[0].ToString();
            //string ip = Dns.GetHostAddresses(_targetClientIp)[0].ToString();
            LogManager.GetCurrentClassLogger().Info("Current client selected: " + _targetClientIp);
            _client = (NetworkManager)Activator.GetObject(
                typeof(NetworkManager),
                BuildTcpRemoteEndpoint(_targetClientIp));
            LogManager.GetCurrentClassLogger().Info(_client.Ping());
            FillList();
        }

        private void FillList()
        {
            if(_client == null) return;
            NameSizePair[] list = GameProviderSingleton.GetInstance().Active.GetRemoteGamesNamesList(_client);
            Array.Sort(list);
            GamesList.Items.Clear();
            foreach (NameSizePair game in list)
            {
                GamesList.Items.Add(SizeFromBytesToMBytes(game));
            }
        }

        private static string BuildTcpRemoteEndpoint(string ip)
        {
            return "tcp://" + ip + ":" + NetworkManagerPort + "/NetworkManager";
        }

        private void ButtonBlizzard_OnClick(object sender, RoutedEventArgs e)
        {
            GameProviderSingleton.GetInstance().Active = 
                BlizzardOperations.GetInstance();
            FillList();
        }

        private void ButtonSteam_OnClick(object sender, RoutedEventArgs e)
        {
            GameProviderSingleton.GetInstance().Active =
                SteamOperations.GetInstance();
            FillList();
        }
    }

   
}