using System;
using MonoTouch.Foundation;
using MonoTouch;
using MonoTouch.UIKit;

namespace LondonBike
{

	public static class Resources
	{
		public static UIImage Near = UIImage.FromBundle("icons/07-map-marker.png");
		public static UIImage Map = UIImage.FromBundle("icons/103-map.png");
		public static UIImage Timer = UIImage.FromBundle("icons/78-stopwatch.png");
		public static UIImage Routing = UIImage.FromBundle("icons/04-squiggle.png");
		public static UIImage Compass = UIImage.FromBundle("icons/71-compass.png");
		public static UIImage TripLog = UIImage.FromBundle("icons/96-book.png");
		public static UIImage Info = UIImage.FromBundle("icons/59-info.png");
		
		public static UIImage Gps = UIImage.FromBundle("icons/74-location-white.png");
		
		public static UIImage StartButton = UIImage.FromBundle("icons/startbutton.png");
		public static UIImage StopButton = UIImage.FromBundle("icons/stopbutton.png");
		
		public static UIImage TapHere = UIImage.FromBundle("icons/taphere.png");
		
			
		public static UIFont DetailFont = UIFont.SystemFontOfSize(16);
		public static UIFont SmallDetailFont = UIFont.SystemFontOfSize(12);
		public static UIFont MediumFont = UIFont.SystemFontOfSize(13);

		//kind blue purple colour
		public static UIColor DetailColor = UIColor.FromRGBA(81, 102, 145, 255);
		public static UIColor DarkBlue = UIColor.FromRGBA(29,53,78,255);
		public static UIColor MidBlue = UIColor.FromRGBA(42,65,80, 255);
		public static UIColor Grey = UIColor.FromRGBA(233,233,233,255);
		public static UIColor HeaderGreen = UIColor.FromRGBA(209,233,181,255);
		public static UIColor TextGreen = UIColor.FromRGBA(103,156,40,255);
		
		public static UIColor OverdueTextColor = UIColor.Red;
	}
}
