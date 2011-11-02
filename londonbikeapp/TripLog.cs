using System;
using System.Collections.Generic;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using System.Threading;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace LondonBike
{
	public class TripLog
	{
		public long Ticks;
		public double StartLat;
		public double StartLon;
		public double EndLat;
		public double EndLon;
		public string StartStation;
		public string EndStation;
		public int TimeInSeconds;
		public int DistanceInMeters;
		
		public TripLog ()
		{
			
		}
		
		public TripLog(string[] items)
		{
			Ticks = GetLong(items[0]);
			StartLat = GetDouble(items[1]);
			StartLon = GetDouble(items[2]);
			EndLat = GetDouble(items[3]);
			EndLon = GetDouble(items[4]);
			StartStation = items[5];
			EndStation = items[6];
			TimeInSeconds = GetInt(items[7]);
			DistanceInMeters = GetInt(items[8]);
		}
		
		public double GetDouble(string d)
		{
			double res = 0;
			if (double.TryParse(d, out res))
				return res;
			return 0;
		}
		
		public int GetInt(string d)
		{
			int res = 0;
			if (int.TryParse(d, out res))
				return res;
			return 0;
		}
		
		public long GetLong(string d)
		{
			long res = 0;
			if (long.TryParse(d, out res))
				return res;
			return 0;
		}
		
		public string DateForDisplay
		{
			get
			{
				return string.Format("{0:MMM dd HH:mm}", new DateTime(Ticks));
			}
		}
		
		public string TimeForDisplay
		{
			get
			{
				TimeSpan ts = new TimeSpan(0,0,TimeInSeconds);
				
				if (ts.TotalSeconds < 60) {
					return string.Format("{0:0} secs", ts.TotalSeconds);
				}
				
				if (ts.TotalMinutes < 60)
				{
					return string.Format("{0:0} mins", ts.TotalMinutes);
				}
				
				return string.Format("{0}h {0}m", ts.Hours, ts.Minutes);
			}
		}
		
		public string DistanceForDisplay
		{
			get
			{
				float distance = DistanceInMeters;
				string dist_display = "";
				
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					if (!defaults.BoolForKey("UseKMs"))
					{
						distance = Util.MetersToMiles(distance);
						
						
						return string.Format("{0:0.00} mi", distance);
					} else
					{
						if (distance > 1000) 
						{
							distance = distance / 1000;
							return string.Format("{0:0.00} km", distance);
						} else {
						
							return string.Format("{0} m", distance);
						}
					}
				}
				
				return "";
			
			}
		}
		
		public string AsLine
		{
			get
			{
				return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
				                     Ticks, StartLat, StartLon,
				                     EndLat, EndLon,
				                     StartStation, EndStation,
				                     TimeInSeconds, DistanceInMeters);
			}
		}
		
		public void SetLocation(double lat, double lon, bool isStart)
		{
			if (isStart) {
				StartLat = lat;
				StartLon = lon;
			} else {
				EndLat = lat;
				EndLon = lon;
			}
		}
		
		public static void MakeDummyData()
		{
			
			Util.Log("Making dummy trip data");
			
			string datafilename = Path.Combine(Util.DocDir, "triplog.txt");
				if (File.Exists(datafilename)) 
					File.Delete(datafilename);
			allTrips = null;
			
			var foo = All;
			
			allTrips.Add(new TripLog {
				Ticks = DateTime.Now.Ticks,
				StartLat = 51.49580589,
				StartLon =  -0.127575233,
				EndLat = 51.50581776, 
				EndLon = -0.100186337,
				StartStation = "Smith Square",
				EndStation = "Bankside Mix",
				TimeInSeconds = 851,
				DistanceInMeters = 3212
			});
			
			allTrips.Add(new TripLog {
				Ticks = DateTime.Now.Ticks,
				StartLat = 51.49580589,
				StartLon =  -0.127575233,
				EndLat = 51.50581776, 
				EndLon = -0.100186337,
				StartStation = "Smith Square",
				EndStation = "Bankside Mix",
				TimeInSeconds = 851,
				DistanceInMeters = 3212
			});allTrips.Add(new TripLog {
				Ticks = DateTime.Now.Ticks,
				StartLat = 51.49580589,
				StartLon =  -0.127575233,
				EndLat = 51.50581776, 
				EndLon = -0.100186337,
				StartStation = "Smith Square",
				EndStation = "Bankside Mix",
				TimeInSeconds = 851,
				DistanceInMeters = 3212
			});allTrips.Add(new TripLog {
				Ticks = DateTime.Now.Ticks,
				StartLat = 51.49580589,
				StartLon =  -0.127575233,
				EndLat = 51.50581776, 
				EndLon = -0.100186337,
				StartStation = "Smith Square",
				EndStation = "Bankside Mix",
				TimeInSeconds = 851,
				DistanceInMeters = 3212
			});
			
			PersistToDisk();
			
			allTrips = null;
			
			foo = All;
		}
		
		
		private static List<TripLog> allTrips = null;
		
		public static List<TripLog> All
		{
			get
			{
				if (allTrips == null)
				{
					allTrips = new List<TripLog>();
					string datafilename = Path.Combine(Util.DocDir, "triplog.txt");
			
					if (File.Exists(datafilename))
					{
						string[] content = File.ReadAllLines(datafilename);
						
						foreach(string line in content)
						{
							string[] items = line.Split('|');
							
							allTrips.Add(new TripLog(items));
						}
						
					}
				}
				
				return allTrips;
			}
		}
		
		public static void PersistToDisk()
		{
			if (allTrips != null)
			{
				string datafilename = Path.Combine(Util.DocDir, "triplog.txt");
				if (File.Exists(datafilename)) 
					File.Delete(datafilename);
				
				List<string> lines = new List<string>();
				
				foreach(var trip in allTrips)
				{
					lines.Add(trip.AsLine);
				}
				
				File.WriteAllLines(datafilename, lines.ToArray());
			}
		}
		
		private static CLLocationManager locationManager = null;
		private static MyLocationManagerDelegate locationDelegate = null;
		
		public static void GetCurrentLocation(TripLog currentLog, bool isStart, NSAction onFound)
		{
			if (!CLLocationManager.LocationServicesEnabled)
			{
				currentLog.SetLocation(-1, -1, isStart);
				
				onFound();
			}
			
			locationManager = new CLLocationManager();
			locationManager.DesiredAccuracy = CLLocation.AccuracyNearestTenMeters;
			
			locationDelegate = new MyLocationManagerDelegate();
			
			locationDelegate.OnLocationError += delegate(NSError error) {
				
				
				currentLog.SetLocation(-1, -1, isStart);
				locationManager.StopUpdatingLocation();
				onFound();
			};
			
			locationDelegate.OnLocationUpdate += delegate(CLLocation location) {
				
				currentLog.SetLocation(location.Coordinate.Latitude, location.Coordinate.Longitude, isStart);
				locationManager.StopUpdatingLocation();
				
				onFound();
				
			};
			
			locationManager.Delegate = locationDelegate;
			
			
			locationManager.StartUpdatingLocation();
			locationDelegate.StartTimer(locationManager);
			Util.TurnOnNetworkActivity();
			
			
		}
		
		public static void ReverseGeocode (TripLog trip)
		{
			
			if (trip.StartStation != "Unknown" && trip.EndStation != "Unknown")
				return;
			
			ThreadPool.QueueUserWorkItem (delegate {
				using (NSAutoreleasePool pool = new NSAutoreleasePool()) {
					
					Util.Log("Geocoding the start");
					if (trip.StartStation == "Unknown") {
						trip.StartStation = ReverseGeocode (trip.StartLat, trip.StartLon);
					}
					Util.Log ("Geocoding the end");
					
					if (trip.EndStation == "Unknown") {
						trip.EndStation = ReverseGeocode (trip.EndLat, trip.EndLon);
					}
					
					Util.Log ("Saving to disk");
					
					PersistToDisk ();
					
					
					
				}
			});
		}
		
		private static string ReverseGeocode(double lat, double lon)
		{
			string res = null;
					
			try {
				if (Reachability.IsHostReachable ("www.google.com")) {
					WebClient client = new WebClient ();
					string output = client.DownloadString (string.Format ("http://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}&sensor=true", lat, lon));
							
					XElement root = XElement.Parse (output);
							
					var items = from item in root.Descendants("result")
								select new 
								{
									AddressType = item.Element("type").Value,
									FormattedAddress = item.Element("formatted_address").Value
								};
							
					foreach (var item in items) {
						string[] parts = item.FormattedAddress.Split (',');
								
						if (parts.Length >= 2) {
							res = string.Format ("{0}, {1}", parts [0].Trim (), parts [1].Trim ());
							return res;
						} else {
							res = item.FormattedAddress;
							return res;
						}
					}
				}
						
				return "Unknown";
			} catch (Exception ex) {
				Util.Log ("Exception in reverse geocode: {0}", ex.Message);
				return "Unknown";
			}
					
			return "Unknown";
		}
		
		
		
		public static TripLog currentTripLog = null;
		
		public static void StartNewTripItem ()
		{
			currentTripLog = new TripLog ();
			currentTripLog.Ticks = DateTime.Now.Ticks;
			currentTripLog.TimeInSeconds = -1;
			currentTripLog.DistanceInMeters = -1;
			
			GetCurrentLocation (currentTripLog, true, delegate {
				
				Util.Log ("Found Start location");
				
#if DEBUG
#if FAKEDATA
				currentTripLog.StartLat = Locations.HydePark.Latitude;
				currentTripLog.StartLon = Locations.HydePark.Longitude;
#endif
#endif
				
				BikeLocation loc = BikeLocation.FindClosestBike (currentTripLog.StartLat, currentTripLog.StartLon);
				if (loc != null) {
					if (loc.DistanceFromCurrentPoint > 500) {
						currentTripLog.StartStation = "Unknown";
						
						
					} else {
						currentTripLog.StartStation = loc.Name;
						
					}
				} else {
					currentTripLog.StartStation = "Unknown";
				}
				
				Util.Log ("Station: " + currentTripLog.StartStation);
			
				NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
					
				defaults.SetString (currentTripLog.AsLine, "triplog_line");
				defaults.Synchronize ();
				
				
				
			});
		}
		
		
		
		public static void RemoveElementAt(int row)
		{
			All.RemoveAt(row);
			PersistToDisk();
		}
		
		public static void StopTripItem (int totalTime)
		{
			if (currentTripLog == null) {
				NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			
				
				string line = defaults.StringForKey ("triplog_line");
				
				if (string.IsNullOrEmpty (line))
					return;
				
				currentTripLog = new TripLog (line.Split ('|'));
			}
			
			currentTripLog.TimeInSeconds = totalTime;
			
			GetCurrentLocation (currentTripLog, false, delegate {
				
				
#if DEBUG
#if FAKEDATA
				currentTripLog.EndLat = Locations.BanksideMix.Latitude;
				currentTripLog.EndLon = Locations.BanksideMix.Longitude;
				currentTripLog.TimeInSeconds = 1120;
#endif
#endif
				
				Util.Log ("Found End location");
				BikeLocation loc = BikeLocation.FindClosestBike (currentTripLog.EndLat, currentTripLog.EndLon);
				if (loc != null) {
					if (loc.DistanceFromCurrentPoint > 500) {
						currentTripLog.EndStation = "Unknown";
					} else {
						currentTripLog.EndStation = loc.Name;
						
					}
				} else {
					currentTripLog.EndStation = "Unknown";
				}
				currentTripLog.DistanceInMeters = (int)BikeLocation.CalculateDistanceInMeters (
				                                                                         new CLLocationCoordinate2D (currentTripLog.StartLat, currentTripLog.StartLon),
				                                                                         new CLLocationCoordinate2D (currentTripLog.EndLat, currentTripLog.EndLon));
				BikeLocation.LogTrip (currentTripLog.TimeInSeconds, currentTripLog.DistanceInMeters);	
				
				Util.Log ("End Station: " + currentTripLog.EndStation);
				allTrips.Insert (0, currentTripLog);
				
			
			
				PersistToDisk ();
				
				if (allTrips.Count > 0)
					ReverseGeocode (allTrips [0]);
				
							
			});
			
			
			
		}
		
		public static void SaveCurrentTripLog()
		{
			
		}
		
		
		
	}
}

