using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace WorkshopTool
{
	public class CConfig
	{
		public static string ValidationErrorMessage;

		private static string ConfigValidationErrorMessage;

		private static string PreviewImageValidationErrorMessage;

		private static int line_number;

		public static string fullFoldername;

		public static string configFilename;

		public static string previewImageFilename;

		public static string previewImagefile;

		public static string title;

		public static string bigfilename;

		public static string tags;

		public static string description;

		public static ulong workshopId;

		public static EModType ModType;

		private static List<string> GetListOfOptions(string input)
		{
			List<string> output = new List<string>();
			string[] input_split = input.Split(' ');
			string[] array = input_split;
			foreach (string s in array)
			{
				s.Trim();
				output.Add(s);
			}
			return output;
		}

		private static bool ValidateConfigFile(string filename)
		{
			bool bHasTitle = false;
			bool bHasTags = false;
			bool bHasGameType = false;
			bool bHasModType = false;
			bool bHasBigFilename = false;
			bool bHasWorkshopID = false;
			bool bHasDescription = false;
			bool bAreFollowingLinesDescription = false;
			title = "";
			tags = "";
			description = "";
			line_number = 0;
			try
			{
				File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);
			}
			catch
			{
				ConfigValidationErrorMessage = "Could not set the config.txt file to NOT be read-only.  Verify that you have permission to modify the config.txt file.";
				return false;
			}
			try
			{
				StreamReader sr = File.OpenText(filename);
				string input4;
				while ((input4 = sr.ReadLine()) != null)
				{
					line_number++;
					string input_trim = input4.Trim();
					if (!(input4 == "") && !(input_trim.Substring(0, 2) == "//"))
					{
						string input_lower = input_trim.ToLower();
						if (bAreFollowingLinesDescription)
						{
							description = description + input4 + '\n';
							bHasDescription = true;
						}
						else if (input_lower.StartsWith("title:"))
						{
							if (bHasTitle)
							{
								ConfigValidationErrorMessage = "Title appears more than once.  You should only provide a single title for the Workshop item.";
								sr.Close();
								return false;
							}
							input4 = input4.Trim();
							title = input4.Substring(6).Trim();
							if (title.Length > 128)
							{
								title = title.Substring(0, 128);
							}
							if (title != "")
							{
								bHasTitle = true;
							}
						}
						else if (input_lower.StartsWith("tags:"))
						{
							if (bHasTags)
							{
								ConfigValidationErrorMessage = "Tags appears more than once.  You should only provide a single set of tags for the Workshop item.";
								sr.Close();
								return false;
							}
							input4 = input4.Trim();
							tags = input4.Substring(5).Trim();
							if (tags != "")
							{
								bHasTags = true;
							}
						}
						else if (input_lower.StartsWith("gametype:"))
						{
							if (bHasGameType)
							{
								ConfigValidationErrorMessage = "GameType appears more than once.  You should only provide a single game type for the Workshop item.";
								sr.Close();
								return false;
							}
							string gametype = input_lower.Substring(9).Trim().ToLower();
							List<string> options = GetListOfOptions(gametype);
							for (int i = 0; i < options.Count; i++)
							{
								if (options[i] != "hw1classic" && options[i] != "hw2classic" && options[i] != "homeworldrm")
								{
									ConfigValidationErrorMessage = "Invalid GameType.  GameType must be any of 'HW1Classic', 'HW2Classic', or 'HomeworldRM' (without the single quotes) separated by spaces.";
									sr.Close();
									return false;
								}
							}
							if (options.Count > 0)
							{
								bHasGameType = true;
							}
						}
						else if (input_lower.StartsWith("modtype:"))
						{
							if (bHasModType)
							{
								ConfigValidationErrorMessage = "ModType appears more than once.  You should only provide a single mod type for the Workshop item.";
								sr.Close();
								return false;
							}
							string modtype = input_lower.Substring(9).Trim().ToLower();
							if (modtype != "mod" && modtype != "badges" && modtype != "locale" && modtype != "cursors")
							{
								ConfigValidationErrorMessage = "Invalid ModType.  ModType must be one of 'MOD', 'Badges', 'Locale' or 'Cursors' (without the single quotes).";
								sr.Close();
								return false;
							}
							ModType = EModType.MODTYPE_Unknown;
							if (modtype == "mod")
							{
								ModType = EModType.MODTYPE_Mod;
							}
							else if (modtype == "locale")
							{
								ModType = EModType.MODTYPE_Locale;
							}
							else if (modtype == "badges")
							{
								ModType = EModType.MODTYPE_Badges;
							}
							else if (modtype == "cursors")
							{
								ModType = EModType.MODTYPE_Cursors;
							}
							bHasModType = true;
						}
						else if (input_lower.StartsWith("bigfilename:"))
						{
							if (bHasBigFilename)
							{
								ConfigValidationErrorMessage = "BigFilename appears more than once.  You should only provide a single .big file name for the Workshop item.";
								sr.Close();
								return false;
							}
							input4 = input4.Trim();
							bigfilename = input4.Substring(12).Trim().Replace(" ", string.Empty);
							string bigfilename_lower = bigfilename.ToLower();
							if (bigfilename_lower == "mymodbigfile")
							{
								ConfigValidationErrorMessage = "BigFilename not changed (still using the default of 'MyModBigFile').  You should change BigFilename to be specific to your MOD.";
								sr.Close();
								return false;
							}
							if (bigfilename_lower.EndsWith(".big"))
							{
								bigfilename = bigfilename.Substring(0, bigfilename.Length - 4);
							}
							if (bigfilename != "")
							{
								bHasBigFilename = true;
							}
						}
						else if (input_lower.StartsWith("workshopid:"))
						{
							if (bHasWorkshopID)
							{
								ConfigValidationErrorMessage = "WorkshopID appears more than once.  There should only be a single WorkshopID line in the config file.";
								sr.Close();
								return false;
							}
							string workshopId_string = input_lower.Substring(11).Trim();
							if (!ulong.TryParse(workshopId_string, out workshopId))
							{
								ConfigValidationErrorMessage = "WorkshopID value is not a number.  The value for WorkshopID should be the ID number of the Workshop item you created or 0 if you haven't created the Workshop item yet.";
								sr.Close();
								return false;
							}
							bHasWorkshopID = true;
						}
						else if (input_lower.StartsWith("description:"))
						{
							bAreFollowingLinesDescription = true;
						}
						else if (!bAreFollowingLinesDescription)
						{
							ConfigValidationErrorMessage = string.Format("Unrecognized input: {0}", input_trim);
							sr.Close();
							return false;
						}
					}
				}
				sr.Close();
			}
			catch (Exception ex)
			{
				ConfigValidationErrorMessage = string.Format("Exeception reading config.txt file ({0})", ex.Message);
				return false;
			}
			line_number = 0;
			if (!bHasTitle)
			{
				ConfigValidationErrorMessage = "Title not found.  You must provide a title for the Workshop item.";
				return false;
			}
			if (!bHasTags)
			{
				ConfigValidationErrorMessage = "Tags not found.  You must provide at least one tag for the Workshop item.";
				return false;
			}
			if (!bHasGameType)
			{
				ConfigValidationErrorMessage = "GameType not found.  You must provide a game type (HW1Classic, HW2Classic, or HomeworldRM).";
				return false;
			}
			if (!bHasModType)
			{
				ConfigValidationErrorMessage = "ModType not found.  You must provide a ModType type (MOD, Locale, Badges, or Cursors).";
				return false;
			}
			if (!bHasBigFilename)
			{
				ConfigValidationErrorMessage = "BigFilename not found.  You must provide a name for your .big file for your MOD.";
				return false;
			}
			if (!bHasWorkshopID)
			{
				ConfigValidationErrorMessage = "WorkshopID not found.  Config.txt file must contain a WorkshopID line.";
				return false;
			}
			if (!bHasDescription)
			{
				ConfigValidationErrorMessage = "Description not found.  You must provide a description for the Workshop item.";
				return false;
			}
			return true;
		}

		private static bool ValidatePreviewImage(string filename)
		{
			try
			{
				Image img = Image.FromFile(filename);
				if (img.Width == 256 && img.Height == 256)
				{
					img.Dispose();
					return true;
				}
				if (img.Width == 512 && img.Height == 512)
				{
					img.Dispose();
					return true;
				}
				img.Dispose();
				PreviewImageValidationErrorMessage = "Image must be 256 x 256 or 512 x 512.";
				return false;
			}
			catch
			{
			}
			PreviewImageValidationErrorMessage = "Error loading preview image file.  Is it a valid .jpg or .png file?";
			return false;
		}

		public static bool ValidateFolderContents(string folder_name)
		{
			fullFoldername = folder_name;
			configFilename = "";
			previewImageFilename = "";
			if (!Directory.Exists(folder_name))
			{
				ValidationErrorMessage = string.Format("Folder name '{0}' does not exist.  Did you specify the full path name?", folder_name);
				return false;
			}
			fullFoldername = Path.GetFullPath(folder_name);
			configFilename = fullFoldername + "\\config.txt";
			if (!File.Exists(configFilename))
			{
				ValidationErrorMessage = string.Format("File 'config.txt' does not exist inside folder '{0}'.", folder_name);
				return false;
			}
			bool bPreviewImageNotFound = true;
			string previewimage_jpg_filename = fullFoldername + "\\preview.jpg";
			string previewimage_png_filename = fullFoldername + "\\preview.png";
			if (File.Exists(previewimage_jpg_filename))
			{
				bPreviewImageNotFound = false;
				previewImagefile = "preview.jpg";
				previewImageFilename = previewimage_jpg_filename;
			}
			if (File.Exists(previewimage_png_filename))
			{
				bPreviewImageNotFound = false;
				previewImagefile = "preview.png";
				previewImageFilename = previewimage_png_filename;
			}
			if (bPreviewImageNotFound)
			{
				ValidationErrorMessage = string.Format("File 'preview.jpg' or 'preview.png' does not exist inside folder '{0}'.", folder_name);
				return false;
			}
			if (!ValidateConfigFile(configFilename))
			{
				if (line_number <= 0)
				{
					string msg2 = ValidationErrorMessage = string.Format("Config.txt validation failed: {0}", ConfigValidationErrorMessage);
				}
				else
				{
					string msg3 = ValidationErrorMessage = string.Format("Config.txt validation failed (line number {0}): {1}", line_number, ConfigValidationErrorMessage);
				}
				return false;
			}
			if (!ValidatePreviewImage(previewImageFilename))
			{
				string msg = ValidationErrorMessage = string.Format("Preview image validation failed: {0}", PreviewImageValidationErrorMessage);
				return false;
			}
			return true;
		}

		public static void CopyConfigFilesToFolder(string folderName)
		{
			string dest_preview_filename = string.Format("{0}\\{1}", folderName, previewImagefile);
			File.Copy(previewImageFilename, dest_preview_filename, true);
			string dest_config_filename = string.Format("{0}\\config.txt", folderName);
			File.Copy(configFilename, dest_config_filename, true);
		}

		public static void UpdateWorkshopItemNumber(ulong itemNumber)
		{
			if (workshopId != itemNumber)
			{
				string tempFilename = Path.GetTempFileName();
				try
				{
					StreamWriter sw = new StreamWriter(tempFilename);
					File.SetAttributes(configFilename, File.GetAttributes(configFilename) & ~FileAttributes.ReadOnly);
					StreamReader sr = File.OpenText(configFilename);
					string input;
					while ((input = sr.ReadLine()) != null)
					{
						string input_lower = input.Trim().ToLower();
						if (input_lower.StartsWith("workshopid:"))
						{
							sw.WriteLine(string.Format("WorkshopID: {0}", itemNumber));
						}
						else
						{
							sw.WriteLine(input);
						}
					}
					sr.Close();
					sw.Close();
					File.Copy(tempFilename, configFilename, true);
					File.Delete(tempFilename);
				}
				catch
				{
				}
			}
		}

		public static void DeleteDirectory(string targetDir)
		{
			File.SetAttributes(targetDir, FileAttributes.Normal);
			string[] files = Directory.GetFiles(targetDir);
			string[] dirs = Directory.GetDirectories(targetDir);
			string[] array = files;
			foreach (string file in array)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}
			string[] array2 = dirs;
			foreach (string dir in array2)
			{
				DeleteDirectory(dir);
			}
			Directory.Delete(targetDir, false);
		}
	}
}
