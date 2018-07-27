using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Steamworks
{
	public sealed class CallResult<T>
	{
		public delegate void APIDispatchDelegate(T param, bool bIOFailure);

	    private APIDispatchDelegate m_Func;

        private CCallbackBaseVTable VTable;

		private IntPtr m_pVTable = IntPtr.Zero;

		private CCallbackBase m_CCallbackBase;

		private GCHandle m_pCCallbackBase;

		private SteamAPICall_t m_hAPICall = SteamAPICall_t.Invalid;

		private readonly int m_size = Marshal.SizeOf(typeof(T));

		/*private event APIDispatchDelegate m_Func
		{
			add
			{
				APIDispatchDelegate aPIDispatchDelegate = this.m_Func;
				APIDispatchDelegate aPIDispatchDelegate2;
				do
				{
					aPIDispatchDelegate2 = aPIDispatchDelegate;
					APIDispatchDelegate value2 = (APIDispatchDelegate)Delegate.Combine(aPIDispatchDelegate2, value);
					aPIDispatchDelegate = Interlocked.CompareExchange<APIDispatchDelegate>(ref this.m_Func, value2, aPIDispatchDelegate2);
				}
				while ((object)aPIDispatchDelegate != aPIDispatchDelegate2);
			}
			remove
			{
				APIDispatchDelegate aPIDispatchDelegate = this.m_Func;
				APIDispatchDelegate aPIDispatchDelegate2;
				do
				{
					aPIDispatchDelegate2 = aPIDispatchDelegate;
					APIDispatchDelegate value2 = (APIDispatchDelegate)Delegate.Remove(aPIDispatchDelegate2, value);
					aPIDispatchDelegate = Interlocked.CompareExchange<APIDispatchDelegate>(ref this.m_Func, value2, aPIDispatchDelegate2);
				}
				while ((object)aPIDispatchDelegate != aPIDispatchDelegate2);
			}
		}*/

		public static CallResult<T> Create(APIDispatchDelegate func = null)
		{
			return new CallResult<T>(func);
		}

		public CallResult(APIDispatchDelegate func = null)
		{
			this.m_Func = func;
			BuildCCallbackBase();
		}

		~CallResult()
		{
			Cancel();
			if (m_pVTable != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_pVTable);
			}
			if (m_pCCallbackBase.IsAllocated)
			{
				m_pCCallbackBase.Free();
			}
		}

		public void Set(SteamAPICall_t hAPICall, APIDispatchDelegate func = null)
		{
			if (func != null)
			{
				this.m_Func = func;
			}
			if (this.m_Func == null)
			{
				throw new Exception("CallResult function was null, you must either set it in the CallResult Constructor or in Set()");
			}
			if (m_hAPICall != SteamAPICall_t.Invalid)
			{
				NativeMethods.SteamAPI_UnregisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)m_hAPICall);
			}
			m_hAPICall = hAPICall;
			if (hAPICall != SteamAPICall_t.Invalid)
			{
				NativeMethods.SteamAPI_RegisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)hAPICall);
			}
		}

		public bool IsActive()
		{
			return m_hAPICall != SteamAPICall_t.Invalid;
		}

		public void Cancel()
		{
			if (m_hAPICall != SteamAPICall_t.Invalid)
			{
				NativeMethods.SteamAPI_UnregisterCallResult(m_pCCallbackBase.AddrOfPinnedObject(), (ulong)m_hAPICall);
				m_hAPICall = SteamAPICall_t.Invalid;
			}
		}

		public void SetGameserverFlag()
		{
			m_CCallbackBase.m_nCallbackFlags |= 2;
		}

		private void OnRunCallback(IntPtr thisptr, IntPtr pvParam)
		{
			m_hAPICall = SteamAPICall_t.Invalid;
			this.m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)), false);
		}

		private void OnRunCallResult(IntPtr thisptr, IntPtr pvParam, bool bFailed, ulong hSteamAPICall)
		{
			if ((SteamAPICall_t)hSteamAPICall == m_hAPICall)
			{
				m_hAPICall = SteamAPICall_t.Invalid;
				this.m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)), bFailed);
			}
		}

		private int OnGetCallbackSizeBytes(IntPtr thisptr)
		{
			return m_size;
		}

		private void BuildCCallbackBase()
		{
			VTable = new CCallbackBaseVTable
			{
				m_RunCallback = new CCallbackBaseVTable.RunCBDel(OnRunCallback),
				m_RunCallResult = new CCallbackBaseVTable.RunCRDel(OnRunCallResult),
				m_GetCallbackSizeBytes = new CCallbackBaseVTable.GetCallbackSizeBytesDel(OnGetCallbackSizeBytes)
			};
			m_pVTable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CCallbackBaseVTable)));
			Marshal.StructureToPtr((object)VTable, m_pVTable, false);
			m_CCallbackBase = new CCallbackBase
			{
				m_vfptr = m_pVTable,
				m_nCallbackFlags = 0,
				m_iCallback = CallbackIdentities.GetCallbackIdentity(typeof(T))
			};
			m_pCCallbackBase = GCHandle.Alloc(m_CCallbackBase, GCHandleType.Pinned);
		}
	}
}
