using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplark
{
	class Program
	{

		static private Dictionary<String, String> _settings = new Dictionary<String, String>();
		
		static void usage()
		{
			Console.WriteLine("Tweet          tw <content>");
			Console.WriteLine("Timeline       tl [<number-of-tweets>]");
			Console.WriteLine("Login          login");
			Console.WriteLine("Quit           q");
			Console.WriteLine("Help           ?");
		}
		
		static void Main(string[] args)
		{
			using (StreamReader sr = new StreamReader("Settings.txt"))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					string[] kvp = line.Split('=');
					_settings.Add(kvp[0], kvp[1]);
				}
			}
			API api = new API(_settings["consumer_key"], _settings["consumer_secret"]);

			bool exit = false;
			while (!exit)
			{
				Console.Write("? ");
				var cmd = Console.ReadLine().Split(' ');
				var cmdLen = cmd.Count();

				if (cmdLen == 0)
				{
					continue;
				}

				var o = cmd[0];
				switch (o)
				{
					case "test":
						api.StatusesUpdate("@mikaderica 얍얍");
						break;

					case "tw":
						if (cmdLen < 2)
						{
							usage();
							break;
						}

						Console.WriteLine(api.StatusesUpdate(String.Join(" ", cmd.Skip(1))));
						break;

					case "tl":
						int count = 15;
						if (cmd.Count() < 1)
						{
							try
							{
								count = Int32.Parse(cmd[1]);
							}
							catch
							{
								Console.WriteLine("Invalid input");
								usage();
								break;
							}
						}
						Console.WriteLine(api.StatusesHomeTineline(count));
						break;

					case "q":
						exit = true;
						break;

					case "login":
						if (!api.Login())
						{
							Console.WriteLine("failed to login");
							break;
						}
						api.printKeys();
						break;

					case "koinichi":
						api.LoginAs(_settings["koinichi_key"], _settings["koinichi_secret"]);
						api.printKeys();
						break;

					case "print":
						api.printKeys();
						break;

					case "?":
					default:
						usage();
						break;
				}
			}
		}
	}
}
