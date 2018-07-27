using System.Runtime.InteropServices;

namespace Steamworks
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[CallbackIdentity(4104)]
	public struct MusicPlayerWillQuit_t
	{
		public const int k_iCallback = 4104;
	}
}
