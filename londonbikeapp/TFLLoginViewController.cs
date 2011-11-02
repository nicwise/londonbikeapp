using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace LondonBike
{
	public class TFLLoginViewController : DialogViewController
	{
		EntryElement login, password;
		
		string _username, _password;
		
		

		public TFLLoginViewController (NSAction afterDataLoaded) : base(null, false)
		{
			NavigationItem.Title = "TfL Login";
			
			//TableView.BackgroundColor = UIColor.FromPatternImage(StockImages.GradientBackground);
			//TableView.BackgroundColor = UIColor.Clear;
			
			
			_username = TflTripLog.TflUsername;
			_password = TflTripLog.TflPassword;
	
			login = new EntryElement("Username", "Your TfL login", _username); 
			
			
			login.KeyboardType = UIKeyboardType.EmailAddress;
			login.AutoCorrectionType = UITextAutocorrectionType.No;
			login.AutoCapitalizationType = UITextAutocapitalizationType.None;
			
			
			password = new EntryElement("Password", "Your password", _password, true);
			password.KeyboardType = UIKeyboardType.Default;
			password.AutoCorrectionType = UITextAutocorrectionType.No;
			password.AutoCapitalizationType = UITextAutocapitalizationType.None;
			
			
			
			
			RootElement root = new RootElement("TfL Login") {

				new Section("", "Enter your TfL credentials. Normally your email address and a password")
				{
					login,
					password
				}
			};
			
			Root = root;
			
			NavigationItem.RightBarButtonItem = new UIBarButtonItem("Login", UIBarButtonItemStyle.Bordered, delegate {
				
				this.login.FetchValue();
				this.password.FetchValue();
				
				TflTripLog.TflUsername = login.Value;
				TflTripLog.TflPassword = password.Value;
				
				ParentViewController.DismissModalViewControllerAnimated(true);
				
				afterDataLoaded();
				
			});
			
		
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered, delegate {
				
				this.ParentViewController.DismissModalViewControllerAnimated(true);
				
			});
			
			
		}
		
		public override void ViewDidLoad ()
		{
			NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
		}
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			login.BecomeFirstResponder();
		}
	}
}

