using System;
using System.Runtime.InteropServices;

namespace Steamworks
{
	public class ISteamMatchmakingRulesResponse
	{
		public delegate void RulesResponded(string pchRule, string pchValue);

		public delegate void RulesFailedToRespond();

		public delegate void RulesRefreshComplete();

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void InternalRulesResponded(IntPtr thisptr, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Steamworks.UTF8Marshaler")] string pchRule, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Steamworks.UTF8Marshaler")] string pchValue);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void InternalRulesFailedToRespond(IntPtr thisptr);

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public delegate void InternalRulesRefreshComplete(IntPtr thisptr);

		[StructLayout(LayoutKind.Sequential)]
		private class VTable
		{
			[NonSerialized]
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public InternalRulesResponded m_VTRulesResponded;

			[NonSerialized]
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public InternalRulesFailedToRespond m_VTRulesFailedToRespond;

			[NonSerialized]
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public InternalRulesRefreshComplete m_VTRulesRefreshComplete;
		}

		private VTable m_VTable;

		private IntPtr m_pVTable;

		private GCHandle m_pGCHandle;

		private RulesResponded m_RulesResponded;

		private RulesFailedToRespond m_RulesFailedToRespond;

		private RulesRefreshComplete m_RulesRefreshComplete;

		public ISteamMatchmakingRulesResponse(RulesResponded onRulesResponded, RulesFailedToRespond onRulesFailedToRespond, RulesRefreshComplete onRulesRefreshComplete)
		{
			if (onRulesResponded == null || onRulesFailedToRespond == null || onRulesRefreshComplete == null)
			{
				throw new ArgumentNullException();
			}
			m_RulesResponded = onRulesResponded;
			m_RulesFailedToRespond = onRulesFailedToRespond;
			m_RulesRefreshComplete = onRulesRefreshComplete;
			m_VTable = new VTable
			{
				m_VTRulesResponded = new InternalRulesResponded(InternalOnRulesResponded),
				m_VTRulesFailedToRespond = new InternalRulesFailedToRespond(InternalOnRulesFailedToRespond),
				m_VTRulesRefreshComplete = new InternalRulesRefreshComplete(InternalOnRulesRefreshComplete)
			};
			m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VTable)));
			Marshal.StructureToPtr((object)m_VTable, m_pVTable, false);
			m_pGCHandle = GCHandle.Alloc(m_pVTable, GCHandleType.Pinned);
		}

		~ISteamMatchmakingRulesResponse()
		{
			if (m_pVTable != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_pVTable);
			}
			if (m_pGCHandle.IsAllocated)
			{
				m_pGCHandle.Free();
			}
		}

		private void InternalOnRulesResponded(IntPtr thisptr, string pchRule, string pchValue)
		{
			m_RulesResponded(pchRule, pchValue);
		}

		private void InternalOnRulesFailedToRespond(IntPtr thisptr)
		{
			m_RulesFailedToRespond();
		}

		private void InternalOnRulesRefreshComplete(IntPtr thisptr)
		{
			m_RulesRefreshComplete();
		}

		public static explicit operator IntPtr(ISteamMatchmakingRulesResponse that)
		{
			return that.m_pGCHandle.AddrOfPinnedObject();
		}
	}
}
