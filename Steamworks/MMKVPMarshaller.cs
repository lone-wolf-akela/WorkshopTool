using System;
using System.Runtime.InteropServices;

namespace Steamworks
{
	public class MMKVPMarshaller
	{
		private IntPtr[] m_AllocatedMemory;

		private IntPtr m_NativeArray;

		public MMKVPMarshaller(MatchMakingKeyValuePair_t[] filters)
		{
			if (filters != null)
			{
				m_AllocatedMemory = new IntPtr[filters.Length];
				int intPtrSize = Marshal.SizeOf(typeof(IntPtr));
				m_NativeArray = Marshal.AllocHGlobal(intPtrSize * filters.Length);
				for (int i = 0; i < filters.Length; i++)
				{
					m_AllocatedMemory[i] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MatchMakingKeyValuePair_t)));
					Marshal.StructureToPtr((object)filters[i], m_AllocatedMemory[i], false);
					Marshal.WriteIntPtr(m_NativeArray, i * intPtrSize, m_AllocatedMemory[i]);
				}
			}
		}

		~MMKVPMarshaller()
		{
			if (m_NativeArray != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(m_NativeArray);
				IntPtr[] allocatedMemory = m_AllocatedMemory;
				foreach (IntPtr ptr in allocatedMemory)
				{
					if (ptr != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(ptr);
					}
				}
			}
		}

		public static implicit operator IntPtr(MMKVPMarshaller that)
		{
			return that.m_NativeArray;
		}
	}
}
