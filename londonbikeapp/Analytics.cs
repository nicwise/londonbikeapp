using System;

using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace LondonBike
{
	public class Analytics
	{
	 	
		
		public static string CATEGORY_ACTION = "action";

		public static string CATEGORY_STATS = "stats";
		public static string ACTION_MA_VERSION = "application version";
		public static string ACTION_MODEL = "device model";
		public static string ACTION_MODEL_VERSION = "model-version";
		public static string ACTION_VERSION = "os version";
		public static string ACTION_NEAR_LIST = "near list";
		public static string ACTION_ROUTE = "route";
		public static string ACTION_MAP_BIKES = "map - bikes";
		public static string ACTION_MAP_DOCKS = "map - docks";
		public static string ACTION_DOWNLOAD = "download data";
		public static string ACTION_STARTUP = "app startup";
		public static string ACTION_TIMER_STOP = "timer stopped";
		public static string ACTION_TIMER_START = "timer started";
		
		
		
		
		
		public static void AppLaunched()
		{
			//this used to call into google analytics
			
			//removed because the stats were rubbish
			
			//all methods stubbed
			
		}
		
		public static bool TrackPageView(string url) 
		{
			return true;
			
		}
		
		public static bool TrackEvent(string category, string action, string label, int @value) 
		{
			
			return true;
			
		}
		
		public static bool Dispatch()
		{
			return true;
		}
		
		
		
	}
	
	public class TrackerException : Exception 
	{
		public NSError CoreError;
		
		public TrackerException(string msg, NSError error) : base(msg)
		{
			CoreError = error;
			
		}
	}
}

