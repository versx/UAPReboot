/*
 * Author: Jeremy Broadbent (versx)
 * Date: 3/14/2017
*/

namespace Ubiquiti.UniFi
{
    using System;
    using System.IO;

    class Program
    {
        static readonly string[] _prefixes = { "--", "-" };

        static void Main(string[] args)
        {
            var user = string.Empty;
            var pass = string.Empty;
            var hosts = string.Empty;
            var port = 22u;
            var verbose = false;

            // If no parameters passed, show usage.
            if (args.Length == 0)
            {
                // Show usage
                return;
            }

            var parameters = CommandLine.ParseArgs(_prefixes, args);
            foreach (var item in parameters)
            {
                switch (item.Key)
                {
                    case "u":
                    case "username":
                        user = Convert.ToString(item.Value);
                        break;
                    case "pw":
                    case "password":
                        pass = Convert.ToString(item.Value);
                        break;
                    case "h":
                    case "hosts":
                        hosts = Convert.ToString(item.Value);
                        break;
                    case "p":
                    case "port":
                        port = Convert.ToUInt16(item.Value);
                        break;
                    case "v":
                    case "verbose":
                        verbose = Convert.ToBoolean(item.Value);
                        break;
                }
            }

            if (string.IsNullOrEmpty(user))
            {
                throw new NullReferenceException("The 'user' parameter cannot be null, exiting...");
            }
            if (string.IsNullOrEmpty(pass))
            {
                throw new NullReferenceException("The 'pass' parameter cannot be null, exiting...");
            }
            if (!File.Exists(hosts))
            {
                throw new FileNotFoundException("The 'hosts.txt' file path specified could not be found, exiting...", nameof(hosts));
            }

            var list = File.ReadAllText(hosts).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var reboot = new UAPReboot
            {
                Username = user,
                Password = pass,
                Port = (ushort)port,
                LogErrors = verbose
            };
            reboot.Hosts.AddRange(list);
            reboot.Run();

            Console.Read();
        }
    }
}