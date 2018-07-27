using System.Runtime.InteropServices;

namespace Steamworks
{
	public static class Packsize
	{
		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		private struct ValvePackingSentinel_t
		{
			private uint m_u32;

			private ulong m_u64;

			private ushort m_u16;

			private double m_d;
		}

		public const int value = 8;

		public static bool Test()
		{
			int sentinelSize = Marshal.SizeOf(typeof(ValvePackingSentinel_t));
			int subscribedFilesSize = Marshal.SizeOf(typeof(RemoteStorageEnumerateUserSubscribedFilesResult_t));
			if (sentinelSize != 32 || subscribedFilesSize != 616)
			{
				return false;
			}
			return true;
		}
	}
}
