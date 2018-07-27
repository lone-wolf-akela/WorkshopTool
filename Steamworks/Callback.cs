using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Steamworks
{
	public sealed class Callback<T>
	{
		public delegate void DispatchDelegate(T param);

	    private DispatchDelegate m_Func;

        private CCallbackBaseVTable VTable;

		private IntPtr m_pVTable = IntPtr.Zero;

		private CCallbackBase m_CCallbackBase;

		private GCHandle m_pCCallbackBase;

		private bool m_bGameServer;

		private readonly int m_size = Marshal.SizeOf(typeof(T));

		private static List<Callback<T>> GCKeepAlive = new List<Callback<T>>();

		private static bool bWarnedOnce = false;

		/*private event DispatchDelegate m_Func
		{
			add
			{
				DispatchDelegate dispatchDelegate = this.m_Func;
				DispatchDelegate dispatchDelegate2;
				do
				{
					dispatchDelegate2 = dispatchDelegate;
					DispatchDelegate value2 = (DispatchDelegate)Delegate.Combine(dispatchDelegate2, value);
					dispatchDelegate = Interlocked.CompareExchange<DispatchDelegate>(ref this.m_Func, value2, dispatchDelegate2);
				}
				while ((object)dispatchDelegate != dispatchDelegate2);
			}
			remove
			{
				DispatchDelegate dispatchDelegate = this.m_Func;
				DispatchDelegate dispatchDelegate2;
				do
				{
					dispatchDelegate2 = dispatchDelegate;
					DispatchDelegate value2 = (DispatchDelegate)Delegate.Remove(dispatchDelegate2, value);
					dispatchDelegate = Interlocked.CompareExchange<DispatchDelegate>(ref this.m_Func, value2, dispatchDelegate2);
				}
				while ((object)dispatchDelegate != dispatchDelegate2);
			}
		}*/

		public static Callback<T> Create(DispatchDelegate func)
		{
			return new Callback<T>(func, false, false);
		}

		public static Callback<T> CreateGameServer(DispatchDelegate func)
		{
			return new Callback<T>(func, true, false);
		}

		public Callback(DispatchDelegate func, bool bGameServer = false, bool bKeepAlive = true)
		{
			m_bGameServer = bGameServer;
			BuildCCallbackBase();
			Register(func);
			if (bKeepAlive)
			{
				if (!bWarnedOnce)
				{
					bWarnedOnce = true;
					throw new InvalidOperationException("Please use the new (as of 3.0.0) api for creating Callbacks. Callback<Type>.Create(func). You must now maintain a handle to the callback so that the GC does not clean it up prematurely.");
				}
				GCKeepAlive.Add(this);
			}
		}

		~Callback()
		{
			Unregister();
			if (m_pVTable != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_pVTable);
			}
			if (m_pCCallbackBase.IsAllocated)
			{
				m_pCCallbackBase.Free();
			}
		}

		public void Register(DispatchDelegate func)
		{
			if (func == null)
			{
				throw new Exception("Callback function must not be null.");
			}
			if ((m_CCallbackBase.m_nCallbackFlags & 1) == 1)
			{
				Unregister();
			}
			if (m_bGameServer)
			{
				SetGameserverFlag();
			}
			this.m_Func = func;
			NativeMethods.SteamAPI_RegisterCallback(m_pCCallbackBase.AddrOfPinnedObject(), CallbackIdentities.GetCallbackIdentity(typeof(T)));
		}

		public void Unregister()
		{
			NativeMethods.SteamAPI_UnregisterCallback(m_pCCallbackBase.AddrOfPinnedObject());
		}

		public void SetGameserverFlag()
		{
			m_CCallbackBase.m_nCallbackFlags |= 2;
		}

		private void OnRunCallback(IntPtr thisptr, IntPtr pvParam)
		{
			this.m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)));
		}

		private void OnRunCallResult(IntPtr thisptr, IntPtr pvParam, bool bFailed, ulong hSteamAPICall)
		{
			this.m_Func((T)Marshal.PtrToStructure(pvParam, typeof(T)));
		}

		private int OnGetCallbackSizeBytes(IntPtr thisptr)
		{
			return m_size;
		}

		private void BuildCCallbackBase()
		{
			VTable = new CCallbackBaseVTable
			{
				m_RunCallResult = new CCallbackBaseVTable.RunCRDel(OnRunCallResult),
				m_RunCallback = new CCallbackBaseVTable.RunCBDel(OnRunCallback),
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
