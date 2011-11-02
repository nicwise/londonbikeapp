

using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.IO;

using MonoTouch.ObjCRuntime;

namespace LondonBike
{
	public class TripLogViewController : DialogViewController
	{
		
		public class EditingSource : DialogViewController.Source
		{
			public EditingSource(DialogViewController dvc) : base(dvc)
			{
			}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				return indexPath.Section == 0;
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) 
				{
					return UITableViewCellEditingStyle.Delete;
				} else 
					return UITableViewCellEditingStyle.None;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				//
				// In this method, we need to actually carry out the request
				//
				var section = Container.Root [indexPath.Section];
				var element = section [indexPath.Row];
				section.Remove (element);
				
				TripLog.RemoveElementAt(indexPath.Row);
			}
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			if (unevenRows)
				throw new NotImplementedException ("You need to create a new SourceSizing subclass, this sample does not have it");
			return new EditingSource (this);
		}
		
		bool updating = false;
		UIBarButtonItem refreshButton = null;
		
		TFLLoginViewController loginDialog = null;
		UINavigationController loginController = null;
		
		public TripLogViewController () : base(null, false)
		{
			Title = "Trip Log";
			
			this.Style = MonoTouch.UIKit.UITableViewStyle.Plain;
			
			ReloadFromDisk();
			
			refreshButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh, delegate {
				
				if (!TflTripLog.HasUsernamePassword)
				{
					ShowLoginDialog();
					
				}
				else
				{
					DownloadTripLog();
				}
				
				
				
			});
			
			NavigationItem.RightBarButtonItem = refreshButton;
			
		}
		
		public void ShowLoginDialog()
		{
					
			loginDialog = new TFLLoginViewController(delegate {
				TflTripLog.ClearTflData();
				DownloadTripLog();
			});
			
			loginController = new UINavigationController(loginDialog);
			
			ParentViewController.PresentModalViewController(loginController, true);
		}
		
		public void DownloadTripLog()
		{
			if (!updating)
			{
				updating = true;
				
				TflTripLog.RefreshTripLogList(delegate {
					updating = false;
					
					InvokeOnMainThread(delegate {
						DisplayTflData();
					});
				});
			
				
			}
		}
		
		public void DisplayTflData()
		{
			
						
			if (Root.Count > 1)
			{
				Root.RemoveAt(1);
			}
			
			Section newSection = new Section("TfL Account");
		
			var tflTripList = TflTripLog.All;
			
			if (tflTripList != null)
			{
				foreach(TflTripLog tflTrip in tflTripList)
				{
					var localTflTripLog = tflTrip;
					newSection.Add(new TflTripLogElement(localTflTripLog));
				}
			} else 
			{
				if (TflTripLog.LastWasError)
				{
					switch (TflTripLog.ErrorType)
					{
						case ErrorType.Login:
							newSection.Add(new DetailStringElement("Login error", "Check your username and password."));
							break;
						case ErrorType.Unknown:
							newSection.Add(new DetailStringElement("Unknown error", "There was a problem getting your data."));
							break;
						case ErrorType.SiteError:
							newSection.Add(new DetailStringElement("TfL Site Error", "TfL appears to be having issues. Try again later."));
							break;
					}
					
				} else {
					newSection.Add(new DetailStringElement("Press refresh to connect to TfL", "This will retrieve your account information."));
				}
			}
			
			var loginElement = new DetailStringElement("Configure your TfL login", "Set your TfL username and password.", delegate {
				ShowLoginDialog();
			});
			
			loginElement.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			newSection.Add(loginElement);
			Root.Add(newSection);
		}
		
		
		public override void ViewDidAppear (bool animated)
		{
			NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
			updating = false;
			ReloadFromDisk();
			
			
		}
		
		public void ReloadFromDisk()
		{
			
			var tripLogList = TripLog.All;
			
			RootElement root = new RootElement("Trip Log");
			
			Section section = new Section("Trip Log");
			
			foreach(TripLog tripLog in tripLogList)
			{
				var localTripLog = tripLog;
				
				section.Add(new TripLogElement(localTripLog, delegate {
					var tlv = new TripLogDetailViewController();
					tlv.TripLog = localTripLog;
					
					NavigationController.PushViewController(tlv, true);
					
				}));
			}

			root.Add(section);
			
			
			
			
			Root = root;
			
			
			DisplayTflData();
			
		}
		
		
	}
}
