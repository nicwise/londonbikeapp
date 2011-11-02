
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LondonBike
{
	public partial class TimerViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public DateTime StartDate;
		public bool Running = false;
		public NSTimer clockTimer;

		public TimerViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public TimerViewController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public TimerViewController () : base("TimerViewController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
			
		}
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad();
			
			LoadTimerUserDefaults();
			
			if (Running)
			{
				StartButton.SetImage(Resources.StopButton, UIControlState.Normal);
				clockTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1), UpdateTimerDisplay);
			} else{
				StartButton.SetImage(Resources.StartButton, UIControlState.Normal);
			}
			StartButton.TouchUpInside += delegate {
				
				
				
				if (Running) 
				{
					Running = false;
					
					SetTimerUserDefaults();
					
					TimeSpan elapsedTime = DateTime.Now - StartDate;
					
					int totalSeconds = (int)Math.Truncate(elapsedTime.TotalSeconds);
					
					TripLog.StopTripItem(totalSeconds);
					
					ClearNotifications();
					
					StartButton.SetImage(Resources.StartButton, UIControlState.Normal);
					//StartButton.SetTitle("Start", UIControlState.Normal);
					clockTimer.Invalidate();
					clockTimer = null;
					
					//CostLabel.Text = "£0.00";
					//ElapsedLabel.Text = "0h 00m";
					PriceIncreaseLabel.Text = "0m";
					StartedLabel.Text = "00:00";
					
					Analytics.TrackEvent(Analytics.CATEGORY_ACTION, Analytics.ACTION_TIMER_STOP, "", 1);
					Analytics.Dispatch();
					//BackgroundLocationManager.Instance.StopLocationManager();
		
					
					
				} else {
					StartDate = DateTime.Now;
					Running = true;
					
					TripLog.StartNewTripItem();
				
					SetNotification(StartDate.AddMinutes(30));
					
					
					SetTimerUserDefaults();
					
					StartButton.SetImage(Resources.StopButton, UIControlState.Normal);
					//StartButton.SetTitle("Stop", UIControlState.Normal);
					UpdateTimerDisplay();
					
					clockTimer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromSeconds(1), UpdateTimerDisplay);
					//BackgroundLocationManager.Instance.StartLocationManager();
					
					
					Analytics.TrackPageView("/timerstart");
					Analytics.TrackEvent(Analytics.CATEGORY_ACTION, Analytics.ACTION_TIMER_START, "", 1);
					Analytics.Dispatch();
		
				}
				
			};
		}
		
		public void SetNotification(DateTime originalTime)
		{
			if (Util.IsIOS4OrBetter)
			{
				UILocalNotification notification = new UILocalNotification();
				notification.TimeZone = NSTimeZone.DefaultTimeZone;
				notification.RepeatInterval = 0;
				notification.SoundName = UILocalNotification.DefaultSoundName;
				notification.AlertBody = string.Format("The free period of your bike hire will expire in 5 mins ({0}).", originalTime.ToShortTimeString());
				//notification.FireDate = DateTime.Now.AddSeconds(10);
				notification.FireDate = originalTime.AddMinutes(-5);
				
				UIApplication.SharedApplication.ScheduleLocalNotification(notification);
			}
		}
		
		public void ClearNotifications()
		{
			if (Util.IsIOS4OrBetter)
			{
				var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
				if (notifications != null || notifications.Length > 0) 
				{
					UIApplication.SharedApplication.CancelAllLocalNotifications();
				}
			}
		}
		
		
		public void SetTimerUserDefaults()
		{
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			
			defaults.SetBool(Running, "running");
			defaults.SetString(StartDate.Ticks.ToString(), "ticks");
			defaults.Synchronize();
			
		}
		
		public void LoadTimerUserDefaults() 
		{
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			
			bool running = defaults.BoolForKey("running");
			
			if (running) 
			{
				Running = running;
				long ticks = 0;
				if (long.TryParse(defaults.StringForKey("ticks"), out ticks)) 
				{
					StartDate = new DateTime(ticks);
					if ((DateTime.Now - StartDate).TotalDays > 1) Running = false;
				}else {
					Running = false;
				}
			}
			
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			//update things here?
			
			UpdateTimerDisplay();
			
		}
		
		public void UpdateTimerDisplay() 
		{
			InvokeOnMainThread(delegate {
				if (Running) 
				{
					
					TimeSpan elapsedTime = DateTime.Now - StartDate;
					
					int totalMinutes = (int)Math.Truncate(elapsedTime.TotalMinutes);
					
					CostLabel.Text = CalculateCost(totalMinutes);
					
					if (elapsedTime.Hours == 0)
					{
						ElapsedLabel.Text = string.Format("{0}m {1:00}s", elapsedTime.Minutes, elapsedTime.Seconds);
					} else 
					{
						ElapsedLabel.Text = string.Format("{0}h {0:00}m", elapsedTime.Hours, elapsedTime.Minutes);
					}
					CalculateTimeToNextPriceIncrease(totalMinutes);
					
					StartedLabel.Text = StartDate.ToString("HH:mm");
					
				} else{
					CostLabel.Text = "£0.00";
					ElapsedLabel.Text = "0m";
					PriceIncreaseLabel.Text = "0m";
					StartedLabel.Text = "00:00";
				}
			});
			
			
		}
		
		private string CalculateCost(int totalMinutes)
		{
			/*
			 * Up to 30 minutes	Free
Then	
Up to 1 hour	£1
Up to 1 hour and 30 minutes	£4
Up to 2 hours	£6
Up to 2 hours and 30 minutes	£10
Up to 3 hours	£15
Up to 6 hours	£35
Up to 24 hours (maximum usage fee)	£50
*/
			
			
			if (totalMinutes < 30) return "£0.00";
			if (totalMinutes < 60) return "£1.00";
			if (totalMinutes < 90) return "£4.00";
			if (totalMinutes < 120) return "£6.00";
			if (totalMinutes < 150) return "£10.00";
			if (totalMinutes < 180) return "£15.00";
			if (totalMinutes < 360) return "£35.00";
			return "£50.00";
			
		}
		
		private void CalculateTimeToNextPriceIncrease(int totalMinutes)
		{
			
			
			int remainder = 999;
			
				
			if (totalMinutes < 360) remainder = 360 - totalMinutes;
			if (totalMinutes < 180) remainder = 180 - totalMinutes;
			if (totalMinutes < 150) remainder = 150 - totalMinutes; 
			if (totalMinutes < 120) remainder = 120 - totalMinutes;
			if (totalMinutes < 90) remainder = 90 - totalMinutes;
			if (totalMinutes < 60) remainder = 60 - totalMinutes;
			if (totalMinutes < 30) remainder = 30 - totalMinutes;
			
			if (remainder < 5) 
			{
				PriceIncreaseLabel.TextColor = UIColor.Red;
			} else
			{
				PriceIncreaseLabel.TextColor = UIColor.White;
			}
			
			if (remainder == 999) 
			{
				PriceIncreaseLabel.Text = "Max / Late";
			} else {
				PriceIncreaseLabel.Text = string.Format("{0}m", remainder);
			}
			
			
		}
		
	}
}

