using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace LondonBike
{
	public class ActivityIndicatorLoadingView : UIAlertView
	{

		private UIActivityIndicatorView _activityView;
		
		public void Show (string title)
		{
			
			Title = title;
			Show ();			
			// Spinner - add after Show() or we have no Bounds.
			_activityView = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge);
			_activityView.Frame = new RectangleF ((Bounds.Width / 2) - 15, Bounds.Height - 50, 30, 30);
			_activityView.StartAnimating ();
			AddSubview (_activityView);
		}
		
		public void Hide ()
		{
			DismissWithClickedButtonIndex (0, true);
		}
	}
	
	
	
	public class ProgressLoadingView : UIAlertView
	{

		private UIProgressView _progressView;
		
		public void Show (string title, int steps)
		{
			
			Title = title;
			Show ();			
			// Spinner - add after Show() or we have no Bounds.
			
			_progressView = new UIProgressView(UIProgressViewStyle.Default);
			_progressView.Frame = new RectangleF (15, Bounds.Height - 50, Bounds.Width - 30, 30);
			                                     
			
			step = 1.0f / steps;
			
			_progressView.Progress = 0;
			
			AddSubview (_progressView);
		}
		
		
		float step = 0.1f;
		float current = 0;
		public void StepProgress()
		{
			
			
			current += step;
			_progressView.Progress = current;
			
		}

		
		
		public void Hide ()
		{
			DismissWithClickedButtonIndex (0, true);
		}
	}
}

