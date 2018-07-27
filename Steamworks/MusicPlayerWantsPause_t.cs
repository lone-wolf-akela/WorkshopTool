using System.Runtime.InteropServices;

namespace Steamworks
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[CallbackIdentity(4106)]
	public struct MusicPlayerWantsPause_t
	{
		public const int k_iCallback = 4106;
	}
}
