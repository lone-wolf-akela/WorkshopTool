using System.Runtime.InteropServices;

namespace Steamworks
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[CallbackIdentity(4102)]
	public struct MusicPlayerRemoteWillDeactivate_t
	{
		public const int k_iCallback = 4102;
	}
}
