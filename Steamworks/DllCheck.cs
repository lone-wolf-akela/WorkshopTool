using System;
using System.Diagnostics;
using System.IO;

namespace Steamworks
{
	public class DllCheck
	{
		public static bool Test()
		{
			bool ret = true;
			return CheckSteamAPIDLL();
		}

		private static bool CheckSteamAPIDLL()
		{
			string strCWD = Directory.GetCurrentDirectory();
			string file;
			int fileBytes;
			if (IntPtr.Size == 4)
			{
				file = Path.Combine(strCWD, "steam_api.dll");
				fileBytes = 145600;
			}
			else
			{
				file = Path.Combine(strCWD, "steam_api64.dll");
				fileBytes = 169152;
			}
			if (File.Exists(file))
			{
				FileInfo fInfo2 = new FileInfo(file);
				if (fInfo2.Length != fileBytes)
				{
					return false;
				}
				if (FileVersionInfo.GetVersionInfo(file).FileVersion != "02.37.91.26")
				{
					return false;
				}
			}
			return true;
		}
	}
}
