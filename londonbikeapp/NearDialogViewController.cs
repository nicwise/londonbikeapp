using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.ObjCRuntime;

namespace LondonBike
{
	public class NearDialogViewController : DialogViewController
	{
		
		
		
		UIBarButtonItem refreshButton = null;
		UIBarButtonItem compassButton = null;
		bool updating = false;

		
		public NearDialogViewController () : base(null, false)
		{
			Title = "Nearest Bikes";
		
			this.RefreshRequested += delegate {
				
				if (!BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
							Util.AppDelegate.map.RefreshPinColours();
							RefreshData();	
						
							ReloadComplete();
							
						});
					}))
				{
					ReloadComplete();
				}
			};
			
			this.Style = MonoTouch.UIKit.UITableViewStyle.Plain;
		
			refreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, delegate {
				
				if (!updating)
				{
					
					updating = true;
					BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
							Util.AppDelegate.map.RefreshPinColours();
							RefreshData();	
							
						});
					});
					
					HasList = true;
					ToggleTapHereView();
					UpdateNearestList();
				}
			});
			
			NavigationItem.RightBarButtonItem = refreshButton;
			
			if (CLLocationManager.HeadingAvailable)
			{
				compassButton = new UIBarButtonItem(Resources.Compass, UIBarButtonItemStyle.Bordered, delegate {
					
					ToggleCompassView();
				});
				
				NavigationItem.LeftBarButtonItem = compassButton;
			}
			
		}
		
		UIImageView taphereview = null;
		public override void ViewDidAppear (bool animated)
		{
			Util.Log("NearView: Appear");
			NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
			
			ToggleTapHereView();
			
			BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
							Util.AppDelegate.map.RefreshPinColours();
							RefreshData();	
							
						});
					});
			
		}
		
		public bool CompassIsOn = false;
		private UIAlertView alert;
		
		public void ToggleCompassView()
		{
			if (HasList)
			{
				if (CLLocationManager.LocationServicesEnabled)
				{
					if (locationManager != null)
					{
						if (!CompassIsOn)
						{
							CompassIsOn = true;
							
							LoadAndShowCompassView();
							locationManager.StartUpdatingHeading();
							
							
						} else {
							CompassIsOn = false;
							locationManager.StopUpdatingHeading();
							HideCompassView();
						}
					}
				}
			} else {
				alert = new UIAlertView("Compass", "You must load a list of docks before using the compass", null, "Ok");
				alert.Show();
			}
		}
		
		
		private CompassView compassView = null;
		
		public void UpdateCompass(int heading)
		{
			if (compassView != null && CompassIsOn)
			{
				compassView.UpdateCompassLabel(BikeLocation.HeadingToString(heading));
			}
		}
		
		public void LoadAndShowCompassView()
		{
			
			if (compassView == null)
			{
				Util.Log("creating compass view");
				
				compassView = new CompassView();
				
				var frame = compassView.View.Frame;
				
				frame.X = (160 - (compassView.View.Frame.Width / 2));
				frame.Y = (320 - (compassView.View.Frame.Height / 2));
				compassView.View.Frame = frame;
				
				compassView.UpdateCompassLabel("--");
				
				ParentViewController.View.AddSubview(compassView.View);
				ParentViewController.View.BringSubviewToFront(compassView.View);
				
				
			}
		}
		
		public void HideCompassView()
		{
			Util.Log("hiding compass view");
				
			compassView.View.RemoveFromSuperview();
			compassView = null;
		}
		
		public void ToggleTapHereView()
		{
			if (HasList)
			{
				if (taphereview != null) 
				{
					UIView.BeginAnimations("foo");
					UIView.SetAnimationDuration(0.25f);
					if (taphereview != null) taphereview.Alpha = 0;
					UIView.SetAnimationDidStopSelector(new Selector("removetaphere"));
					UIView.CommitAnimations();
				}
			} else {
				if (taphereview == null)
				{
					taphereview = new UIImageView(Resources.TapHere);
					taphereview.Frame = new System.Drawing.RectangleF(320f - Resources.TapHere.Size.Width - 25, 64, Resources.TapHere.Size.Width, Resources.TapHere.Size.Height);
					ParentViewController.View.AddSubview(taphereview);
					
				}
				ParentViewController.View.BringSubviewToFront(taphereview);
				ParentViewController.View.SetNeedsDisplay();
			}
		}
		
		[Export("remotetaphere")]
		public void RemoveTapHere()
		{
			if (taphereview != null) taphereview.RemoveFromSuperview();
			taphereview = null;
		}
		
		CLLocationManager locationManager = null;
		MyLocationManagerDelegate locationDelegate = null;
		
		bool HasList = false;
		public void UpdateNearestList()
		{
			if (!CLLocationManager.LocationServicesEnabled) 
			{
				alert = new UIAlertView("No Location Available", "Sorry, no location services are available. However, you may still be able to use the timer and map.", null, "Ok");
				alert.Show();
				updating = false;
				return;
			}
				
			if (locationManager == null) 
			{
		    	locationManager = new CLLocationManager();
				locationManager.DesiredAccuracy = CLLocation.AccuracyNearestTenMeters;
				
			
			
				locationDelegate = new MyLocationManagerDelegate();
				locationDelegate.OnLocationError += delegate(NSError error) {
					
				
					alert = new UIAlertView("No Location Available", "Sorry, but there was an error finding your location. However, you may still be able to use the timer and map.", null, "Ok");
					alert.Show();
					
					updating = false;
				};
				
				locationDelegate.OnHeadingUpdate += delegate(CLHeading heading) {
					Util.Log("heading: " + heading.TrueHeading);
					
					UpdateCompass((int)heading.TrueHeading);
				};
				locationDelegate.OnLocationUpdate += delegate(CLLocation location) {
					CLLocationCoordinate2D coord = location.Coordinate;
#if DEBUG

					coord = Locations.BanksideMix;
#endif
					
					
					var bikeList = BikeLocation.FindClosestBikeList(coord.Latitude, coord.Longitude);
					
					RootElement root = new RootElement("Nearest Bikes");
					HasList = true;
					
					
					
					
					
					Section section = new Section();
					int i = 0;
					
					if (location.HorizontalAccuracy > 100)
					{
						section.Add(new StringElement("Low GPS accuracy!") {
							Alignment = UITextAlignment.Center
						});
					}
					
					
					
					foreach(BikeLocation bike in bikeList)
					{
						var localBike = bike;
						
						
						
						section.Add(new BikeElement(localBike, delegate {
							
							Util.AppDelegate.SetFocusOnLocation(localBike);
							
							
							
						}));
						
						
							
						
						       
						i ++;
						if (i > 20) break;
					}
					
					root.Add(section);
					
					
					Root = root;
					
					updating = false;
					
					BikeLocation.LogSearch();
					
					
				};
				locationManager.Delegate = locationDelegate;
			}
			
			locationManager.StartUpdatingLocation();
			locationDelegate.StartTimer(locationManager);
			Util.TurnOnNetworkActivity();
			
				
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			Util.Log("NearDVC: Disappear");
			base.ViewWillDisappear (animated);
			if (locationManager != null) 
			{
				locationManager.StopUpdatingLocation();
				locationDelegate.StopTimer();
			}
		}
		
		
		
		public void RefreshData()
		{
			Util.Log("refreshing data");
			if (Root != null)
			{
				this.ReloadData();
			}
		}
	}
		
	
	public class MyLocationManagerDelegate : CLLocationManagerDelegate
	{
		private NSTimer locationTimer = null;
		private CLLocationManager Manager;
		private CLLocation lastAttempt;
		private bool hasLastAttempt = false;
		
		public void StartTimer(CLLocationManager manager)
		{
			hasLastAttempt = false;
			
			Manager = manager;
			locationTimer = NSTimer.CreateScheduledTimer(TimeSpan.FromSeconds(30), TerminateLocationUpdate);
		}
		
		public void StopTimer()
		{
			if (locationTimer != null)
			{
				locationTimer.Invalidate();
			}
			locationTimer = null;
			Manager = null;
		}
		
		
		public void TerminateLocationUpdate()
		{
			
			
			Manager.StopUpdatingLocation();
			Util.TurnOffNetworkActivity();
			StopTimer();
			
			if (hasLastAttempt)
			{
				if (OnLocationUpdate != null) {
					OnLocationUpdate (lastAttempt);
				}
			} else 
			{
				if (OnLocationError != null)
				{
					OnLocationError(null);
				}
			}
		}
		
		public override void Failed (CLLocationManager manager, NSError error)
		{
			manager.StopUpdatingLocation ();
			Util.TurnOffNetworkActivity();
			StopTimer();
			
			if (OnLocationError != null)
			{
				OnLocationError(error);
			}
		}
		
		public override void UpdatedHeading (CLLocationManager manager, CLHeading newHeading)
		{
			if (OnHeadingUpdate != null)
				OnHeadingUpdate(newHeading);
		
		}
		
		
		public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
		{
			
			
			
			
			if (newLocation.HorizontalAccuracy <= 100) 
			{
				manager.StopUpdatingLocation ();
				
				StopTimer();
				
				hasLastAttempt = false;
				
				Util.TurnOffNetworkActivity();
				if (OnLocationUpdate != null) {
					OnLocationUpdate (newLocation);
				}
				
				return;
			}
			
			
			if (!hasLastAttempt || newLocation.HorizontalAccuracy <= lastAttempt.HorizontalAccuracy) 
			{
				hasLastAttempt = true;
				lastAttempt = newLocation;
			}
			
			
		}
		public event HeadingUpdateDelegare OnHeadingUpdate;
		public event LocationUpdateDelegate OnLocationUpdate;
		public event LocationErrorDelegate OnLocationError;
		
		
	}
	public delegate void LocationUpdateDelegate (CLLocation location);
	public delegate void LocationErrorDelegate (NSError error);
	public delegate void HeadingUpdateDelegare(CLHeading heading);
	
	
	
		
		
		
	
}

