using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;
using ThumbServer.My;

namespace ThumbServer
{
	[StandardModule]
	internal sealed class RenderServer
	{
		private enum ShowWindowEnum
		{
			Hide = 0,
			ShowNormal = 1,
			ShowMinimized = 2,
			ShowMaximized = 3,
			Maximize = 3,
			ShowNormalNoActivate = 4,
			Show = 5,
			Minimize = 6,
			ShowMinNoActivate = 7,
			ShowNoActivate = 8,
			Restore = 9,
			ShowDefault = 10,
			ForceMinimized = 11
		}

		private static int timesWrittenTo = 4;

		private static bool rendererLoaded = false;

		private static string consoleFile;

		private static int consoleLines = 1000;

		public const int SW_RESTORE = 9;

		public const int SW_SHOW = 5;

		public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
		{
			checked
			{
				int width2;
				int height2;
				if (preserveAspectRatio)
				{
					int width = image.Width;
					int height = image.Height;
					float num = (float)size.Width / (float)width;
					float num2 = (float)size.Height / (float)height;
					float num3 = ((num2 < num) ? num2 : num);
					width2 = (int)Math.Round((float)width * num3);
					height2 = (int)Math.Round((float)height * num3);
				}
				else
				{
					width2 = size.Width;
					height2 = size.Height;
				}
				Image image2 = new Bitmap(width2, height2);
				using (Graphics graphics = Graphics.FromImage(image2))
				{
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.DrawImage(image, 0, 0, width2, height2);
				}
				return image2;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		[STAThread]
		public static void Main()
		{
			string productVersion = Application.ProductVersion;
			Console.Title = "GoodBlox Render v" + productVersion + " (RevisedOTDbg 1.5.6.2)";
			writeLine("GoodBlox Render v" + productVersion + ":tm:");
			string text = Application.ExecutablePath.Replace(Application.StartupPath + "\\", null);
			bool flag = false;
			string text2 = "nan";
			if (File.Exists("update"))
			{
				flag = true;
				text2 = DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt");
				writeLine("Finishing update..");
				File.Delete("tmbsNew.exe");
				File.Delete("update");
			}
			if (text.Contains("tmbsNew"))
			{
				writeLine("Updating..");
				if (!Directory.Exists("oldVersions"))
				{
					Directory.CreateDirectory("oldVersions");
				}
				string fileName = Application.StartupPath + "\\ThumbServer.exe";
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
				string fileVersion = versionInfo.FileVersion;
				File.Move(Application.StartupPath + "\\ThumbServer.exe", Application.StartupPath + "\\oldVersions\\ThumbServer" + fileVersion + ".exe");
				File.Copy("tmbsNew.exe", "ThumbServer.exe");
				File.Create("update");
				Process.Start("ThumbServer.exe");
				ProjectData.EndApp();
			}
			writeLine("Loading settings..");
			if (!File.Exists("settings.ini"))
			{
				writeLine("settings.ini does not exist. Please copy format.ini, edit it appropriately and try again.");
				writeLine("Press any key to exit.");
				Console.ReadKey();
				ProjectData.EndApp();
			}
			string text3 = Application.StartupPath + "\\avatarRenders\\";
			string text4 = Application.StartupPath + "\\hatRenders\\";
			string text5 = Application.StartupPath + "\\tshirtRenders\\";
			string text6 = Application.StartupPath + "\\modelRenders\\";
			string path = Application.StartupPath + "\\placeRenders\\";
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			if (!Directory.Exists(text4))
			{
				Directory.CreateDirectory(text4);
			}
			if (!Directory.Exists(text5))
			{
				Directory.CreateDirectory(text5);
			}
			if (!Directory.Exists(text6))
			{
				Directory.CreateDirectory(text6);
			}
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			int num = 0;
			RichTextBox richTextBox = new RichTextBox();
			richTextBox.Text = File.ReadAllText("settings.ini");
			Array array = new string[13]
			{
				"waitToCapture", "waitforNewCheck", "Shard", "placeCaptureWait", "baseUrl", "storUrl", "storUsn", "storPsw", "MaxEmptyCalls", "AdditionalWarmBootTimes",
				"CoolBootTime", "Password", "ConsoleLines"
			};
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["waitToCapture"] = "12500";
			dictionary["waitforNewCheck"] = "1000";
			dictionary["Shard"] = "";
			dictionary["placeCaptureWait"] = "15000";
			dictionary["baseUrl"] = "";
			dictionary["storUrl"] = "";
			dictionary["storUsn"] = "";
			dictionary["storPsw"] = "";
			dictionary["MaxEmptyCalls"] = "30";
			dictionary["AdditionalWarmBootTimes"] = "0";
			dictionary["CoolBootTime"] = "7500";
			dictionary["Password"] = "";
			dictionary["ConsoleLines"] = "1000";
			string[] lines = richTextBox.Lines;
			foreach (string text7 in lines)
			{
				foreach (object item in array)
				{
					string text8 = Conversions.ToString(item);
					if (text7.StartsWith(text8 + "="))
					{
						dictionary[text8] = text7.Replace(text8 + "=", null);
					}
				}
			}
			int num2 = default(int);
			if (Versioned.IsNumeric(dictionary["waitToCapture"]))
			{
				num2 = Conversions.ToInteger(dictionary["waitToCapture"]);
			}
			else
			{
				kill("waitToCapture number is not valid. Check your settings file and try again.");
			}
			int millisecondsTimeout = default(int);
			if (Versioned.IsNumeric(dictionary["waitforNewCheck"]))
			{
				millisecondsTimeout = Conversions.ToInteger(dictionary["waitforNewCheck"]);
			}
			else
			{
				kill("waitforNewCheck number is not valid. Check your settings file and try again.");
			}
			int num3 = default(int);
			if (Versioned.IsNumeric(dictionary["placeCaptureWait"]))
			{
				num3 = Conversions.ToInteger(dictionary["placeCaptureWait"]);
			}
			else
			{
				kill("placeCaptureWait number is not valid. Check your settings file and try again.");
			}
			int num4 = default(int);
			if (Versioned.IsNumeric(dictionary["MaxEmptyCalls"]))
			{
				num4 = Conversions.ToInteger(dictionary["MaxEmptyCalls"]);
			}
			else
			{
				kill("MaxEmptyCalls number is not valid. Check your settings file and try again.");
			}
			int num5 = default(int);
			if (Versioned.IsNumeric(dictionary["AdditionalWarmBootTimes"]))
			{
				num5 = Conversions.ToInteger(dictionary["AdditionalWarmBootTimes"]);
			}
			else
			{
				kill("AdditionalWarmBootTimes number is not valid. Check your settings file and try again.");
			}
			int num6 = default(int);
			if (Versioned.IsNumeric(dictionary["CoolBootTime"]))
			{
				num6 = Conversions.ToInteger(dictionary["CoolBootTime"]);
			}
			else
			{
				kill("CoolBootTime number is not valid. Check your settings file and try again.");
			}
			if (Versioned.IsNumeric(dictionary["ConsoleLines"]))
			{
				consoleLines = Conversions.ToInteger(dictionary["ConsoleLines"]);
			}
			else
			{
				kill("ConsoleLines number is not valid. Check your settings file and try again.");
			}
			if ((Operators.CompareString(dictionary["Shard"], "", TextCompare: false) == 0) | (Operators.CompareString(dictionary["baseUrl"], "", TextCompare: false) == 0) | (Operators.CompareString(dictionary["storUrl"], "", TextCompare: false) == 0) | (Operators.CompareString(dictionary["storUsn"], "", TextCompare: false) == 0) | (Operators.CompareString(dictionary["storPsw"], "", TextCompare: false) == 0) | (Operators.CompareString(dictionary["Password"], "", TextCompare: false) == 0))
			{
				writeLine("The following variables cannot be empty:");
				writeLine("Shard (" + dictionary["Shard"] + ")");
				writeLine("baseUrl (" + dictionary["baseUrl"] + ")");
				writeLine("storUrl (" + dictionary["storUrl"] + ")");
				writeLine("storUsn (" + dictionary["storUsn"] + ")");
				writeLine("storPsw (" + dictionary["storPsw"] + ")");
				writeLine("Password (" + dictionary["Password"] + ")");
				kill("Make sure they have all been entered correctly and try again.");
			}
			int num7 = default(int);
			if (Versioned.IsNumeric(dictionary["Shard"]))
			{
				num7 = Conversions.ToInteger(dictionary["Shard"]);
			}
			else
			{
				kill("Shard number is not valid. Check your settings file and try again.");
			}
			if (dictionary["baseUrl"].EndsWith("/") | dictionary["storUrl"].EndsWith("/"))
			{
				kill("Please remove the / from all urls in your settings file, and try again.");
			}
			string text9 = dictionary["baseUrl"];
			string text10 = dictionary["storUrl"];
			string userName = default(string);
			string password = default(string);
			try
			{
				if (File.Exists("inttst.tmp"))
				{
					File.Delete("inttst.tmp");
				}
				MyProject.Computer.Network.DownloadFile(dictionary["storUrl"], "inttst.tmp", dictionary["storUsn"], dictionary["storPsw"]);
				text10 = dictionary["storUrl"];
				userName = dictionary["storUsn"];
				password = dictionary["storPsw"];
				try
				{
					File.Delete("inttst.tmp");
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception ex2 = ex;
					ProjectData.ClearProjectError();
				}
			}
			catch (Exception ex3)
			{
				ProjectData.SetProjectError(ex3);
				Exception ex4 = ex3;
				try
				{
					File.Delete("inttst.tmp");
				}
				catch (Exception ex5)
				{
					ProjectData.SetProjectError(ex5);
					Exception ex6 = ex5;
					ProjectData.ClearProjectError();
				}
				kill("The credentials for the storage server is invalid. Check them and try again.");
				ProjectData.ClearProjectError();
			}
			string text11 = dictionary["Password"];
			rendererLoaded = true;
			Rectangle rectangle = default(Rectangle);
			int num8 = 0;
			writeLine("Current shard: " + Conversions.ToString(num7));
			while (true)
			{
				writeLine("Cold booting the render..");
				Process process = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?oid=1");
				writeLine("Sleeping " + Conversions.ToString((double)num6 / 1000.0) + " seconds..");
				Thread.Sleep(num6);
				writeLine("Killing..");
				process.Kill();
				writeLine("Warm booting the render..");
				Process process2 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?oid=1");
				writeLine("Sleeping " + Conversions.ToString((double)num6 / 1000.0) + " seconds..");
				Thread.Sleep(num6);
				writeLine("Killing..");
				process2.Kill();
				if (num5 > 0)
				{
					writeLine("Performing additional warm boots as requested..");
					do
					{
						num = checked(num + 1);
						writeLine("Additional warm boot " + Conversions.ToString(num));
						Process process3 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?oid=1");
						writeLine("Sleeping " + Conversions.ToString((double)num6 / 1000.0) + " seconds..");
						Thread.Sleep(num6);
						writeLine("Killing..");
						process3.Kill();
					}
					while (num == num4 || num > num4);
					Console.WriteLine("Additional warm boots have been completed.");
				}
				num8 = 0;
				writeLine("The render should now open quickly.");
				while (true)
				{
					writeLine("Fetching asset to be rendered..");
					WebClient webClient = new WebClient();
					webClient.Headers.Add("user-agent", "GoodBloxrenderv/1.0");
					try
					{
						string[] array2 = webClient.DownloadString(text9 + "/goodblox/api/scripts/render_server/?ver=" + productVersion + "&shard=" + Conversions.ToString(num7) + "&password=" + text11).Split('|');
						if (Operators.CompareString(array2[0], "update", TextCompare: false) == 0)
						{
							writeLine("An update was found.");
							string text12 = array2[1];
							writeLine("URL: " + text12);
							if (File.Exists("tmbsNew.exe"))
							{
								File.Delete("tmbsNew.exe");
							}
							writeLine("Deleted tmbsNew.exe");
							writeLine("Downloading..");
							MyProject.Computer.Network.DownloadFile(text12, Application.StartupPath + "\\tmbsNew.exe");
							writeLine("Starting update..");
							Process.Start(Application.StartupPath + "\\tmbsNew.exe");
							ProjectData.EndApp();
						}
						if (Operators.CompareString(array2[0], "nan", TextCompare: false) != 0)
						{
							SetCursorPos(0, 0);
							writeLine("Moved cursor to 0,0");
						}
						checked
						{
							if (Operators.CompareString(array2[0], "0", TextCompare: false) == 0)
							{
								writeLine("Rendering avatar (userID : " + array2[2] + ")");
								int num9 = Conversions.ToInteger(array2[2]);
								string text13 = text3 + Conversions.ToString(num9) + "\\";
								if (!Directory.Exists(text13))
								{
									Directory.CreateDirectory(text13);
								}
								Process process4 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?oid=" + Conversions.ToString(num9));
								writeLine("Sleeping " + Conversions.ToString((double)num2 / 1000.0) + " seconds..");
								Thread.Sleep(num2);
								FocusWindow("Roblox - [Place1]", null);
								BringMainWindowToFront("Render");
								ImageModelServerView();
								if (Operators.CompareString(array2[3], "0", TextCompare: false) == 0)
								{
									writeLine("Capturing (hatOn: false)");
									Bitmap bitmap = new Bitmap(746, 914);
									Bitmap bitmap2 = new Bitmap(916, 914);
									Point point = new Point(612, 101);
									Bitmap bitmap3 = new Bitmap(100, 100);
									Bitmap bitmap4 = new Bitmap(180, 220);
									Graphics graphics = Graphics.FromImage(bitmap);
									Graphics graphics2 = Graphics.FromImage(bitmap2);
									Graphics graphics3 = Graphics.FromImage(bitmap3);
									Graphics graphics4 = Graphics.FromImage(bitmap4);
									Point upperLeftSource = new Point(rectangle.Left + 602, rectangle.Top + 89);
									graphics.CopyFromScreen(upperLeftSource, Point.Empty, bitmap.Size);
									upperLeftSource = new Point(rectangle.Left + 512, rectangle.Top + 89);
									graphics2.CopyFromScreen(upperLeftSource, Point.Empty, bitmap2.Size);
									bitmap.MakeTransparent(Color.FromArgb(0, 255, 1));
									bitmap.MakeTransparent(Color.FromArgb(75, 254, 0));
									bitmap.MakeTransparent(Color.FromArgb(75, 255, 0));
									bitmap.MakeTransparent(Color.FromArgb(82, 255, 0));
									bitmap.MakeTransparent(Color.FromArgb(79, 254, 0));
									bitmap.MakeTransparent(Color.FromArgb(79, 255, 0));
									bitmap2.MakeTransparent(Color.FromArgb(0, 255, 1));
									bitmap2.MakeTransparent(Color.FromArgb(75, 254, 0));
									bitmap2.MakeTransparent(Color.FromArgb(75, 255, 0));
									bitmap2.MakeTransparent(Color.FromArgb(82, 255, 0));
									bitmap2.MakeTransparent(Color.FromArgb(79, 254, 0));
									writeLine("Saving..");
									Size size = new Size(193, 192);
									Image image = ResizeImage(bitmap2, size);
									graphics4.DrawImage(image, -4, 14);
									bitmap4.Save(Conversions.ToString(num9) + ".png");
									size = new Size(100, 100);
									Image image2 = ResizeImage(bitmap2, size);
									graphics3.DrawImage(image2, 1, 2);
									bitmap3.Save(Conversions.ToString(num9) + "-others.png");
								}
								else
								{
									writeLine("Capturing (hatOn: true)");
									Bitmap bitmap5 = new Bitmap(746, 914);
									Bitmap bitmap6 = new Bitmap(916, 914);
									Point point2 = new Point(612, 101);
									Graphics graphics5 = Graphics.FromImage(bitmap5);
									Graphics graphics6 = Graphics.FromImage(bitmap6);
									Point upperLeftSource = new Point(rectangle.Left + 602, rectangle.Top + 89);
									graphics5.CopyFromScreen(upperLeftSource, Point.Empty, bitmap5.Size);
									upperLeftSource = new Point(rectangle.Left + 512, rectangle.Top + 89);
									graphics6.CopyFromScreen(upperLeftSource, Point.Empty, bitmap6.Size);
									bitmap5.MakeTransparent(Color.FromArgb(0, 255, 1));
									bitmap5.MakeTransparent(Color.FromArgb(75, 254, 0));
									bitmap5.MakeTransparent(Color.FromArgb(75, 255, 0));
									bitmap5.MakeTransparent(Color.FromArgb(82, 255, 0));
									bitmap5.MakeTransparent(Color.FromArgb(79, 254, 0));
									bitmap6.MakeTransparent(Color.FromArgb(0, 255, 1));
									bitmap6.MakeTransparent(Color.FromArgb(75, 254, 0));
									bitmap6.MakeTransparent(Color.FromArgb(75, 255, 0));
									bitmap6.MakeTransparent(Color.FromArgb(82, 255, 0));
									bitmap6.MakeTransparent(Color.FromArgb(79, 254, 0));
									writeLine("Saving..");
									Size size = new Size(180, 220);
									Image image3 = ResizeImage(bitmap5, size);
									image3.Save(text13 + Conversions.ToString(num9) + ".png");
									size = new Size(100, 100);
									Image image4 = ResizeImage(bitmap6, size);
									image4.Save(text13 + Conversions.ToString(num9) + "-others.png");
								}
								process4.Kill();
								writeLine("Uploading..");
								FileStream fileStream = File.Open(text13 + Conversions.ToString(num9) + ".png", FileMode.Open);
								FileStream fileStream2 = File.Open(text13 + Conversions.ToString(num9) + "-others.png", FileMode.Open);
								UploadAsync(text9 + "/goodblox/api/scripts/render_server/uploadTheRenderersOk.php?password=1pVoBfdx3PcgGZKR&queueid=" + array2[1], fileStream, fileStream2, 0);
								fileStream.Dispose();
								fileStream2.Dispose();
								writeLine("Done!");
								MyProject.Computer.FileSystem.RenameFile(text13 + Conversions.ToString(num9) + ".png", Conversions.ToString(num9) + "_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								MyProject.Computer.FileSystem.RenameFile(text13 + Conversions.ToString(num9) + "-others.png", Conversions.ToString(num9) + "-others_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								continue;
							}
							if (Operators.CompareString(array2[0], "1", TextCompare: false) == 0)
							{
								writeLine("Rendering hat (hatID : " + array2[2] + ")");
								int num10 = Conversions.ToInteger(array2[2]);
								string text14 = text4 + Conversions.ToString(num10) + "\\";
								if (!Directory.Exists(text14))
								{
									Directory.CreateDirectory(text14);
								}
								SetCursorPos(0, 0);
								writeLine("Moved cursor to 0,0");
								Process process5 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?aaid=" + Conversions.ToString(num10));
								writeLine("Sleeping " + Conversions.ToString((double)num2 / 1000.0) + " seconds..");
								Thread.Sleep(num2);
								FocusWindow("Roblox - [Place1]", null);
								BringMainWindowToFront("Render");
								ImageModelServerView();
								Bitmap bitmap7 = new Bitmap(921, 921);
								Point point3 = new Point(965, 521);
								Point point4 = new Point(995, 519);
								Graphics graphics7 = Graphics.FromImage(bitmap7);
								Point upperLeftSource = new Point(rectangle.Left + 500, rectangle.Top + 85);
								graphics7.CopyFromScreen(upperLeftSource, Point.Empty, bitmap7.Size);
								bitmap7.MakeTransparent(Color.FromArgb(75, 254, 0));
								bitmap7.MakeTransparent(Color.FromArgb(75, 255, 0));
								bitmap7.MakeTransparent(Color.FromArgb(82, 255, 0));
								bitmap7.MakeTransparent(Color.FromArgb(0, 255, 1));
								Size size = new Size(120, 120);
								Image image5 = ResizeImage(bitmap7, size);
								image5.Save(text14 + Conversions.ToString(num10) + ".png");
								size = new Size(250, 250);
								Image image6 = ResizeImage(bitmap7, size);
								image6.Save(text14 + Conversions.ToString(num10) + "-catpage.png");
								process5.Kill();
								writeLine("Uploading..");
								FileStream fileStream3 = File.Open(text14 + Conversions.ToString(num10) + ".png", FileMode.Open);
								FileStream fileStream4 = File.Open(text14 + Conversions.ToString(num10) + "-catpage.png", FileMode.Open);
								UploadAsync(text9 + "/goodblox/api/scripts/render_server/uploadTheRenderersOk.php?password=1pVoBfdx3PcgGZKR&queueid=" + array2[1], fileStream3, fileStream4, 1);
								fileStream3.Dispose();
								fileStream4.Dispose();
								writeLine("Done!");
								MyProject.Computer.FileSystem.RenameFile(text14 + Conversions.ToString(num10) + ".png", Conversions.ToString(num10) + "_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								MyProject.Computer.FileSystem.RenameFile(text14 + Conversions.ToString(num10) + "-catpage.png", Conversions.ToString(num10) + "-others_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								continue;
							}
							if (Operators.CompareString(array2[0], "2", TextCompare: false) == 0)
							{
								writeLine("Rendering shirt (shirtID : " + array2[2] + ")");
								string text15 = array2[2];
								string text16 = text5 + text15 + "\\";
								if (!Directory.Exists(text16))
								{
									Directory.CreateDirectory(text16);
								}
								string text17 = array2[3];
								writeLine("TextureID " + array2[3]);
								Image image7 = new Bitmap(250, 250);
								image7 = Image.FromFile("TShirt.Png");
								Graphics graphics8 = Graphics.FromImage(image7);
								WebClient webClient2 = new WebClient();
								writeLine("Downloading shirt image..");
								Bitmap image8 = (Bitmap)Image.FromStream(new MemoryStream(webClient2.DownloadData(text9 + "/goodblox/api/scripts/asset/?id=" + text17)));
								writeLine("Resizing image..");
								Size size = new Size(128, 128);
								Image image9 = ResizeImage(image8, size);
								writeLine("Drawing..");
								graphics8.DrawImage(image9, 57, 46);
								writeLine("Saving..");
								image7.Save(text16 + text15 + ".png", ImageFormat.Png);
								writeLine("Uploading..");
								MyProject.Computer.Network.UploadFile(text16 + text15 + ".png", text9 + "/goodblox/api/scripts/render_server/uploadTheRenderersOk.php?password=1pVoBfdx3PcgGZKR&queueid=" + array2[1]);
								writeLine("Done!");
								MyProject.Computer.FileSystem.RenameFile(text16 + text15 + ".png", text15 + "_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								continue;
							}
							if (Operators.CompareString(array2[0], "3", TextCompare: false) == 0)
							{
								writeLine("Rendering model (mID : " + array2[2] + ")");
								int num11 = Conversions.ToInteger(array2[2]);
								string text18 = text6 + Conversions.ToString(num11) + "\\";
								if (!Directory.Exists(text18))
								{
									Directory.CreateDirectory(text18);
								}
								Process process6 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "-script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/asset/scr.php?aaid=" + Conversions.ToString(num11));
								writeLine("Sleeping " + Conversions.ToString((double)num2 / 1000.0) + " seconds..");
								Thread.Sleep(num2);
								FocusWindow("Roblox - [Place1]", null);
								BringMainWindowToFront("Render");
								ImageModelServerView();
								writeLine("Capturing...");
								Bitmap bitmap8 = new Bitmap(921, 921);
								Point point5 = new Point(965, 521);
								Point point6 = new Point(995, 519);
								Graphics graphics9 = Graphics.FromImage(bitmap8);
								Point upperLeftSource = new Point(rectangle.Left + 500, rectangle.Top + 85);
								graphics9.CopyFromScreen(upperLeftSource, Point.Empty, bitmap8.Size);
								bitmap8.MakeTransparent(Color.FromArgb(75, 254, 0));
								bitmap8.MakeTransparent(Color.FromArgb(75, 255, 0));
								bitmap8.MakeTransparent(Color.FromArgb(82, 255, 0));
								bitmap8.MakeTransparent(Color.FromArgb(0, 255, 1));
								Size size = new Size(120, 120);
								Image image10 = ResizeImage(bitmap8, size);
								image10.Save(text18 + Conversions.ToString(num11) + ".png");
								process6.Kill();
								writeLine("Uploading..");
								MyProject.Computer.Network.UploadFile(text18 + Conversions.ToString(num11) + ".png", text9 + "/goodblox/api/scripts/render_server/uploadTheRenderersOk.php?password=1pVoBfdx3PcgGZKR&queueid=" + array2[1]);
								writeLine("Done!");
								MyProject.Computer.FileSystem.RenameFile(text18 + Conversions.ToString(num11) + ".png", Conversions.ToString(num11) + "_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								continue;
							}
							if (Operators.CompareString(array2[0], "4", TextCompare: false) == 0)
							{
								writeLine("Rendering place (placeID : " + array2[2] + ")");
								int num12 = Conversions.ToInteger(array2[2]);
								string text19 = Application.StartupPath + "\\" + Conversions.ToString(num12) + ".rbxl";
								string text20 = Application.StartupPath + "\\placeRenders\\" + Conversions.ToString(num12);
								if (!Directory.Exists(text20))
								{
									Directory.CreateDirectory(text20);
								}
								if (File.Exists(text19))
								{
									writeLine("Deleting existing place..");
									File.Delete(text19);
								}
								writeLine("Downloading place from storage server..");
								MyProject.Computer.Network.DownloadFile(text10 + "/places/" + Conversions.ToString(num12) + ".rbxl", text19, userName, password);
								writeLine("Changing sky..");
								MyProject.Computer.FileSystem.RenameDirectory(Application.StartupPath + "\\Render\\content\\sky", "sky2");
								MyProject.Computer.FileSystem.RenameDirectory(Application.StartupPath + "\\Render\\content\\sky1", "sky");
								SetCursorPos(0, 0);
								Process process7 = Process.Start(Application.StartupPath + "\\Render\\GBRender.exe", "\"" + text19 + "\" -script \"" + text9.Replace("https://", "http://") + "/goodblox/api/scripts/removeGears.lua\"");
								writeLine("Sleeping " + Conversions.ToString((double)num3 / 1000.0) + " seconds..");
								Thread.Sleep(num3);
								FocusWindow("Roblox - [" + Conversions.ToString(num12) + ".rbxl]", null);
								BringMainWindowToFront("Render");
								Bitmap bitmap9 = new Bitmap(1440, 900);
								Graphics graphics10 = Graphics.FromImage(bitmap9);
								Point upperLeftSource = new Point(rectangle.Left + 50, rectangle.Top + 100);
								graphics10.CopyFromScreen(upperLeftSource, Point.Empty, bitmap9.Size);
								bitmap9.MakeTransparent(Color.FromArgb(75, 254, 0));
								bitmap9.MakeTransparent(Color.FromArgb(75, 255, 0));
								bitmap9.MakeTransparent(Color.FromArgb(82, 255, 0));
								bitmap9.MakeTransparent(Color.FromArgb(0, 255, 1));
								Size size = new Size(720, 450);
								Image image11 = ResizeImage(bitmap9, size);
								image11.Save(text20 + Conversions.ToString(num12) + "-large.png", ImageFormat.Png);
								size = new Size(160, 100);
								Image image12 = ResizeImage(bitmap9, size);
								image12.Save(text20 + Conversions.ToString(num12) + "-small.png", ImageFormat.Png);
								size = new Size(420, 230);
								Image image13 = ResizeImage(bitmap9, size, preserveAspectRatio: false);
								image13.Save(text20 + Conversions.ToString(num12) + ".png", ImageFormat.Png);
								size = new Size(1440, 900);
								Image image14 = ResizeImage(bitmap9, size);
								image14.Save(text20 + Conversions.ToString(num12) + "-xlarge.png", ImageFormat.Png);
								process7.Kill();
								writeLine("Rechanging sky..");
								MyProject.Computer.FileSystem.RenameDirectory(Application.StartupPath + "\\Render\\content\\sky", "sky1");
								MyProject.Computer.FileSystem.RenameDirectory(Application.StartupPath + "\\Render\\content\\sky2", "sky");
								writeLine("Uploading..");
								FileStream fileStream5 = File.Open(text20 + Conversions.ToString(num12) + "-large.png", FileMode.Open);
								FileStream fileStream6 = File.Open(text20 + Conversions.ToString(num12) + "-small.png", FileMode.Open);
								FileStream fileStream7 = File.Open(text20 + Conversions.ToString(num12) + ".png", FileMode.Open);
								FileStream fileStream8 = File.Open(text20 + Conversions.ToString(num12) + "-xlarge.png", FileMode.Open);
								UploadAsync(text9 + "/goodblox/api/scripts/render_server/uploadTheRenderersOk.php?password=1pVoBfdx3PcgGZKR&queueid=" + array2[1], fileStream5, fileStream6, 2, fileStream7, fileStream8);
								fileStream5.Dispose();
								fileStream6.Dispose();
								fileStream7.Dispose();
								fileStream8.Dispose();
								writeLine("Done!");
								MyProject.Computer.FileSystem.RenameFile(text20 + Conversions.ToString(num12) + ".png", Conversions.ToString(num12) + "_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								MyProject.Computer.FileSystem.RenameFile(text20 + Conversions.ToString(num12) + "-small.png", Conversions.ToString(num12) + "-small_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								MyProject.Computer.FileSystem.RenameFile(text20 + Conversions.ToString(num12) + "-large.png", Conversions.ToString(num12) + "-large_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								MyProject.Computer.FileSystem.RenameFile(text20 + Conversions.ToString(num12) + "-xlarge.png", Conversions.ToString(num12) + "-xlarge_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".png");
								File.Delete(Conversions.ToString(num12) + ".rbxl");
								continue;
							}
							if (Operators.CompareString(array2[0], "nan", TextCompare: false) == 0)
							{
								writeLine("Nothing to render..");
								Thread.Sleep(millisecondsTimeout);
								num8++;
								if (unchecked(num8 == num4 || num8 > num4))
								{
									break;
								}
								continue;
							}
							writeLine("Received message.");
							writeLine(array2[0]);
							Thread.Sleep(millisecondsTimeout);
							if (Operators.CompareString(array2[1], "End", TextCompare: false) == 0)
							{
								ProjectData.EndApp();
								continue;
							}
							num8++;
						}
						if (num8 == num4 || num8 > num4)
						{
							break;
						}
					}
					catch (WebException ex7)
					{
						ProjectData.SetProjectError(ex7);
						WebException ex8 = ex7;
						Process[] processesByName = Process.GetProcessesByName("GBRender");
						Process[] array3 = processesByName;
						foreach (Process process8 in array3)
						{
							process8.Kill();
						}
						writeLine("Exception occured.");
						writeLine();
						writeLine(ex8);
						writeLine();
						writeLine("Restarting program in 5 seconds. More exception information will be stored in renderapp_exception_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".txt");
						Thread.Sleep(5000);
						string contents = "--- [ Exception Information On " + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + " ] ---\r\nMessage: " + ex8.Message + "\r\n\r\nStack Trace:\r\n" + ex8.StackTrace + "\r\n\r\nSource:\r\n" + ex8.Source + "\r\n\r\nMore Data:\r\n" + ex8.ToString();
						if (!Directory.Exists(Application.StartupPath + "\\RenderExceptions"))
						{
							Directory.CreateDirectory(Application.StartupPath + "\\RenderExceptions");
						}
						File.WriteAllText(Application.StartupPath + "\\RenderExceptions\\renderapp_exception_" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".txt", contents);
						Application.Restart();
						ProjectData.ClearProjectError();
						return;
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
		public static void kill(object str)
		{
			writeLine(RuntimeHelpers.GetObjectValue(str));
			writeLine("Press any key to exit.");
			Console.ReadKey();
			ProjectData.EndApp();
		}

		public static void writeLine(object str = null)
		{
			Console.WriteLine(RuntimeHelpers.GetObjectValue(str));
			consoleFile = consoleFile + str.ToString() + Environment.NewLine;
			checked
			{
				timesWrittenTo++;
				if ((timesWrittenTo == consoleLines) | (timesWrittenTo > consoleLines))
				{
					string contents = "--- [ Log file from " + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + " ] ---\r\n" + consoleFile;
					if (!Directory.Exists(Application.StartupPath + "\\Logs"))
					{
						Directory.CreateDirectory(Application.StartupPath + "\\Logs");
					}
					File.WriteAllText(Application.StartupPath + "\\Logs\\log" + DateTime.Parse(Conversions.ToString(DateTime.Now)).ToString("dd_MM_yyyy_hh_mm_ss_tt") + ".txt", contents);
					timesWrittenTo = 0;
					contents = "";
					Console.Clear();
				}
			}
		}

		private static object UploadAsync(string url, Stream avv1, Stream avv2, int Type, Stream avv3 = null, Stream avv4 = null)
		{
			HttpContent content = new StreamContent(avv1);
			HttpContent content2 = new StreamContent(avv2);
			HttpClient httpClient = new HttpClient();
			MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
			switch (Type)
			{
			case 0:
				multipartFormDataContent.Add(content, "av1", "filename");
				multipartFormDataContent.Add(content2, "av2", "filename");
				break;
			case 1:
				multipartFormDataContent.Add(content, "av1", "filename");
				multipartFormDataContent.Add(content2, "av2", "filename");
				break;
			case 2:
			{
				HttpContent content3 = new StreamContent(avv3);
				HttpContent content4 = new StreamContent(avv4);
				multipartFormDataContent.Add(content, "av1", "filename");
				multipartFormDataContent.Add(content2, "av2", "filename");
				multipartFormDataContent.Add(content3, "av3", "filename");
				multipartFormDataContent.Add(content4, "av4", "filename");
				break;
			}
			}
			HttpResponseMessage result = httpClient.PostAsync(url, multipartFormDataContent).Result;
			if (result.IsSuccessStatusCode)
			{
				return true;
			}
			return false;
		}

		public static void ImageModelServerView()
		{
			Crap();
			Thread.Sleep(1000);
			SetCursorPos(0, 0);
			Crap();
		}

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern long SetCursorPos(int X, int Y);

		[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		private static void Crap()
		{
			SetCursorPos(0, 0);
			SetCursorPos(242, 30);
			mouse_event(2, 0, 0, 0, 1);
			mouse_event(4, 0, 0, 0, 1);
			Thread.Sleep(500);
			SetCursorPos(303, 215);
			mouse_event(2, 0, 0, 0, 1);
			mouse_event(4, 0, 0, 0, 1);
			Thread.Sleep(500);
			SetCursorPos(0, 0);
		}

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

		[DllImport("user32.dll")]
		private static extern int SetForegroundWindow(IntPtr hwnd);

		public static object BringMainWindowToFront(string processName)
		{
			Process process = Process.GetProcessesByName(processName).FirstOrDefault();
			if (process != null)
			{
				if (process.MainWindowHandle == IntPtr.Zero)
				{
					ShowWindow(process.Handle, ShowWindowEnum.Restore);
				}
				SetForegroundWindow(process.MainWindowHandle);
				writeLine("Focused window");
				return true;
			}
			writeLine("Window not found. Retrying..");
			return false;
		}

		[DllImport("user32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern bool IsIconic(int hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int FindWindow([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpClassName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpWindowName);

		public static void FocusWindow(string strWindowCaption, string strClassName)
		{
			int num = FindWindow(ref strClassName, ref strWindowCaption);
			if (num > 0)
			{
				SetForegroundWindow((IntPtr)num);
				if (IsIconic(num))
				{
					ShowWindow((IntPtr)num, ShowWindowEnum.Restore);
				}
				else
				{
					ShowWindow((IntPtr)num, ShowWindowEnum.Show);
				}
			}
		}
	}
}
