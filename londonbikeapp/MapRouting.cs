using System;
using MonoTouch.CoreLocation;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using MonoTouch.UIKit;


namespace LondonBike
{
	

	
	public class MapRouting
	{
		private CLLocationCoordinate2D Source;
		private CLLocationCoordinate2D Dest;
		public bool HasRoute = false;
		public int Time;
		public int Distance;
		public CLLocationCoordinate2D[] Points;
		public List<CLLocation> PointsList;
		
		public MapRouting (CLLocationCoordinate2D source, CLLocationCoordinate2D dest)
		{
			Source = source;
			Dest = dest;
		}
		
		public string TimeForDisplay
		{
			get
			{
				if (Time == 0) return "Not available";
				
				TimeSpan ts = new TimeSpan(0,0,Time);
				
				return string.Format("{0:0.0} mins", ts.TotalMinutes);
				
			}
		}
		
		public string DistanceForDisplay
		{
			get 
			{
				if (Distance == 0) return "Not available";
				
				float distance = Distance;
				
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					if (!defaults.BoolForKey("UseKMs"))
					{
						distance = Util.MetersToMiles(distance);
						
						
						return string.Format("{0:0.00}mi", distance);
					} else
					{
						if (distance > 1000) 
						{
							distance = distance / 1000;
							return string.Format("{0:0.00}km", distance);
						}
						
						return string.Format("{0}m", distance);
					}
				}
				
				
			}
		}
		
		public void FindRoute(NSAction callbackWhenDone)
		{
			using (var defaults = NSUserDefaults.StandardUserDefaults)
			{
					string routingType = defaults.StringForKey("routing_type");
				
					if (string.IsNullOrEmpty(routingType)) routingType = "cyclestreets-fastest";
				
				Util.Log("using routing: " + routingType);
				
				switch(routingType) 
				{ //balanced|fastest|quietest|shortest
					case "cyclestreets-fastest":
						FindCycleRouteRoute("fastest", callbackWhenDone);
						break;
					case "cyclestreets-balanced":
						FindCycleRouteRoute("balanced", callbackWhenDone);
						break;
					case "cyclestreets-quietest":
						FindCycleRouteRoute("quietest", callbackWhenDone);
						break;
					case "cloudmade":
						FindCloudmadeRoute(callbackWhenDone);
						break;
					default:
						FindCycleRouteRoute("fastest", callbackWhenDone);
						break;
				}
			}
		}
		
		// you need to go to cloudmade and get a key!
		const string locationUrl = "http://routes.cloudmade.com/KEYS_GO_HERE/api/0.3/{0},{1},{2},{3}/bicycle.gpx?lang=en&unit=km&token={4}";
		public void FindCloudmadeRoute(NSAction callbackWhenDone)
		{
			
			Util.Log ("YOU NEED TO GET A CLOUDMADE KEY FOR THIS TO WORK");
			
		    wc = new WebClient();
			
			string deviceId = UIDevice.CurrentDevice.UniqueIdentifier;
			
			string tokenUrl = string.Format("http://auth.cloudmade.com/token/KEYS_GO_HERE?userid={0}",
			                                deviceId.Substring(5, deviceId.Length - 5));
			
			
			string token = wc.UploadString(tokenUrl, "");
			
			wc = new WebClient();
			
			string url = string.Format(locationUrl, Source.Latitude, Source.Longitude, Dest.Latitude, Dest.Longitude, token);
			
			Console.WriteLine(url);
			StartWatchdogTimer();
			
			wc.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
		
				using (var pool = new NSAutoreleasePool()) 
				{
					StopWatchdogTimer();
					if (e.Cancelled) 
					{
						HasRoute = false;
						callbackWhenDone();
						return;
					}
					
					
					
					try 
					{
						string output = e.Result;
					
						Util.Log(output);
						XNamespace gpxNamespace = "http://www.topografix.com/GPX/1/1";
					
						
						
						XElement root = XElement.Parse(output);
						
						var extensionElement = root.Element(gpxNamespace + "extensions");
						
						if (!Int32.TryParse(extensionElement.Element(gpxNamespace + "distance").Value, out Distance)) Distance = 0;
						if (!Int32.TryParse(extensionElement.Element(gpxNamespace + "time").Value, out Time)) Time = 0;
						
						
						var gpxitems = from gpxitem in root.Elements(gpxNamespace + "wpt")
							select new CLLocationCoordinate2D() {
								Latitude = double.Parse(gpxitem.Attribute("lat").Value),
								Longitude = double.Parse(gpxitem.Attribute("lon").Value)
							};
						
						
						Points = gpxitems.ToArray();
						
						PointsList = new List<CLLocation>();
						
						foreach(var point in Points)
						{
							PointsList.Add(new CLLocation(point.Latitude, point.Longitude));
						}
						
						HasRoute = true;
					} catch (Exception ex)
					{
						HasRoute = false;
						Util.Log(ex.Message);
					}
					
					callbackWhenDone();
				}
			};
			
			wc.DownloadStringAsync(new Uri(url));
			
			
		}
		
		//Get a key from cyclestreets.net
		
		const string cycleStreetsLocationUrl = @"http://www.cyclestreets.net/api/journey.xml?key=CYCLE_STREETSKEY_HERE&start_longitude={1}&start_latitude={0}&finish_longitude={3}&finish_latitude={2}&plan={4}&segments=0";
		WebClient wc = null;
		NSTimer watchdogTimer = null;
		
		private void StartWatchdogTimer()
		{
			
			
			watchdogTimer = NSTimer.CreateScheduledTimer(new TimeSpan(0,0,15), delegate {
				Console.WriteLine("cancelled");
				wc.CancelAsync();
				HasRoute = false;
			});
		}
		
		private void StopWatchdogTimer()
		{
			
			if (watchdogTimer != null)
			{
				watchdogTimer.Invalidate();
				watchdogTimer = null;
			}
		}
		
		public void FindCycleRouteRoute(string type, NSAction callbackWhenDone)
		{
			wc = new WebClient();
			string url = string.Format(cycleStreetsLocationUrl, Source.Latitude, Source.Longitude, Dest.Latitude, Dest.Longitude, type);
			Util.Log(url);
			StartWatchdogTimer();
			
			wc.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e) {
				
				using (var pool = new NSAutoreleasePool()) 
				{
					StopWatchdogTimer();
					if (e.Cancelled) 
					{
						HasRoute = false;
						callbackWhenDone();
						return;
					}
					
					
					
					try 
					{
						
						string output = e.Result;
					
					
						if (output.Contains("type=\"error\""))
						{
							
							HasRoute = false;
							callbackWhenDone();
							return;
						}
						
						XElement root = XElement.Parse(output);
						
						var firstElement = root.Element("marker");
						
						if (!Int32.TryParse(firstElement.Attribute("time").Value, out Time)) Time = 0;
						if (!Int32.TryParse(firstElement.Attribute("length").Value, out Distance)) Distance = 0;
						
						string elements = firstElement.Attribute("coordinates").Value;
						
						string[] elementitems = elements.Split(' ');
						
						List<CLLocationCoordinate2D> coordlist = new List<CLLocationCoordinate2D>();
						
						foreach(string coord in elementitems)
						{
							string[] coords = coord.Split(',');
						
							double lat, lon;
							
							bool worked = double.TryParse(coords[0], out lon) && double.TryParse(coords[1], out lat);
							
							if (worked) 
							{
								coordlist.Add(new CLLocationCoordinate2D(lat, lon));
							}
							
						}
						
						
						
						Points = coordlist.ToArray();
						
						
						PointsList = new List<CLLocation>();
						
						foreach(var point in Points)
						{
							PointsList.Add(new CLLocation(point.Latitude, point.Longitude));
						}
						
						HasRoute = true;
					} catch (Exception ex) {
						HasRoute = false;
					}
					
					callbackWhenDone();
				}
			};
			
			wc.DownloadStringAsync(new Uri(url));
			
		}
		
		
	}
}

