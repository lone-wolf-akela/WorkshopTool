using System;
using System.Runtime.InteropServices;

namespace Steamworks
{
	public static class SteamApps
	{
		public static bool BIsSubscribed()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsSubscribed();
		}

		public static bool BIsLowViolence()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsLowViolence();
		}

		public static bool BIsCybercafe()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsCybercafe();
		}

		public static bool BIsVACBanned()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsVACBanned();
		}

		public static string GetCurrentGameLanguage()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetCurrentGameLanguage();
		}

		public static string GetAvailableGameLanguages()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetAvailableGameLanguages();
		}

		public static bool BIsSubscribedApp(AppId_t appID)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsSubscribedApp(appID);
		}

		public static bool BIsDlcInstalled(AppId_t appID)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsDlcInstalled(appID);
		}

		public static uint GetEarliestPurchaseUnixTime(AppId_t nAppID)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetEarliestPurchaseUnixTime(nAppID);
		}

		public static bool BIsSubscribedFromFreeWeekend()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsSubscribedFromFreeWeekend();
		}

		public static int GetDLCCount()
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetDLCCount();
		}

		public static bool BGetDLCDataByIndex(int iDLC, out AppId_t pAppID, out bool pbAvailable, out string pchName, int cchNameBufferSize)
		{
			InteropHelp.TestIfAvailableClient();
			IntPtr pchName2 = Marshal.AllocHGlobal(cchNameBufferSize);
			bool ret = NativeMethods.ISteamApps_BGetDLCDataByIndex(iDLC, out pAppID, out pbAvailable, pchName2, cchNameBufferSize);
			pchName = (ret ? InteropHelp.PtrToStringUTF8(pchName2) : null);
			Marshal.FreeHGlobal(pchName2);
			return ret;
		}

		public static void InstallDLC(AppId_t nAppID)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamApps_InstallDLC(nAppID);
		}

		public static void UninstallDLC(AppId_t nAppID)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamApps_UninstallDLC(nAppID);
		}

		public static void RequestAppProofOfPurchaseKey(AppId_t nAppID)
		{
			InteropHelp.TestIfAvailableClient();
			NativeMethods.ISteamApps_RequestAppProofOfPurchaseKey(nAppID);
		}

		public static bool GetCurrentBetaName(out string pchName, int cchNameBufferSize)
		{
			InteropHelp.TestIfAvailableClient();
			IntPtr pchName2 = Marshal.AllocHGlobal(cchNameBufferSize);
			bool ret = NativeMethods.ISteamApps_GetCurrentBetaName(pchName2, cchNameBufferSize);
			pchName = (ret ? InteropHelp.PtrToStringUTF8(pchName2) : null);
			Marshal.FreeHGlobal(pchName2);
			return ret;
		}

		public static bool MarkContentCorrupt(bool bMissingFilesOnly)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_MarkContentCorrupt(bMissingFilesOnly);
		}

		public static uint GetInstalledDepots(AppId_t appID, DepotId_t[] pvecDepots, uint cMaxDepots)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetInstalledDepots(appID, pvecDepots, cMaxDepots);
		}

		public static uint GetAppInstallDir(AppId_t appID, out string pchFolder, uint cchFolderBufferSize)
		{
			InteropHelp.TestIfAvailableClient();
			IntPtr pchFolder2 = Marshal.AllocHGlobal((int)cchFolderBufferSize);
			uint ret = NativeMethods.ISteamApps_GetAppInstallDir(appID, pchFolder2, cchFolderBufferSize);
			pchFolder = ((ret != 0) ? InteropHelp.PtrToStringUTF8(pchFolder2) : null);
			Marshal.FreeHGlobal(pchFolder2);
			return ret;
		}

		public static bool BIsAppInstalled(AppId_t appID)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_BIsAppInstalled(appID);
		}

		public static CSteamID GetAppOwner()
		{
			InteropHelp.TestIfAvailableClient();
			return (CSteamID)NativeMethods.ISteamApps_GetAppOwner();
		}

		public static string GetLaunchQueryParam(string pchKey)
		{
			InteropHelp.TestIfAvailableClient();
			return NativeMethods.ISteamApps_GetLaunchQueryParam(pchKey);
		}
	}
}
