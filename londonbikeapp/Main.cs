
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;

namespace LondonBike
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		
		
		public UITabBarController tabBar;
		public UIView MainView;
		public UIViewController[] tabControllers;
		public NearDialogViewController near;
		public MapViewController map;
		public TimerViewController timer;
		public TripLogViewController tripLog;
		public InfoViewController infoPage;
		
		public void SetFocusOnLocation(BikeLocation bike)
		{
			UIView.BeginAnimations("foo");
			UIView.SetAnimationDuration(1);
			
			tabBar.SelectedIndex = 1;
			
			UIView.SetAnimationTransition(UIViewAnimationTransition.None, tabBar.View, true);
			UIView.CommitAnimations();
			map.FocusOnLocation(bike);
		}
		
		public override void WillEnterForeground (UIApplication application)
		{
			Util.Log("WillEnterForeground");
			Appirator.AppLaunched();
			Analytics.AppLaunched();
		}
		
		public override void DidEnterBackground (UIApplication application)
		{
			Util.Log("Did enter background");
			Analytics.Dispatch();
		}
		
		public override void FinishedLaunching (UIApplication application)
		{
			Util.Log("In FinishedLaunching(application)");
			
		}
		
		
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// If you have defined a view, add it here:
			// window.AddSubview (navigationController.View);
			
			//TripLog.MakeDummyData();
			
			Util.Log("In finished launching (with options)");
			
		
			bool localNotification = false;
			if (options != null)
			{
				if (Util.IsIOS4OrBetter)
				{
					UILocalNotification localNotif = (UILocalNotification)options.ObjectForKey(UIApplication.LaunchOptionsLocalNotificationKey);
					
					if (localNotif != null)
					{
						localNotification = true;
					}
				}
			}
			
			Util.LogStartup();
			
			
			var bikes = BikeLocation.AllBikes;
			
			
			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
			tabBar = new UITabBarController();MainView = tabBar.View;
			window.AddSubview(MainView);
			
			
			near = new NearDialogViewController();
			map = new MapViewController();
			tripLog = new TripLogViewController();
			infoPage = new InfoViewController();
			
			
			timer = new TimerViewController{
					TabBarItem = new UITabBarItem("Timer", Resources.Timer, 1)
				};
			
			tabControllers = new UIViewController[] {
			
				new UINavigationController(near) {
					TabBarItem = new UITabBarItem("Near", Resources.Near, 0)
				},
				new UINavigationController(map) {
					TabBarItem = new UITabBarItem("Map", Resources.Map, 2)
				},
				timer,
				new UINavigationController(tripLog) {
					TabBarItem = new UITabBarItem("Trip Log", Resources.TripLog, 3)
				},
				new UINavigationController(infoPage) {
					TabBarItem = new UITabBarItem("Info", Resources.Info, 4)
				}
			};
			
			
			tabBar.SetViewControllers(tabControllers, false);
			
			if (localNotification)
			{
				tabBar.SelectedIndex = 2;
			}
			
			window.MakeKeyAndVisible ();
			
			BikeLocation.UpdateFromWebsite(delegate { 
				Util.Log("Update on startup done"); 
				map.RefreshPinColours();
			});
			
			Appirator.AppLaunched();
			Analytics.AppLaunched();
			
			return true;
		}
		
		UIAlertView alert;
		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			//alert = new UIAlertView("Alarm", "there is an alarm going on.", null, "Ok");
			//alert.Show();
			
			tabBar.SelectedIndex = 2;
			
		}
		
		

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}

