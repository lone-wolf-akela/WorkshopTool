using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks
{
	public class InteropHelp
	{
		public class SteamParamStringArray
		{
			private IntPtr[] m_Strings;

			private IntPtr m_ptrStrings;

			private IntPtr m_pSteamParamStringArray;

			public SteamParamStringArray(IList<string> strings)
			{
				if (strings == null)
				{
					m_pSteamParamStringArray = IntPtr.Zero;
				}
				else
				{
					m_Strings = new IntPtr[strings.Count];
					for (int i = 0; i < strings.Count; i++)
					{
						byte[] strbuf = new byte[Encoding.UTF8.GetByteCount(strings[i]) + 1];
						Encoding.UTF8.GetBytes(strings[i], 0, strings[i].Length, strbuf, 0);
						m_Strings[i] = Marshal.AllocHGlobal(strbuf.Length);
						Marshal.Copy(strbuf, 0, m_Strings[i], strbuf.Length);
					}
					m_ptrStrings = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * m_Strings.Length);
					SteamParamStringArray_t stringArray = new SteamParamStringArray_t
					{
						m_ppStrings = m_ptrStrings,
						m_nNumStrings = m_Strings.Length
					};
					Marshal.Copy(m_Strings, 0, stringArray.m_ppStrings, m_Strings.Length);
					m_pSteamParamStringArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SteamParamStringArray_t)));
					Marshal.StructureToPtr((object)stringArray, m_pSteamParamStringArray, false);
				}
			}

			~SteamParamStringArray()
			{
				IntPtr[] strings = m_Strings;
				foreach (IntPtr ptr in strings)
				{
					Marshal.FreeHGlobal(ptr);
				}
				if (m_ptrStrings != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(m_ptrStrings);
				}
				if (m_pSteamParamStringArray != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(m_pSteamParamStringArray);
				}
			}

			public static implicit operator IntPtr(SteamParamStringArray that)
			{
				return that.m_pSteamParamStringArray;
			}
		}

		public static void TestIfPlatformSupported()
		{
		}

		public static void TestIfAvailableClient()
		{
			TestIfPlatformSupported();
			if (NativeMethods.SteamClient() == IntPtr.Zero)
			{
				throw new InvalidOperationException("Steamworks is not initialized.");
			}
		}

		public static void TestIfAvailableGameServer()
		{
			TestIfPlatformSupported();
			if (NativeMethods.SteamClientGameServer() == IntPtr.Zero)
			{
				throw new InvalidOperationException("Steamworks is not initialized.");
			}
		}

		public static string PtrToStringUTF8(IntPtr nativeUtf8)
		{
			if (nativeUtf8 == IntPtr.Zero)
			{
				return string.Empty;
			}
			int len;
			for (len = 0; Marshal.ReadByte(nativeUtf8, len) != 0; len++)
			{
			}
			if (len == 0)
			{
				return string.Empty;
			}
			byte[] buffer = new byte[len];
			Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
			return Encoding.UTF8.GetString(buffer);
		}
	}
}
