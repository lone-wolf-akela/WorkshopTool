using System;
using System.Collections.Generic;
using System.IO;

namespace WorkshopTool
{
	public static class CArchive
	{
		private struct LocaleStruct
		{
			public string LocaleLanguage;

			public List<string> LocaleFileList;
		}

		public static string ArchiveErrorMessage;

		private static List<string> DirList;

		private static List<LocaleStruct> LocaleList;

		private static List<string> CursorsList;

		private static List<string> BadgesList;

		private static int nest_level;

		public static string CreateArchiveTempFolder()
		{
			string tempFolder = Path.GetTempFileName();
			File.Delete(tempFolder);
			Directory.CreateDirectory(tempFolder);
			if (!Directory.Exists(tempFolder))
			{
				ArchiveErrorMessage = string.Format("Can't create TEMP folder: '{0}'", tempFolder);
				return null;
			}
			return tempFolder;
		}

		private static void DirRecursive(string inDir, bool bInIsLocale)
		{
			nest_level++;
			try
			{
				string[] files = Directory.GetFiles(inDir);
				foreach (string file in files)
				{
					DirList.Add(file);
				}
			}
			catch
			{
			}
			try
			{
				if (!bInIsLocale)
				{
					string[] directories = Directory.GetDirectories(inDir);
					foreach (string dir in directories)
					{
						bool bIsLocale = nest_level == 1 && dir.ToLower().EndsWith("\\locale");
						DirRecursive(dir, bIsLocale);
					}
				}
			}
			catch
			{
			}
			nest_level--;
		}

		private static void DirLocaleRecursive(string inDir, int locale_index)
		{
			try
			{
				string[] files = Directory.GetFiles(inDir);
				foreach (string file in files)
				{
					LocaleList[locale_index].LocaleFileList.Add(file);
				}
			}
			catch
			{
			}
			try
			{
				string[] directories = Directory.GetDirectories(inDir);
				foreach (string dir in directories)
				{
					DirLocaleRecursive(dir, locale_index);
				}
			}
			catch
			{
			}
		}

		private static void DirCursorsRecursive(string inDir)
		{
			try
			{
				string[] files = Directory.GetFiles(inDir);
				foreach (string file in files)
				{
					CursorsList.Add(file);
				}
			}
			catch
			{
			}
			try
			{
				string[] directories = Directory.GetDirectories(inDir);
				foreach (string dir in directories)
				{
					DirCursorsRecursive(dir);
				}
			}
			catch
			{
			}
		}

		public static void GetFileList(string dir)
		{
			DirList = new List<string>();
			LocaleList = new List<LocaleStruct>();
			CursorsList = new List<string>();
			BadgesList = new List<string>();
			nest_level = 0;
			DirRecursive(dir, false);
			try
			{
				int locale_index = 0;
				string[] directories = Directory.GetDirectories(dir);
				foreach (string dir_temp2 in directories)
				{
					if (dir_temp2.ToLower().EndsWith("\\locale"))
					{
						string[] directories2 = Directory.GetDirectories(dir_temp2);
						foreach (string dir_language in directories2)
						{
							LocaleStruct locale_struct = default(LocaleStruct);
							locale_struct.LocaleLanguage = Path.GetFileName(dir_language);
							locale_struct.LocaleFileList = new List<string>();
							LocaleList.Add(locale_struct);
							DirLocaleRecursive(dir_language, locale_index);
							locale_index++;
						}
						break;
					}
				}
				string[] directories3 = Directory.GetDirectories(dir);
				foreach (string dir_temp in directories3)
				{
					if (dir_temp.ToLower().EndsWith("\\ui"))
					{
						string[] directories4 = Directory.GetDirectories(dir_temp);
						foreach (string ui_dir_temp in directories4)
						{
							if (ui_dir_temp.ToLower().EndsWith("\\cursors"))
							{
								DirCursorsRecursive(ui_dir_temp);
							}
						}
					}
				}
				string[] files = Directory.GetFiles(dir);
				foreach (string file in files)
				{
					if (file.EndsWith(".tga"))
					{
						BadgesList.Add(file);
					}
				}
			}
			catch
			{
			}
		}

		public static bool CreateBuildFile(EModType modtype, ref List<ArchiveCommandStruct> archive_commands, string tempFolder, string sourceFolder, string bigFileName)
		{
			if (!Directory.Exists(tempFolder))
			{
				ArchiveErrorMessage = string.Format("Can't access TEMP folder: '{0}'", tempFolder);
				return false;
			}
			if (!Directory.Exists(sourceFolder))
			{
				ArchiveErrorMessage = string.Format("Can't access MOD folder: '{0}'", sourceFolder);
				return false;
			}
			GetFileList(sourceFolder);
			if (DirList.Count == 0)
			{
				ArchiveErrorMessage = string.Format("No files were found in MOD folder: '{0}'", sourceFolder);
				return false;
			}
			string configFile = string.Format("{0}\\config.txt", sourceFolder);
			for (int k = 0; k < DirList.Count; k++)
			{
				if (string.Equals(DirList[k], configFile, StringComparison.OrdinalIgnoreCase))
				{
					DirList.RemoveAt(k);
					break;
				}
			}
			string jpgFile = string.Format("{0}\\preview.jpg", sourceFolder);
			for (int m = 0; m < DirList.Count; m++)
			{
				if (string.Equals(DirList[m], jpgFile, StringComparison.OrdinalIgnoreCase))
				{
					DirList.RemoveAt(m);
					break;
				}
			}
			string pngFile = string.Format("{0}\\preview.png", sourceFolder);
			for (int i2 = 0; i2 < DirList.Count; i2++)
			{
				if (string.Equals(DirList[i2], pngFile, StringComparison.OrdinalIgnoreCase))
				{
					DirList.RemoveAt(i2);
					break;
				}
			}
			if (modtype == EModType.MODTYPE_Mod)
			{
				string buildfileName4 = tempFolder + "\\buildfile.txt";
				try
				{
					using (StreamWriter streamWriter = new StreamWriter(buildfileName4))
					{
						streamWriter.WriteLine("Archive name=\"{0}\"", bigFileName);
						streamWriter.WriteLine("TOCStart name=\"TOC{0}\" alias=\"Data\" relativeroot=\"\"", bigFileName);
						streamWriter.WriteLine("FileSettingsStart defcompression=\"1\"");
						streamWriter.WriteLine("Override wildcard=\"*.*\" minsize=\"-1\" maxsize=\"100\" ct=\"0\"");
						streamWriter.WriteLine("Override wildcard=\"*.mp3\" minsize=\"-1\" maxsize=\"-1\" ct=\"0\"");
						streamWriter.WriteLine("Override wildcard=\"*.wav\" minsize=\"-1\" maxsize=\"-1\" ct=\"0\"");
						streamWriter.WriteLine("Override wildcard=\"*.jpg\" minsize=\"-1\" maxsize=\"-1\" ct=\"0\"");
						streamWriter.WriteLine("Override wildcard=\"*.lua\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.fda\" minsize=\"-1\" maxsize=\"-1\" ct=\"0\"");
						streamWriter.WriteLine("Override wildcard=\"*.txt\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.ship\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.resource\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.pebble\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.level\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.wepn\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.subs\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.miss\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.events\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.madstate\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.script\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.ti\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.st\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.vp\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("Override wildcard=\"*.wf\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter.WriteLine("SkipFile wildcard=\"Keeper.txt\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter.WriteLine("SkipFile wildcard=\"*.big\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter.WriteLine("SkipFile wildcard=\"*_.*\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter.WriteLine("FileSettingsEnd");
						for (int n = 0; n < DirList.Count; n++)
						{
							streamWriter.WriteLine("{0}", DirList[n]);
						}
						streamWriter.WriteLine("TOCEnd");
					}
					ArchiveCommandStruct command_struct = default(ArchiveCommandStruct);
					command_struct.arg_string = string.Format("-a \"{0}\\{1}.big\" -c \"{2}\" -r \"{3}\" -v", tempFolder, bigFileName, buildfileName4, sourceFolder);
					command_struct.buildfilename = buildfileName4;
					archive_commands.Add(command_struct);
				}
				catch (Exception ex4)
				{
					ArchiveErrorMessage = string.Format("Error writing to 'buildfile.txt' for main TOC in the TEMP folder! {0}", ex4.Message);
					return false;
				}
			}
			if (modtype == EModType.MODTYPE_Mod || modtype == EModType.MODTYPE_Locale)
			{
				for (int locale_index = 0; locale_index < LocaleList.Count; locale_index++)
				{
					string buildfileName = string.Format("{0}\\buildfile_locale_{1}.txt", tempFolder, locale_index);
					try
					{
						using (StreamWriter sw = new StreamWriter(buildfileName))
						{
							sw.WriteLine("Archive name=\"{0}{1}\"", bigFileName, LocaleList[locale_index].LocaleLanguage);
							sw.WriteLine("TOCStart name=\"TOC{0}{1}\" alias=\"Locale\" relativeroot=\"Locale\\{2}\"", bigFileName, LocaleList[locale_index].LocaleLanguage, LocaleList[locale_index].LocaleLanguage);
							sw.WriteLine("FileSettingsStart defcompression=\"1\"");
							sw.WriteLine("Override wildcard=\"*.*\" minsize=\"-1\" maxsize=\"100\" ct=\"0\"");
							sw.WriteLine("Override wildcard=\"*.lua\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
							sw.WriteLine("Override wildcard=\"*.dat\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
							sw.WriteLine("Override wildcard=\"*.ucs\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
							sw.WriteLine("SkipFile wildcard=\"*.mp3\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.wav\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.jpg\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.fda\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.ship\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.resource\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.pebble\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.level\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.wepn\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.subs\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.miss\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.events\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.madstate\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.script\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*.big\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("SkipFile wildcard=\"*_.*\" minsize=\"-1\" maxsize=\"-1\"");
							sw.WriteLine("FileSettingsEnd");
							for (int i = 0; i < LocaleList[locale_index].LocaleFileList.Count; i++)
							{
								sw.WriteLine("{0}", LocaleList[locale_index].LocaleFileList[i]);
							}
							sw.WriteLine("TOCEnd");
						}
						ArchiveCommandStruct command_struct2 = default(ArchiveCommandStruct);
						command_struct2.arg_string = string.Format("-a \"{0}\\{1}.big\" -c \"{2}\" -r \"{3}\" -v", tempFolder, LocaleList[locale_index].LocaleLanguage, buildfileName, sourceFolder);
						command_struct2.buildfilename = buildfileName;
						archive_commands.Add(command_struct2);
					}
					catch (Exception ex)
					{
						ArchiveErrorMessage = string.Format("Error writing to 'buildfile.txt' for Locale TOC in the TEMP folder! {0}", ex.Message);
						return false;
					}
				}
			}
			if (modtype == EModType.MODTYPE_Cursors)
			{
				string buildfileName3 = string.Format("{0}\\buildfile_cursors.txt", tempFolder);
				try
				{
					using (StreamWriter streamWriter2 = new StreamWriter(buildfileName3))
					{
						streamWriter2.WriteLine("Archive name=\"{0}\"", bigFileName);
						streamWriter2.WriteLine("TOCStart name=\"TOC{0}\" alias=\"Data\" relativeroot=\"\"", bigFileName);
						streamWriter2.WriteLine("FileSettingsStart defcompression=\"1\"");
						streamWriter2.WriteLine("Override wildcard=\"*.*\" minsize=\"-1\" maxsize=\"100\" ct=\"0\"");
						streamWriter2.WriteLine("Override wildcard=\"*.lua\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter2.WriteLine("Override wildcard=\"*.dds\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter2.WriteLine("Override wildcard=\"*.anim\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.mp3\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.wav\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.jpg\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.fda\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.ship\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.resource\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.pebble\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.level\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.wepn\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.subs\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.miss\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.events\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.madstate\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.script\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*.big\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("SkipFile wildcard=\"*_.*\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter2.WriteLine("FileSettingsEnd");
						for (int l = 0; l < CursorsList.Count; l++)
						{
							streamWriter2.WriteLine("{0}", CursorsList[l]);
						}
						streamWriter2.WriteLine("TOCEnd");
					}
					ArchiveCommandStruct command_struct3 = default(ArchiveCommandStruct);
					command_struct3.arg_string = string.Format("-a \"{0}\\{1}.big\" -c \"{2}\" -r \"{3}\" -v", tempFolder, bigFileName, buildfileName3, sourceFolder);
					command_struct3.buildfilename = buildfileName3;
					archive_commands.Add(command_struct3);
				}
				catch (Exception ex3)
				{
					ArchiveErrorMessage = string.Format("Error writing to 'buildfile.txt' for Cursors TOC in the TEMP folder! {0}", ex3.Message);
					return false;
				}
			}
			if (modtype == EModType.MODTYPE_Badges)
			{
				string buildfileName2 = string.Format("{0}\\buildfile_badges.txt", tempFolder);
				try
				{
					using (StreamWriter streamWriter3 = new StreamWriter(buildfileName2))
					{
						streamWriter3.WriteLine("Archive name=\"{0}\"", bigFileName);
						streamWriter3.WriteLine("TOCStart name=\"TOC{0}\" alias=\"Badges\" relativeroot=\"\"", bigFileName);
						streamWriter3.WriteLine("FileSettingsStart defcompression=\"1\"");
						streamWriter3.WriteLine("Override wildcard=\"*.*\" minsize=\"-1\" maxsize=\"100\" ct=\"0\"");
						streamWriter3.WriteLine("Override wildcard=\"*.tga\" minsize=\"-1\" maxsize=\"-1\" ct=\"1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.mp3\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.wav\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.jpg\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.fda\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.ship\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.resource\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.pebble\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.level\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.wepn\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.subs\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.miss\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.events\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.madstate\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.script\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.lua\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.dds\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.anim\" minsize=\"-1\" maxsize=\"-1\" ct=\"2\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*.big\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("SkipFile wildcard=\"*_.*\" minsize=\"-1\" maxsize=\"-1\"");
						streamWriter3.WriteLine("FileSettingsEnd");
						for (int j = 0; j < BadgesList.Count; j++)
						{
							streamWriter3.WriteLine("{0}", BadgesList[j]);
						}
						streamWriter3.WriteLine("TOCEnd");
					}
					ArchiveCommandStruct command_struct4 = default(ArchiveCommandStruct);
					command_struct4.arg_string = string.Format("-a \"{0}\\{1}.big\" -c \"{2}\" -r \"{3}\" -v", tempFolder, bigFileName, buildfileName2, sourceFolder);
					command_struct4.buildfilename = buildfileName2;
					archive_commands.Add(command_struct4);
				}
				catch (Exception ex2)
				{
					ArchiveErrorMessage = string.Format("Error writing to 'buildfile.txt' for Badges TOC in the TEMP folder! {0}", ex2.Message);
					return false;
				}
			}
			return true;
		}
	}
}
