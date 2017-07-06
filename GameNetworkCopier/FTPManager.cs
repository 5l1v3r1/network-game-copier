﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.FileSystem.DotNet;
using TestFtpServer.Logging;

namespace GameNetworkCopier
{
    class FtpManager
    {
        private FtpServer _ftpServer;
        private static readonly FtpManager Instance = new FtpManager();

        public static FtpManager GetInstance()
        {
            return Instance;
        }

        public void LaunchFtpServer(string pathToServe)
        {
            // allow only anonymous logins
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());

            // use %TEMP%/TestFtpServer as root folder
            var fsProvider = new DotNetFileSystemProvider(pathToServe, false);

            // Initialize the FTP server
            using (_ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1")
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            })

                // Start the FTP server
                _ftpServer.Start();

        }

        public void StopFtpServer()
        {
            _ftpServer?.Stop();
        }

        public void ftpClient()
        {
            // create an FTP client
            FtpClient client = new FtpClient("localhost");

            // if you don't specify login credentials, we use the "anonymous" user account
            //client.Credentials = new NetworkCredential("david", "pass123");

            // begin connecting to the server
            client.Connect();

            // get a list of files and directories in the "/htdocs" folder
            foreach (FtpListItem item in client.GetListing("/htdocs"))
            {

                // if this is a file
                if (item.Type == FtpFileSystemObjectType.File)
                {

                    // get the file size
                    long size = client.GetFileSize(item.FullName);

                }

                // get modified date/time of the file or folder
                DateTime time = client.GetModifiedTime(item.FullName);

                // calculate a hash for the file on the server side (default algorithm)
                FtpHash hash = client.GetHash(item.FullName);

            }

            // upload a file
            client.UploadFile(@"C:\teste\asdf.txt", "/htdocs/big.txt");

            // rename the uploaded file
            client.Rename("/htdocs/big.txt", "/htdocs/big2.txt");

            // download the file again
            client.DownloadFile(@"C:\lel.txt", "/htdocs/teste.txt");

            // delete the file
            //client.DeleteFile("/htdocs/big2.txt");

            // delete a folder recursively
            //client.DeleteDirectory("/htdocs/extras/");

            // check if a file exists
            if (client.FileExists("/htdocs/big2.txt")) { }

            // check if a folder exists
            if (client.DirectoryExists("/htdocs/extras/")) { }

            // upload a file and retry 3 times before giving up
            client.RetryAttempts = 3;
          //  client.UploadFile(@"C:\MyVideo.mp4", "/htdocs/big.txt", FtpExists.Overwrite, false, FtpVerify.Retry);

            // disconnect! good bye!
            client.Disconnect();
        }
    }
}
