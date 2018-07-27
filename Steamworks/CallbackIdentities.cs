using System;

namespace Steamworks
{
	internal class CallbackIdentities
	{
		public static int GetCallbackIdentity(Type callbackStruct)
		{
			object[] customAttributes = callbackStruct.GetCustomAttributes(typeof(CallbackIdentityAttribute), false);
			int num = 0;
			if (num >= customAttributes.Length)
			{
				throw new Exception("Callback number not found for struct " + callbackStruct);
			}
			CallbackIdentityAttribute attribute = (CallbackIdentityAttribute)customAttributes[num];
			return attribute.Identity;
		}
	}
}
