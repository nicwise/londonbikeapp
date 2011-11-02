using System;
using MonoTouch.UIKit;
using System.IO;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.MessageUI;

namespace LondonBike
{
	public class Util
	{
		public static bool IsIOS4OrBetter
		{
			get
			{
				string version = UIDevice.CurrentDevice.SystemVersion;

				string[] versionElements = version.Split ('.');

				if (versionElements.Length > 0) {
					int versionInt = 0;
					if (Int32.TryParse (versionElements [0], out versionInt)) {
						if (versionInt >= 4)
							return true;
					}

					return false;

				}

				return false;

			}

		}

		public static AppDelegate AppDelegate
		{
			get
			{
				return (UIApplication.SharedApplication.Delegate as AppDelegate);
			}

		}

		public static void LoadUrl (string url)
		{
			UIApplication.SharedApplication.OpenUrl (new NSUrl (url));
		}

		public static string BaseDir
		{
			get
			{
				return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
			}

		}

		public static string AppDir
		{
			get
			{
				return Environment.CurrentDirectory;
			}
		}

		public static string DocDir
		{
			get
			{
				return Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			}
		}

		public static float MetersToMiles (float distance)
		{
			return (distance / 1000.0f) * 0.621371192f;
		}

		private static int networkActivityCounter = 0;

		public static void TurnOnNetworkActivity ()
		{
			if (networkActivityCounter == 0) {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
			}
			networkActivityCounter ++;
		}

		public static void TurnOffNetworkActivity ()
		{
			networkActivityCounter--;

			if (networkActivityCounter <= 0) {
				networkActivityCounter = 0;
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			}
		}

		public static bool IsReachable (string host)
		{
			return Reachability.InternetConnectionStatus () != NetworkStatus.NotReachable && 
					Reachability.IsHostReachable (host);

		}

		public static void LogStartup ()
		{
			if (DebugMode) {

				Log ("London Bike App Startup");
				Log ("SysName: {0}", UIDevice.CurrentDevice.SystemName);
				Log ("SysVer: {0}", UIDevice.CurrentDevice.SystemVersion);
				Log ("Name: {0}", UIDevice.CurrentDevice.Name);
				Log ("Model: {0}", UIDevice.CurrentDevice.Model);
				Log ("Local Model: {0}", UIDevice.CurrentDevice.LocalizedModel);
				Log ("Multitask: {0}", UIDevice.CurrentDevice.IsMultitaskingSupported.ToString ());
				Log ("Log filename: {0}", LogFilename);
			}


		}

		public static string AppVersion
		{
			get
			{
				return NSBundle.MainBundle.ObjectForInfoDictionary ("CFBundleVersion").ToString ();
			}
		}

		public static string CurrentVersion
		{
			get
			{
				using (NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults) {
					return defaults.StringForKey ("appversion") ?? "1.0";
				}
			}
			set
			{
				using (NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults) {
					defaults.SetString (AppVersion, "appversion");

					defaults.Synchronize ();
				}
			}
		}

		public static void Log (string message, params object[] param)
		{
		//.#.if DEBUG
					if (!DebugMode)
					{
						Console.WriteLine(message, param);
					}
		//.#.endif

			if (DebugMode) {
				Console.WriteLine (message, param);
				string msg = string.Format ("{0}: {1}", DateTime.Now.ToString ("yyyyMMdd/HHmmss"), string.Format (message, param));

				lock (loggingGate) {
					using (StreamWriter sw = File.AppendText (LogFilename)) {
						sw.WriteLine (msg);
						sw.Flush ();
						sw.Close ();
					}
				}

			}

		}

		public static void DeleteLogFile ()
		{
			if (File.Exists (LogFilename)) {
				File.Delete (LogFilename);
			}
		}

		public static string LogFilename
		{
			get
			{
				return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "londonbikeapp.log");
			}
		}

		public static object loggingGate = new object ();
		public static bool DebugMode = true;
		
		public class MailAttachment
		{
			public string Filename;
			public string FileType;
			public string EmailFilename;
			
			public MailAttachment(string filename, string filetype, string emailFilename)
			{
				Filename = filename;
				FileType = filetype;
				EmailFilename = emailFilename;
			}
		}
		
		private static MFMailComposeViewController mailCompose = null;
		public static void SendMail(UIViewController parent, string to, string subject, MailAttachment attachment, string body, NSAction onDone)
		{

			if (MFMailComposeViewController.CanSendMail)
			{
				
				mailCompose = new MFMailComposeViewController();
			
				mailCompose.SetSubject(subject);
				mailCompose.SetToRecipients(new string[] {to});
				if (attachment != null) 
				{
					NSData att = NSData.FromFile(attachment.Filename);
					mailCompose.AddAttachmentData(att, attachment.FileType, attachment.EmailFilename);
				}			
				mailCompose.SetMessageBody(body, false);
				mailCompose.Finished += delegate(object sender, MFComposeResultEventArgs e) {
					mailCompose.DismissModalViewControllerAnimated(true);
					onDone();
		
				};
				parent.PresentModalViewController(mailCompose, true);
			}
		}





	}
}

