using System.Runtime.InteropServices;

namespace Steamworks
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[CallbackIdentity(4105)]
	public struct MusicPlayerWantsPlay_t
	{
		public const int k_iCallback = 4105;
	}
}
