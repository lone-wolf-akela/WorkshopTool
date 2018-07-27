using System;

namespace Steamworks
{
	public struct CSteamID : IEquatable<CSteamID>, IComparable<CSteamID>
	{
		public static readonly CSteamID Nil = default(CSteamID);

		public static readonly CSteamID OutofDateGS = new CSteamID(new AccountID_t(0u), 0u, EUniverse.k_EUniverseInvalid, EAccountType.k_EAccountTypeInvalid);

		public static readonly CSteamID LanModeGS = new CSteamID(new AccountID_t(0u), 0u, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeInvalid);

		public static readonly CSteamID NotInitYetGS = new CSteamID(new AccountID_t(1u), 0u, EUniverse.k_EUniverseInvalid, EAccountType.k_EAccountTypeInvalid);

		public static readonly CSteamID NonSteamGS = new CSteamID(new AccountID_t(2u), 0u, EUniverse.k_EUniverseInvalid, EAccountType.k_EAccountTypeInvalid);

		public ulong m_SteamID;

		public CSteamID(AccountID_t unAccountID, EUniverse eUniverse, EAccountType eAccountType)
		{
			m_SteamID = 0uL;
			Set(unAccountID, eUniverse, eAccountType);
		}

		public CSteamID(AccountID_t unAccountID, uint unAccountInstance, EUniverse eUniverse, EAccountType eAccountType)
		{
			m_SteamID = 0uL;
			InstancedSet(unAccountID, unAccountInstance, eUniverse, eAccountType);
		}

		public CSteamID(ulong ulSteamID)
		{
			m_SteamID = ulSteamID;
		}

		public void Set(AccountID_t unAccountID, EUniverse eUniverse, EAccountType eAccountType)
		{
			SetAccountID(unAccountID);
			SetEUniverse(eUniverse);
			SetEAccountType(eAccountType);
			if (eAccountType == EAccountType.k_EAccountTypeClan || eAccountType == EAccountType.k_EAccountTypeGameServer)
			{
				SetAccountInstance(0u);
			}
			else
			{
				SetAccountInstance(1u);
			}
		}

		public void InstancedSet(AccountID_t unAccountID, uint unInstance, EUniverse eUniverse, EAccountType eAccountType)
		{
			SetAccountID(unAccountID);
			SetEUniverse(eUniverse);
			SetEAccountType(eAccountType);
			SetAccountInstance(unInstance);
		}

		public void Clear()
		{
			m_SteamID = 0uL;
		}

		public void CreateBlankAnonLogon(EUniverse eUniverse)
		{
			SetAccountID(new AccountID_t(0u));
			SetEUniverse(eUniverse);
			SetEAccountType(EAccountType.k_EAccountTypeAnonGameServer);
			SetAccountInstance(0u);
		}

		public void CreateBlankAnonUserLogon(EUniverse eUniverse)
		{
			SetAccountID(new AccountID_t(0u));
			SetEUniverse(eUniverse);
			SetEAccountType(EAccountType.k_EAccountTypeAnonUser);
			SetAccountInstance(0u);
		}

		public bool BBlankAnonAccount()
		{
			if (GetAccountID() == new AccountID_t(0u) && BAnonAccount())
			{
				return GetUnAccountInstance() == 0;
			}
			return false;
		}

		public bool BGameServerAccount()
		{
			if (GetEAccountType() != EAccountType.k_EAccountTypeGameServer)
			{
				return GetEAccountType() == EAccountType.k_EAccountTypeAnonGameServer;
			}
			return true;
		}

		public bool BPersistentGameServerAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeGameServer;
		}

		public bool BAnonGameServerAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeAnonGameServer;
		}

		public bool BContentServerAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeContentServer;
		}

		public bool BClanAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeClan;
		}

		public bool BChatAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeChat;
		}

		public bool IsLobby()
		{
			if (GetEAccountType() == EAccountType.k_EAccountTypeChat)
			{
				return (GetUnAccountInstance() & 0x40000) != 0;
			}
			return false;
		}

		public bool BIndividualAccount()
		{
			if (GetEAccountType() != EAccountType.k_EAccountTypeIndividual)
			{
				return GetEAccountType() == EAccountType.k_EAccountTypeConsoleUser;
			}
			return true;
		}

		public bool BAnonAccount()
		{
			if (GetEAccountType() != EAccountType.k_EAccountTypeAnonUser)
			{
				return GetEAccountType() == EAccountType.k_EAccountTypeAnonGameServer;
			}
			return true;
		}

		public bool BAnonUserAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeAnonUser;
		}

		public bool BConsoleUserAccount()
		{
			return GetEAccountType() == EAccountType.k_EAccountTypeConsoleUser;
		}

		public void SetAccountID(AccountID_t other)
		{
			m_SteamID = (ulong)(((long)m_SteamID & -4294967296L) | ((long)(uint)other & 4294967295L));
		}

		public void SetAccountInstance(uint other)
		{
			m_SteamID = (ulong)(((long)m_SteamID & -4503595332403201L) | (((long)other & 1048575L) << 32));
		}

		public void SetEAccountType(EAccountType other)
		{
			m_SteamID = (ulong)(((long)m_SteamID & -67553994410557441L) | (((long)other & 15L) << 52));
		}

		public void SetEUniverse(EUniverse other)
		{
			m_SteamID = (ulong)((long)(m_SteamID & 0xFFFFFFFFFFFFFF) | (((long)other & 255L) << 56));
		}

		public void ClearIndividualInstance()
		{
			if (BIndividualAccount())
			{
				SetAccountInstance(0u);
			}
		}

		public bool HasNoIndividualInstance()
		{
			if (BIndividualAccount())
			{
				return GetUnAccountInstance() == 0;
			}
			return false;
		}

		public AccountID_t GetAccountID()
		{
			return new AccountID_t((uint)(m_SteamID & uint.MaxValue));
		}

		public uint GetUnAccountInstance()
		{
			return (uint)((m_SteamID >> 32) & 0xFFFFF);
		}

		public EAccountType GetEAccountType()
		{
			return (EAccountType)((m_SteamID >> 52) & 0xF);
		}

		public EUniverse GetEUniverse()
		{
			return (EUniverse)((m_SteamID >> 56) & 0xFF);
		}

		public bool IsValid()
		{
			if (GetEAccountType() <= EAccountType.k_EAccountTypeInvalid || GetEAccountType() >= EAccountType.k_EAccountTypeMax)
			{
				return false;
			}
			if (GetEUniverse() <= EUniverse.k_EUniverseInvalid || GetEUniverse() >= EUniverse.k_EUniverseMax)
			{
				return false;
			}
			if (GetEAccountType() == EAccountType.k_EAccountTypeIndividual && (GetAccountID() == new AccountID_t(0u) || GetUnAccountInstance() > 4))
			{
				return false;
			}
			if (GetEAccountType() == EAccountType.k_EAccountTypeClan && (GetAccountID() == new AccountID_t(0u) || GetUnAccountInstance() != 0))
			{
				return false;
			}
			if (GetEAccountType() == EAccountType.k_EAccountTypeGameServer && GetAccountID() == new AccountID_t(0u))
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			return m_SteamID.ToString();
		}

		public override bool Equals(object other)
		{
			if (other is CSteamID)
			{
				return this == (CSteamID)other;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return m_SteamID.GetHashCode();
		}

		public static bool operator ==(CSteamID x, CSteamID y)
		{
			return x.m_SteamID == y.m_SteamID;
		}

		public static bool operator !=(CSteamID x, CSteamID y)
		{
			return !(x == y);
		}

		public static explicit operator CSteamID(ulong value)
		{
			return new CSteamID(value);
		}

		public static explicit operator ulong(CSteamID that)
		{
			return that.m_SteamID;
		}

		public bool Equals(CSteamID other)
		{
			return m_SteamID == other.m_SteamID;
		}

		public int CompareTo(CSteamID other)
		{
			return m_SteamID.CompareTo(other.m_SteamID);
		}
	}
}