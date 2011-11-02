using System;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;


namespace LondonBike
{
	/// <summary>
	/// A MonoTouch port of Appirator, which pops up every X launches and asks the user to review the app.
	/// Slightly PITA, but it works and it's useful.
	/// 
	/// Original code is here: http://arashpayan.com/blog/2009/09/07/presenting-appirater/
	/// </summary>
	public class Appirator
	{
		//Change these, otherwise I get your reviews.
		public const int APP_ID = 384046992;
		public const string APP_NAME = "London Bike App";
		public const string MESSAGE = "If you enjoy using this app, please take a moment to rate it. Thanks for your support!";
		public const string MESSAGE_TITLE = "Rate {0}";
		public const string CANCEL_BUTTON = "No, thanks";
		public const string RATE_BUTTON = "Rate {0}";
		public const string LATER_BUTTON = "Remind me later";
		public const int LAUNCHES_UNTIL_PROMPT = 20;
		
		private const string CONFIG_LAUNCHDATE = "config_launchdate";
		private const string CONFIG_LAUNCHCOUNT = "config_launchcount";
		private const string CONFIG_CURRENTVERSION = "config_currentversion";
		private const string CONFIG_RATEDCURRENTVERSION = "config_ratedversion";
		private const string CONFIG_DECLINEDTORATE = "config_declined_to_rate";
		
		private string TemplateReviewUrl = "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?id={0}&onlyLatestVersion=true&pageNumber=0&sortOrdering=1&type=Purple+Software";
		
		public Appirator ()
		{
		}
		
		private bool IsConnectedToNetwork()
		{
			return Util.IsReachable("www.apple.com");
		}

		private static Appirator app = null;
		
		public static void AppLaunched()
		{
			app = new Appirator();
			app.DoWorkInBackground();
		}
		
		private UIAlertView alertView = null;
		
		public void DoWorkInBackground()
		{
			ThreadPool.QueueUserWorkItem(delegate {
				using (NSAutoreleasePool autorelease = new NSAutoreleasePool())
				{
					
					Util.Log("Apirator launched");
					bool forceShowDialog = false;
#if DEBUG
					//forceShowDialog = true;
#endif
					
					bool shouldShowDialog = false;
					
					
					
					string currentVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
					using (NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults)
					{
						string trackingVersion = defaults.StringForKey(CONFIG_CURRENTVERSION);
						if (trackingVersion == null)
						{
							trackingVersion = currentVersion;
							defaults.SetString(trackingVersion, CONFIG_CURRENTVERSION);
							
						}
						
						Util.Log("Tracking version: " + trackingVersion);
						
						if (trackingVersion == currentVersion)
						{
							int launchCount = defaults.IntForKey(CONFIG_LAUNCHCOUNT);
							launchCount ++;
							defaults.SetInt(launchCount, CONFIG_LAUNCHCOUNT);
							Util.Log("Launch count is: " + launchCount);
							
							bool declinedToRate = defaults.BoolForKey(CONFIG_DECLINEDTORATE);
							bool hasRated = defaults.BoolForKey(CONFIG_RATEDCURRENTVERSION);
							
							if (launchCount > LAUNCHES_UNTIL_PROMPT &&
							    !declinedToRate &&
							    !hasRated)
							{
								if (IsConnectedToNetwork()) 
								{
									shouldShowDialog = true;
								}
							}   
							    
							
							defaults.Synchronize();
							
						} else {
							
							
					        Util.Log("Setting defatuls");
							defaults.SetString(currentVersion, CONFIG_CURRENTVERSION);
							defaults.SetInt(1, CONFIG_LAUNCHCOUNT);
							defaults.SetBool(false, CONFIG_RATEDCURRENTVERSION);
							defaults.SetBool(false, CONFIG_DECLINEDTORATE);
							defaults.Synchronize();
						}
						
					}
						
					
					
					if (shouldShowDialog || forceShowDialog)
					{
						
						
					    Util.Log("Apirator: Showing Dialog");
						UIApplication.SharedApplication.InvokeOnMainThread(delegate {
							alertView = new UIAlertView(string.Format(MESSAGE_TITLE, APP_NAME),
							                                        string.Format(MESSAGE, APP_NAME),
							                                        null,
							                                        CANCEL_BUTTON,
							                                        string.Format(RATE_BUTTON, APP_NAME),
							                                        LATER_BUTTON);
							alertView.Clicked += delegate(object sender, UIButtonEventArgs e) {
								
								using (NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults)
								{
									switch(e.ButtonIndex)
									{
									case 0:
										defaults.SetBool(true, CONFIG_DECLINEDTORATE);
										defaults.Synchronize();
										Util.Log("declined to rate. Boo");
										break;
									case 1:
										Util.Log("rating it! yay!");
										
										string reviewUrl = string.Format(TemplateReviewUrl, APP_ID);
										Util.Log(reviewUrl);
										UIApplication.SharedApplication.OpenUrl(new NSUrl(reviewUrl));
										
										defaults.SetBool(true, CONFIG_RATEDCURRENTVERSION);
									    defaults.Synchronize();
										
										
										break;
									case 2:
										Util.Log("doing it later");
										defaults.SetInt(5, CONFIG_LAUNCHCOUNT);
										defaults.Synchronize();
										
										int launchCount = defaults.IntForKey(CONFIG_LAUNCHCOUNT);
										
										Util.Log("Launch count is: " + launchCount);
										
										break;
									}
								}
								
							};
							
							alertView.Canceled += delegate(object sender, EventArgs e) {
								
							};
							alertView.Show();
						});
					}
					
					
					
				}
				
			});
			
		}
	}
	
}

