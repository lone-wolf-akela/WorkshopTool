using Steamworks;

namespace WorkshopTool
{
	public delegate void CreateItemDelegate(EResult Result, bool bNeedsToAcceptWorkshopLegalAgreement, PublishedFileId_t NewItemCreated);
}
