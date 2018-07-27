using Steamworks;
using System;
using System.Collections.Generic;

namespace WorkshopTool
{
	public class CSteamInterface
	{
		public static string ErrorMessage;

		public static SteamUGCDetails_t[] publishedWorkshopItems;

		public static PublishedFileId_t[] subscribedItems;

		public static SteamUGCDetails_t[] subscribedItemDetails;

		public static uint numSubscribedItems;

		public static int subscribedItemIndex;

		private static UGCUpdateHandle_t hUpdate;

		private static CallResult<SteamUGCQueryCompleted_t> m_SteamUGCQueryCompletedCallback;

		private static CallResult<CreateItemResult_t> m_CreateItemResultCallback;

		private static CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResultCallback;

		private static CallResult<SteamUGCRequestUGCDetailsResult_t> m_SteamUGCRequestUGCDetailsResultCallback;

		private static GetPublishedItemsDelegate PublishedItems_delegate;

		private static CreateItemDelegate CreateItem_delegate;

		private static UpdateItemDelegate UpdateItem_delegate;

		public static void Init()
		{
			publishedWorkshopItems = null;
			subscribedItems = null;
			subscribedItemDetails = null;
			hUpdate = UGCUpdateHandle_t.Invalid;
			m_SteamUGCQueryCompletedCallback = CallResult<SteamUGCQueryCompleted_t>.Create(OnUGCQueryCompleted);
			m_CreateItemResultCallback = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
			m_SubmitItemUpdateResultCallback = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
		}

		public static void GetPublishedItems(AccountID_t accountId, AppId_t WorkshopAppId, AppId_t GameAppId, GetPublishedItemsDelegate InPublishedItemsDelegate)
		{
			PublishedItems_delegate = InPublishedItemsDelegate;
			uint nPage = 1u;
			UGCQueryHandle_t hQuery = SteamUGC.CreateQueryUserUGCRequest(accountId, EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderDesc, WorkshopAppId, GameAppId, nPage);
			SteamAPICall_t hSteamAPICall = SteamUGC.SendQueryUGCRequest(hQuery);
			if (hSteamAPICall != SteamAPICall_t.Invalid)
			{
				m_SteamUGCQueryCompletedCallback.Set(hSteamAPICall, null);
			}
			else
			{
				PublishedItems_delegate(EResult.k_EResultFail);
			}
		}

		public static void CreateItem(AppId_t GameAppId, CreateItemDelegate InCreateItemDelegate)
		{
			CreateItem_delegate = InCreateItemDelegate;
			SteamAPICall_t hSteamAPICall = SteamUGC.CreateItem(GameAppId, EWorkshopFileType.k_EWorkshopFileTypeFirst);
			if (hSteamAPICall != SteamAPICall_t.Invalid)
			{
				m_CreateItemResultCallback.Set(hSteamAPICall, null);
			}
			else
			{
				CreateItem_delegate(EResult.k_EResultFail, false, default(PublishedFileId_t));
			}
		}

		public static bool UpdateItem(AppId_t GameAppId, PublishedFileId_t ItemToUpdate, bool bIsNewItem, string foldername, string previewfilename, string title, string tags, string description, UpdateItemDelegate InUpdateItemDelegate)
		{
			UpdateItem_delegate = InUpdateItemDelegate;
			hUpdate = SteamUGC.StartItemUpdate(GameAppId, ItemToUpdate);
			if (title == "")
			{
				ErrorMessage = "No title was provided.";
				return false;
			}
			if (tags == "")
			{
				ErrorMessage = "No tags were provided.";
				return false;
			}
			if (description == "")
			{
				ErrorMessage = "No description was provided.";
				return false;
			}
			string[] tags_array = tags.Split(' ');
			List<string> tags_list = new List<string>(tags_array);
			SteamUGC.SetItemTitle(hUpdate, title);
			SteamUGC.SetItemTags(hUpdate, tags_list);
			SteamUGC.SetItemDescription(hUpdate, description);
			if (bIsNewItem)
			{
				SteamUGC.SetItemVisibility(hUpdate, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate);
			}
			SteamUGC.SetItemContent(hUpdate, foldername);
			SteamUGC.SetItemPreview(hUpdate, previewfilename);
			SteamAPICall_t hSteamAPICall = SteamUGC.SubmitItemUpdate(hUpdate, "");
			if (hSteamAPICall != SteamAPICall_t.Invalid)
			{
				m_SubmitItemUpdateResultCallback.Set(hSteamAPICall, null);
			}
			else
			{
				UpdateItem_delegate(EResult.k_EResultFail);
			}
			return true;
		}

		public static EItemUpdateStatus GetUpdateItemProgress(out ulong OutBytesProcessed, out ulong OutBytesTotal)
		{
			EItemUpdateStatus status = EItemUpdateStatus.k_EItemUpdateStatusInvalid;
			OutBytesProcessed = 0uL;
			OutBytesTotal = 0uL;
			if (hUpdate != UGCUpdateHandle_t.Invalid)
			{
				status = SteamUGC.GetItemUpdateProgress(hUpdate, out OutBytesProcessed, out OutBytesTotal);
			}
			return status;
		}

		private static void OnUGCQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
		{
			publishedWorkshopItems = new SteamUGCDetails_t[pCallback.m_unNumResultsReturned];
			for (uint index = 0u; index < pCallback.m_unNumResultsReturned; index++)
			{
				SteamUGC.GetQueryUGCResult(pCallback.m_handle, index, out publishedWorkshopItems[index]);
			}
			SteamUGC.ReleaseQueryUGCRequest(pCallback.m_handle);
			WorkshopTool.WorkshopToolForm.BeginInvoke((Action)delegate
			{
				PublishedItems_delegate(pCallback.m_eResult);
			});
		}

		private static void OnCreateItemResult(CreateItemResult_t pCallback, bool bIOFailure)
		{
			WorkshopTool.WorkshopToolForm.BeginInvoke((Action)delegate
			{
				CreateItem_delegate(pCallback.m_eResult, pCallback.m_bUserNeedsToAcceptWorkshopLegalAgreement, pCallback.m_nPublishedFileId);
			});
		}

		private static void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIOFailure)
		{
			WorkshopTool.WorkshopToolForm.BeginInvoke((Action)delegate
			{
				UpdateItem_delegate(pCallback.m_eResult);
			});
		}
	}
}
