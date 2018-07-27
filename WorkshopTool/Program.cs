using Steamworks;
using System;
using System.Windows.Forms;

namespace WorkshopTool
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			if (!SteamAPI.Init())
			{
				MessageBox.Show("Steam initialization failed.  Is Steam running?", "Steam Error", MessageBoxButtons.OK);
			}
			else if (!SteamUser.BLoggedOn())
			{
				MessageBox.Show("User is not logged into Steam.", "Steam Error", MessageBoxButtons.OK);
			}
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new WorkshopTool(args));
			}
		}
	}
}
