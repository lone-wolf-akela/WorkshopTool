using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WorkshopTextBox;

namespace WorkshopTool
{
	public class WorkshopTool : Form
	{
		private string[] VisibilityTypes = new string[3]
		{
			"Public",
			"Friends Only",
			"Private"
		};

		private string[] CmdlineArgs;

		private bool bDontDeleteTempFiles;

		private AppId_t Workshop_AppId;

		private AppId_t Game_AppId;

		private System.Threading.Timer SteamCallbackTimer;

		private bool bIsUploadInProgress;

		private DateTime previousTimer;

		private CSteamID steamId;

		private AccountID_t accountId;

		private Process process;

		private bool bProcesOutputDone;

		private string MODFolder;

		private string TempFolder;

		private bool bIsExecutingCreateCommand;

		private bool bIsExecutingUpdateCommand;

		private List<ArchiveCommandStruct> ArchiveCommands;

		private int archive_index;

		private PublishedFileId_t updateItemFileId;

		public static WorkshopTool WorkshopToolForm;

		private IContainer components;

		private WorkshopTextBox.WorkshopTextBox workshopTextBox1;

		public WorkshopTool(string[] args)
		{
			WorkshopToolForm = this;
			CmdlineArgs = args;
			InitializeComponent();
			bIsUploadInProgress = false;
			previousTimer = DateTime.Now;
			Workshop_AppId = new AppId_t(347380u);
			Game_AppId = new AppId_t(244160u);
			CSteamInterface.Init();
			bIsExecutingCreateCommand = false;
			bIsExecutingUpdateCommand = false;
			AutoResetEvent autoEvent = new AutoResetEvent(false);
			SteamCallbackTimer = new System.Threading.Timer(RunSteamCallbacks, autoEvent, 10, 10);
		}

		private void WorkshopTool_Load(object sender, EventArgs e)
		{
			steamId = SteamUser.GetSteamID();
			accountId = steamId.GetAccountID();
			for (int index = 0; index < CmdlineArgs.Count(); index++)
			{
				if (CmdlineArgs[index] == "-nodelete")
				{
					bDontDeleteTempFiles = true;
				}
			}
			workshopTextBox1.PrintLine("Use 'quit' or 'exit' to exit WorkshopTool.  Use 'help' to get help.");
			workshopTextBox1.PrintPrompt();
		}

		private void WorkshopTool_FormClosing(object sender, FormClosingEventArgs e)
		{
			SteamCallbackTimer.Dispose();
			SteamAPI.Shutdown();
		}

		private void RunSteamCallbacks(object stateInfo)
		{
			SteamAPI.RunCallbacks();
			if (bIsUploadInProgress && DateTime.Now.Subtract(previousTimer).Milliseconds > 100)
			{
				ulong bytesProcessed;
				ulong bytesTotal;
				CSteamInterface.GetUpdateItemProgress(out bytesProcessed, out bytesTotal);
				if (bytesTotal != 0)
				{
					BeginInvoke((MethodInvoker)delegate
					{
						workshopTextBox1.PrintLineAtLineStart(string.Format("  Uploaded {0:n0} of {1:n0} bytes", bytesProcessed, bytesTotal));
					});
				}
				previousTimer = DateTime.Now;
			}
		}

		private string[] GetCommandArguments(string command)
		{
			char[] char_array = command.ToCharArray();
			bool bInsideQuote = false;
			for (int j = 0; j < char_array.Length; j++)
			{
				if (char_array[j] == '"')
				{
					bInsideQuote = !bInsideQuote;
				}
				else if (!bInsideQuote && char_array[j] == ' ')
				{
					char_array[j] = '\n';
				}
			}
			string[] args = new string(char_array).Split('\n');
			List<string> args_List = new List<string>(args);
			for (int i = args_List.Count - 1; i >= 0; i--)
			{
				if (args_List[i] == "")
				{
					args_List.RemoveAt(i);
				}
				else if (args_List[i][0] == '"')
				{
					string quoted_string2 = args_List[i].Substring(1);
					int len = quoted_string2.Length;
					if (quoted_string2[len - 1] == '"')
					{
						quoted_string2 = quoted_string2.Substring(0, len - 1);
					}
					quoted_string2 = (args_List[i] = quoted_string2.Trim());
				}
			}
			return args_List.ToArray();
		}

		private void RunArchiveForArchiveIndex()
		{
			process = new Process();
			process.StartInfo.FileName = "LoneWolf_Archiver.exe";
			process.StartInfo.Arguments = ArchiveCommands[archive_index].arg_string;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += OutputHandler;
			process.EnableRaisingEvents = true;
			process.Exited += ProcessExited;
			process.Start();
			process.BeginOutputReadLine();
		}

		private bool RunArchiveOnModFolder(string sourceFolder, string bigFilename)
		{
			ArchiveCommands = new List<ArchiveCommandStruct>();
			if (CArchive.CreateBuildFile(CConfig.ModType, ref ArchiveCommands, TempFolder, sourceFolder, bigFilename))
			{
				if (ArchiveCommands.Count > 0)
				{
					bProcesOutputDone = false;
					archive_index = 0;
					RunArchiveForArchiveIndex();
					return true;
				}
			}
			else
			{
				workshopTextBox1.PrintLine(string.Format("  {0}", CArchive.ArchiveErrorMessage));
			}
			return false;
		}

		private void ProcessArchiveExitStatus(int exitCode)
		{
			if (exitCode == -1)
			{
				workshopTextBox1.PrintNewLine();
				workshopTextBox1.PrintLine(string.Format("  Archive.exe failed."));
				if (!bDontDeleteTempFiles)
				{
					CConfig.DeleteDirectory(TempFolder);
				}
				workshopTextBox1.PrintPrompt();
			}
			else if (bIsExecutingCreateCommand)
			{
				if (!bDontDeleteTempFiles)
				{
					File.Delete(ArchiveCommands[archive_index].buildfilename);
				}
				archive_index++;
				if (archive_index < ArchiveCommands.Count)
				{
					RunArchiveForArchiveIndex();
				}
				else
				{
					workshopTextBox1.PrintNewLine();
					workshopTextBox1.PrintLine(string.Format("Creating new item on Steam Workshop..."));
					CSteamInterface.CreateItem(Game_AppId, ItemCreated);
				}
			}
			else
			{
				if (bIsExecutingUpdateCommand)
				{
					if (!bDontDeleteTempFiles)
					{
						File.Delete(ArchiveCommands[archive_index].buildfilename);
					}
					archive_index++;
					if (archive_index < ArchiveCommands.Count)
					{
						RunArchiveForArchiveIndex();
						return;
					}
					workshopTextBox1.PrintNewLine();
					workshopTextBox1.PrintLine(string.Format("Updating item on Steam Workshop..."));
					if (CSteamInterface.UpdateItem(Game_AppId, updateItemFileId, false, TempFolder, CConfig.previewImageFilename, CConfig.title, CConfig.tags, CConfig.description, ItemUpdated))
					{
						bIsUploadInProgress = true;
						return;
					}
					workshopTextBox1.PrintLine(string.Format("  Steam update item failed: {0}", CSteamInterface.ErrorMessage));
					if (!bDontDeleteTempFiles)
					{
						CConfig.DeleteDirectory(TempFolder);
					}
				}
				workshopTextBox1.PrintPrompt();
			}
		}

		private void WorkshopTextBox_CommandEntered(object sender, EventArgs e)
		{
			Console.WriteLine("cmd = {0}", workshopTextBox1.Command);
			string command = workshopTextBox1.Command;
			string[] args = GetCommandArguments(command);
			bIsExecutingCreateCommand = false;
			bIsExecutingUpdateCommand = false;
			if (args != null && args.Length != 0)
			{
				switch (args[0].ToLower())
				{
				case "quit":
				case "exit":
					Application.Exit();
					return;
				case "help":
					workshopTextBox1.PrintLine("help:");
					workshopTextBox1.PrintLine("  Use 'quit' or 'exit' to exit WorkshopTool.");
					workshopTextBox1.PrintLine("  Use 'list' to print out a list of Workshop items that you have uploaded to Workshop.");
					workshopTextBox1.PrintLine("  Use 'create <foldername>' to create and upload a new Workshop item.");
					workshopTextBox1.PrintLine("  Use 'update <foldername>' to update an existing Workshop item.");
					break;
				case "list":
					CSteamInterface.GetPublishedItems(accountId, Workshop_AppId, Game_AppId, PublishedItems);
					return;
				case "create":
					if (args.Length > 1)
					{
						MODFolder = args[1];
						MODFolder = MODFolder.Trim();
						MODFolder = MODFolder.TrimEnd('\\');
						if (!CConfig.ValidateFolderContents(MODFolder))
						{
							workshopTextBox1.PrintLine(string.Format("  {0}", CConfig.ValidationErrorMessage));
						}
						else
						{
							workshopTextBox1.PrintNewLine();
							workshopTextBox1.PrintLine(string.Format("Using the following settings from the config.txt file in MOD folder '{0}'", MODFolder));
							workshopTextBox1.PrintLine(string.Format("  Title: {0}", CConfig.title));
							workshopTextBox1.PrintLine(string.Format("  Tags: {0}", CConfig.tags));
							workshopTextBox1.PrintLine(string.Format("  BigFilename: {0}.big", CConfig.bigfilename));
							bIsExecutingCreateCommand = true;
							TempFolder = CArchive.CreateArchiveTempFolder();
							if (TempFolder != null)
							{
								if (bDontDeleteTempFiles)
								{
									workshopTextBox1.PrintNewLine();
									workshopTextBox1.PrintLine(string.Format("TEMP folder is: '{0}'", TempFolder));
								}
								CConfig.CopyConfigFilesToFolder(TempFolder);
								workshopTextBox1.PrintNewLine();
								workshopTextBox1.PrintLine(string.Format("Running Archive.exe on MOD folder..."));
								if (RunArchiveOnModFolder(MODFolder, CConfig.bigfilename))
								{
									return;
								}
								workshopTextBox1.PrintLine("  There was a problem creating the .big file(s) for your mod.  You may need to set the WorkshopTool to 'Run this program as an Administrator'");
							}
							else
							{
								workshopTextBox1.PrintLine(string.Format("  {0}", CArchive.ArchiveErrorMessage));
							}
						}
					}
					else
					{
						workshopTextBox1.PrintLine("  Usage: 'create <foldername>' - You must provide the folder name that contains your MOD files.");
					}
					break;
				case "update":
					if (args.Length > 1)
					{
						MODFolder = args[1];
						MODFolder = MODFolder.Trim();
						MODFolder = MODFolder.TrimEnd('\\');
						if (!CConfig.ValidateFolderContents(MODFolder))
						{
							workshopTextBox1.PrintLine(string.Format("  {0}", CConfig.ValidationErrorMessage));
						}
						else
						{
							if (CConfig.workshopId != 0)
							{
								bIsExecutingUpdateCommand = true;
								workshopTextBox1.PrintNewLine();
								workshopTextBox1.PrintLine("Retrieving list of published Workshop items...");
								CSteamInterface.GetPublishedItems(accountId, Workshop_AppId, Game_AppId, PublishedItems);
								return;
							}
							workshopTextBox1.PrintLine(string.Format("  The Workshop item in the folder '{0}' doesn't have a valid Steam Workshop ID number.  You need to use the 'create' command to create the Workshop item first before you can update it.", MODFolder));
						}
					}
					else
					{
						workshopTextBox1.PrintLine("  Usage: 'update <foldername>' - You must provide the folder name that contains your MOD files.");
					}
					break;
				default:
					workshopTextBox1.PrintLine(string.Format("  unknown command: {0}", command));
					break;
				}
			}
			workshopTextBox1.PrintPrompt();
		}

		private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if (outLine.Data != null)
			{
				WorkshopToolForm.Invoke((Action)delegate
				{
					WorkshopToolForm.workshopTextBox1.PrintLine(outLine.Data);
				});
			}
			else
			{
				WorkshopToolForm.bProcesOutputDone = true;
			}
		}

		private static void ProcessExited(object sender, EventArgs e)
		{
			while (!WorkshopToolForm.bProcesOutputDone)
			{
				Thread.Sleep(100);
			}
			int exitCode = WorkshopToolForm.process.ExitCode;
			WorkshopToolForm.BeginInvoke((Action)delegate
			{
				WorkshopToolForm.ProcessArchiveExitStatus(exitCode);
			});
		}

		private void PublishedItems(EResult Result)
		{
			if (Result == EResult.k_EResultOK)
			{
				if (CSteamInterface.publishedWorkshopItems == null || CSteamInterface.publishedWorkshopItems.Length == 0)
				{
					workshopTextBox1.PrintLine("  You don't have any items published to the Steam Workshop.");
				}
				else
				{
					bool bUpdateFoundWorkshopId = false;
					for (int index = 0; index < CSteamInterface.publishedWorkshopItems.Length; index++)
					{
						SteamUGCDetails_t item = CSteamInterface.publishedWorkshopItems[index];
						if (bIsExecutingUpdateCommand)
						{
							if ((ulong)item.m_nPublishedFileId == CConfig.workshopId)
							{
								bUpdateFoundWorkshopId = true;
								updateItemFileId = item.m_nPublishedFileId;
								workshopTextBox1.PrintNewLine();
								workshopTextBox1.PrintLine(string.Format("Updating Workshop item number: '{0}'", item.m_nPublishedFileId));
								workshopTextBox1.PrintLine(string.Format("Title: {0}", CConfig.title));
								TempFolder = CArchive.CreateArchiveTempFolder();
								CConfig.CopyConfigFilesToFolder(TempFolder);
								workshopTextBox1.PrintNewLine();
								workshopTextBox1.PrintLine(string.Format("Running Archive.exe on MOD folder..."));
								if (RunArchiveOnModFolder(MODFolder, CConfig.bigfilename))
								{
									return;
								}
								workshopTextBox1.PrintLine("  There was a problem creating the .big file(s) for your mod.  You may need to set the WorkshopTool to 'Run this program as an Administrator'");
							}
						}
						else
						{
							workshopTextBox1.PrintLine(string.Format("  Item # {0}", index + 1));
							if (item.m_eResult == EResult.k_EResultOK)
							{
								workshopTextBox1.PrintLine(string.Format("    Title = {0}", item.m_rgchTitle));
								workshopTextBox1.PrintLine(string.Format("    Tags = {0}", item.m_rgchTags));
								workshopTextBox1.PrintLine(string.Format("    Visibility = {0}", VisibilityTypes[(int)item.m_eVisibility]));
								workshopTextBox1.PrintLine(string.Format("    Banned = {0}", item.m_bBanned ? "True" : "False"));
								workshopTextBox1.PrintLine(string.Format("    Published Workshop Id = {0}", item.m_nPublishedFileId.m_PublishedFileId));
								workshopTextBox1.PrintLine(string.Format("    Upvotes / Downvotes = {0} / {1}", item.m_unVotesUp, item.m_unVotesDown));
							}
							else
							{
								workshopTextBox1.PrintLine("    This item is in an invalid state.  You might want to delete it then create it again.");
							}
						}
					}
					if (!bUpdateFoundWorkshopId && bIsExecutingUpdateCommand)
					{
						workshopTextBox1.PrintNewLine();
						workshopTextBox1.PrintLine(string.Format("Could not find Workshop item number '{0}' from your MOD folder's config.txt file in list of published items.  Make sure that you have not modified the WorkshopID number in the config.txt file.", CConfig.workshopId));
						workshopTextBox1.PrintLine(string.Format("Use the 'list' command to get a list of items that you have published to the Steam Workshop and make sure that the WorkshopID number in your config.txt file matches one of those items."));
						workshopTextBox1.PrintNewLine();
						workshopTextBox1.PrintLine(string.Format("You may have to delete your item on the Steam Workshop web page and create it again (or manually fix the WorkshopID number in the config.txt file)."));
						workshopTextBox1.PrintNewLine();
					}
				}
			}
			else
			{
				workshopTextBox1.PrintLine(string.Format("  Steam error getting published Workshop items: Error code = {0}", Result));
			}
			workshopTextBox1.PrintPrompt();
		}

		private void ItemCreated(EResult Result, bool bNeedsToAcceptWorkshopLegalAgreement, PublishedFileId_t NewItemCreated)
		{
			if (Result == EResult.k_EResultOK)
			{
				workshopTextBox1.PrintLine(string.Format("  Workshop item created.  Item Id = {0}", NewItemCreated.m_PublishedFileId));
				CConfig.UpdateWorkshopItemNumber(NewItemCreated.m_PublishedFileId);
				if (bNeedsToAcceptWorkshopLegalAgreement)
				{
					workshopTextBox1.PrintNewLine();
					workshopTextBox1.PrintLine("You will need to agree to the Steam Workshop Terms of Service agreement at:");
					workshopTextBox1.PrintNewLine();
					workshopTextBox1.PrintLine("http://steamcommunity.com/sharedfiles/workshoplegalagreement");
					workshopTextBox1.PrintNewLine();
					workshopTextBox1.PrintLine("...before this Workshop item can be updated or set to 'public' visibility.");
					workshopTextBox1.PrintLine("After agreeing to the Terms of Service agreement, you will need to use the 'update' command to update this Workshop item.");
					ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", "http://steamcommunity.com/sharedfiles/workshoplegalagreement");
					Process.Start(startInfo);
					if (!bDontDeleteTempFiles)
					{
						CConfig.DeleteDirectory(TempFolder);
					}
					workshopTextBox1.PrintPrompt();
					return;
				}
				if (CSteamInterface.UpdateItem(Game_AppId, NewItemCreated, true, TempFolder, CConfig.previewImageFilename, CConfig.title, CConfig.tags, CConfig.description, ItemUpdated))
				{
					bIsUploadInProgress = true;
					return;
				}
				workshopTextBox1.PrintLine(string.Format("  Steam update item failed: {0}", CSteamInterface.ErrorMessage));
				workshopTextBox1.PrintLine("  You should manually update the item again or delete the item from the Steam Workshop web page.");
				if (!bDontDeleteTempFiles)
				{
					CConfig.DeleteDirectory(TempFolder);
				}
			}
			else
			{
				workshopTextBox1.PrintLine(string.Format("  Steam error creating item: Error code = {0}", Result));
				if (!bDontDeleteTempFiles)
				{
					CConfig.DeleteDirectory(TempFolder);
				}
			}
			workshopTextBox1.PrintPrompt();
		}

		private void ItemUpdated(EResult Result)
		{
			bIsUploadInProgress = false;
			ulong bytesProcessed;
			ulong bytesTotal;
			CSteamInterface.GetUpdateItemProgress(out bytesProcessed, out bytesTotal);
			if (bytesTotal != 0)
			{
				workshopTextBox1.PrintLineAtLineStart(string.Format("  Uploaded {0:n0} of {1:n0} bytes", bytesProcessed, bytesTotal));
			}
			workshopTextBox1.PrintNewLine();
			if (Result == EResult.k_EResultOK)
			{
				workshopTextBox1.PrintLine("  Workshop item updated.");
			}
			else
			{
				workshopTextBox1.PrintLine(string.Format("  Steam error updating item: Error code = {0}", Result));
			}
			workshopTextBox1.PrintNewLine();
			if (!bDontDeleteTempFiles)
			{
				CConfig.DeleteDirectory(TempFolder);
			}
			workshopTextBox1.PrintPrompt();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkshopTool));
			workshopTextBox1 = new WorkshopTextBox.WorkshopTextBox();
			SuspendLayout();
			workshopTextBox1.AcceptsReturn = true;
			workshopTextBox1.BackColor = System.Drawing.Color.White;
			workshopTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			workshopTextBox1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
			workshopTextBox1.ForeColor = System.Drawing.Color.Black;
			workshopTextBox1.Location = new System.Drawing.Point(0, 0);
			workshopTextBox1.MaxLength = 0;
			workshopTextBox1.Multiline = true;
			workshopTextBox1.Name = "workshopTextBox1";
			workshopTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			workshopTextBox1.Size = new System.Drawing.Size(944, 442);
			workshopTextBox1.TabIndex = 0;
			workshopTextBox1.CommandEntered += new System.EventHandler(WorkshopTextBox_CommandEntered);
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(944, 442);
			base.Controls.Add(workshopTextBox1);
			base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
			base.Name = "WorkshopTool";
			Text = "WorkshopTool";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(WorkshopTool_FormClosing);
			base.Load += new System.EventHandler(WorkshopTool_Load);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
