using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks
{
	public class UTF8Marshaler : ICustomMarshaler
	{
		public const string DoNotFree = "DoNotFree";

		private static UTF8Marshaler static_instance_free = new UTF8Marshaler(true);

		private static UTF8Marshaler static_instance = new UTF8Marshaler(false);

		private bool _freeNativeMemory;

		private UTF8Marshaler(bool freenativememory)
		{
			_freeNativeMemory = freenativememory;
		}

		public IntPtr MarshalManagedToNative(object managedObj)
		{
			if (managedObj == null)
			{
				return IntPtr.Zero;
			}
			string str = managedObj as string;
			if (str == null)
			{
				throw new Exception("UTF8Marshaler must be used on a string.");
			}
			byte[] strbuf = new byte[Encoding.UTF8.GetByteCount(str) + 1];
			Encoding.UTF8.GetBytes(str, 0, str.Length, strbuf, 0);
			IntPtr buffer = Marshal.AllocHGlobal(strbuf.Length);
			Marshal.Copy(strbuf, 0, buffer, strbuf.Length);
			return buffer;
		}

		public object MarshalNativeToManaged(IntPtr pNativeData)
		{
			int len;
			for (len = 0; Marshal.ReadByte(pNativeData, len) != 0; len++)
			{
			}
			if (len == 0)
			{
				return string.Empty;
			}
			byte[] strbuf = new byte[len];
			Marshal.Copy(pNativeData, strbuf, 0, strbuf.Length);
			return Encoding.UTF8.GetString(strbuf);
		}

		public void CleanUpNativeData(IntPtr pNativeData)
		{
			if (_freeNativeMemory)
			{
				Marshal.FreeHGlobal(pNativeData);
			}
		}

		public void CleanUpManagedData(object managedObj)
		{
		}

		public int GetNativeDataSize()
		{
			return -1;
		}

		public static ICustomMarshaler GetInstance(string cookie)
		{
			string a;
			if ((a = cookie) != null && a == "DoNotFree")
			{
				return static_instance;
			}
			return static_instance_free;
		}
	}
}
