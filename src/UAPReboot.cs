namespace Ubiquiti.UniFi
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    using Renci.SshNet;
    using Renci.SshNet.Common;

    public class UAPReboot
    {
        private const string ErrorLogFileName = "error.log";

        #region Variables

        private int _failed;
        private int _success;
        private bool _cancel;
        private bool _complete;
        private List<string> _uapErrorList;
        private Stopwatch _stopwatch;

        #endregion

        #region Properties

        /// <summary>
        /// List of hostnames or IP addresses.
        /// </summary>
        public List<string> Hosts { get; }

        /// <summary>
        /// SSH connecting port.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Admin username to authenticate with access point.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Admin password to authenticate with access point.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Log errors.
        /// </summary>
        public bool LogErrors { get; set; }

        #endregion

        #region Constructor(s)

        public UAPReboot()
        {
            Hosts = new List<string>();
            _uapErrorList = new List<string>();
            _stopwatch = new Stopwatch();
        }

        public UAPReboot(List<string> hosts, string username, string password)
            : this(hosts, 22, username, password)
        {
        }

        public UAPReboot(List<string> hosts, ushort port, string username, string password)
            : this()
        {
            Hosts = hosts;
            Port = port;
            Username = username;
            Password = password;
        }

        #endregion

        #region Public Methods

        public void Run()
        {
            _cancel = false;
            _complete = false;

            _stopwatch.Start();

            Log(ConsoleColor.Green, true, $"Starting to reboot {Hosts.Count.ToString("N0")} UniFi access points in total, please wait as this may take a few minutes depending on the amount of hosts...");
            Log(ConsoleColor.White, true, string.Empty);

            SshClient client;

            foreach (string host in Hosts)
            {
                if (_cancel) break;

                Log(ConsoleColor.White, false, $"Rebooting access point {host}...");

                string result = string.Empty;
                client = new SshClient(host, Port, Username, Password);
                client.HostKeyReceived += (object sender, HostKeyEventArgs e) =>
                {
                    e.CanTrust = true;
                };
                client.ErrorOccurred += (object sender, ExceptionEventArgs e) =>
                {
                    if (LogErrors)
                    {
                        File.AppendAllText(ErrorLogFileName, $"Error: {e.Exception}");
                    }
                };
                try
                {
                    client.Connect();
                }
                catch (Exception ex)
                {
                    if (LogErrors)
                    {
                        File.AppendAllText(ErrorLogFileName, $"Error: {ex}");
                    }

                    _failed++;
                    _uapErrorList.Add(host);
                    Log(ConsoleColor.Red, true, " [ERR]");
                    continue;
                }

                if (client.IsConnected)
                {
                    result = client.RunCommand("reboot").Result;
                    //Console.WriteLine($"Result for command '{command}':\r\n{result}");
                }
                else
                {
                    _failed++;
                    _uapErrorList.Add(host);
                    if (LogErrors)
                    {
                        File.AppendAllText(ErrorLogFileName, $"Failed to connect to access point {host}, are you sure it's up and running?");
                    }
                    Log(ConsoleColor.Red, true, " [ERR]");
                    continue;
                }

                //TODO: Find a better way to see if the command was executed, check UniFi documentation.
                //if (client.IsConnected || result.Contains("error"))
                if (!string.IsNullOrEmpty(result) || result.Contains("error"))
                {
                    _failed++;
                    _uapErrorList.Add(host);
                    //Log(ConsoleColor.Red, $"Failed to reboot access point {host}.");
                    Log(ConsoleColor.Red, true, " [ERR]");
                    client.Disconnect();
                }
                else
                {
                     _success++;
                    //Log(ConsoleColor.Green, $"Successfully rebooted access point {host}.");
                    Log(ConsoleColor.Green, true, " [OK]");
                }

                client.Dispose();
                client = null;
            }
            _stopwatch.Stop();
            _complete = true;

            PrintCompleteMessage();
        }

        public void Cancel()
        {
            _cancel = true;

            if (!_complete)
            {
                _complete = true;
                Log(ConsoleColor.Yellow, true, "A cancel operation has been initiated...");
            }
        }

        #endregion

        #region Private Methods

        private void PrintCompleteMessage()
        {
            Log(ConsoleColor.White, true, string.Empty);
            Log(ConsoleColor.White, true, string.Empty);

            if (_failed > 0)
            {
                Log(ConsoleColor.Red, true, $"{_failed}/{Hosts.Count.ToString("N0")} access points failed to reboot.");
                Log(ConsoleColor.Red, true, "List of IP addresses that failed to reboot:");
                Log(ConsoleColor.Gray, true, "".PadRight(15, '*'));
                foreach (string failedHost in _uapErrorList)
                {
                    Log(ConsoleColor.White, true, failedHost);
                }
            }
            else
            {
                Log(ConsoleColor.Green, true, $"{_success}/{Hosts.Count.ToString("N0")} access points successfully rebooted.");
            }

            Log(ConsoleColor.White, true, string.Empty);
            Log(ConsoleColor.White, true, $"Total time taken: {_stopwatch.Elapsed}");
        }

        private void Log(ConsoleColor color, bool newline, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            var msg = args.Length > 0 ? string.Format(format, args) : format;
            if (newline)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Console.Write(msg);
            }
            Console.ResetColor();
        }

        #endregion
    }
}