using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreLocation;
using System.Net;
using MonoTouch.CoreFoundation;
using System.IO;
using System.Threading;
using MonoTouch.Foundation;
using System.Text;


namespace LondonBike
{
	/// <summary>
	/// Stores and updates info about where the bikes are
	/// TfL have more info on the data they provide here
	/// 
	/// http://www.tfl.gov.uk/businessandpartners/syndication/default.aspx
	/// 
	/// this normally calls a AppEngine service which returns a pipe-seperates list,
	/// very much like baselist.txt, but it's live, coming from the TFL urls
	/// </summary>
	public class BikeLocation
	{
		public string Name;
		
		
		public int Capacity;
		public int DocksAvailable = -1;
		public int BikesAvailable = -1;
		public double Lat;
		public double Long;
		public int DistanceFromCurrentPoint;
		public int Heading;
		public bool IsAvailable = true;
		public string UniqueId = "";
		public bool Touched = false;
		
		
		public BikeLocation(string name, string postcode, int capacity, double lat, double _long, bool isAvailable)
		{
			Name = name;
			
			
			Capacity = capacity;
			Lat = lat;
			Long = _long;
			Location = new CLLocationCoordinate2D(Lat, Long);
			IsAvailable = isAvailable;
			UniqueId = string.Format("{0:0.00000}{1:0.00000}", Lat, Long);
		}
		
		public BikeLocation(string[] dock, bool setAvailable)
		{
			//51.48802-0.16688|Flood Street|51.48802358|-0.16687854|True|18|14
			
			UniqueId = dock[0];
			Name = dock[1];
			Lat = double.Parse(dock[2]);
			Long = double.Parse(dock[3]);
			IsAvailable = dock[4].ToLower() == "true";
			Capacity = int.Parse(dock[5]);
			Location = new CLLocationCoordinate2D(Lat, Long);
			if (setAvailable)
			{
				DocksAvailable = int.Parse(dock[6]);
			} else {
				DocksAvailable = -1;
			}
			BikesAvailable = int.Parse(dock[7]);
		}
		
	
		
		
		public string DistanceFromCurrentPointForDisplay
		{
			get
			{
				float distance = DistanceFromCurrentPoint;
				
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
		
		public CLLocationCoordinate2D Location { get; set; }
		
		private static List<BikeLocation> allBikes = null;
		private static Dictionary<string, BikeLocation> bikesById = null;
		
		public static List<BikeLocation> AllBikes
		{
			get 
			{
				BuildBikeList();
				return allBikes;
			}
		}
		
		public static bool isUpdating = false;
		public static string devkey = "";
		
		
		public static void LogTrip(int time, int distance)
		{
			ThreadPool.QueueUserWorkItem(delegate {
			
				using (NSAutoreleasePool pool = new NSAutoreleasePool())
				{
				
					try {
						Util.Log("Logging Search:");
						
						// Normally, this would call into the appengine service to record the trip
						
						// all of these used to have a devkey in, hence the string.format
						
						if (Util.IsReachable("www.fastchicken.co.nz"))
						{
							var wc = new WebClient();
							string s = wc.DownloadString(string.Format("http://www.fastchicken.co.nz?distance={0}&time={1}",  distance, time));
							Util.Log("trip: " + s);
						}
					} catch {
					}
				}
			});
		}
		
		public static void LogSearch()
		{
			ThreadPool.QueueUserWorkItem(delegate {
			
				using (NSAutoreleasePool pool = new NSAutoreleasePool())
				{
				
					try {
						Util.Log("Logging Search:");
						
						if (Util.IsReachable("www.fastchicken.co.nz"))
						{
							var wc = new WebClient();
							string s = wc.DownloadString(string.Format("http://www.fastchicken.co.nz/?count=1"));
							Util.Log("stats: " + s);
						}
					} catch {
					}
				}
			});
		}
		
		public static void LogRoute()
		{
			ThreadPool.QueueUserWorkItem(delegate {
			
				using (NSAutoreleasePool pool = new NSAutoreleasePool())
				{
				
					try {
						Util.Log("Logging route:");
						
						if (Util.IsReachable("www.fastchicken.co.nz"))
						{
							var wc = new WebClient();
							string s = wc.DownloadString(string.Format("http://www.fastchicken.co.nz?count=1"));
							Util.Log("route: " + s);
						}
					} catch 
					{
					}
				}
			});
		}
		
		public static bool UpdateFromWebsite(NSAction onDone)
		{
			
			if (isUpdating) return false;
			
			isUpdating = true;
			
			
			Analytics.TrackPageView("/download");
			Analytics.TrackEvent(Analytics.CATEGORY_ACTION, Analytics.ACTION_DOWNLOAD, "", 1);
			
			ThreadPool.QueueUserWorkItem(delegate {
			
				using (NSAutoreleasePool pool = new NSAutoreleasePool())
				{
				
					Util.Log("updating.....");
					
					if (Util.IsReachable("www.fastchicken.co.nz"))
					{
				
						//Thread.Sleep(500);
						
						Util.TurnOnNetworkActivity();
						
						try {
						
							string s = "";
							try 
							{
								var wc = new WebClient();
								//this would normally call the live service. This one is the same, but a static file
								s = wc.DownloadString("http://www.fastchicken.co.nz/lba/docks.txt");
							} catch (Exception ex)
							{
								Util.Log("Error downloading from the server.");
								Util.Log(ex.Message);
								return;
							}
							
							if (s == "KEY NOT VALID")
								return;
							string[] items = s.Split('^');
							
							foreach(BikeLocation bike in AllBikes)
							{
								bike.Touched = false;
							}
							
							foreach(var item in items)
							{
								string[] bits = item.Split('|');
								
								try {
									
									BikeLocation location = null;
									
									if (bikesById.ContainsKey(bits[0])) 
									{
										location = bikesById[bits[0]];
									} else
									{
										location = new BikeLocation(bits, true);
										AddBike(location);
									}
								
									location.DocksAvailable = -1;
									location.IsAvailable = bits[4].ToLower() == "true";
									location.DocksAvailable = Int32.Parse(bits[6]);
									location.Capacity = Int32.Parse(bits[5]);
									if (bits.Length == 8)
									{
										location.BikesAvailable = Int32.Parse(bits[7]);
									} else 
									{
										location.BikesAvailable = location.Capacity - location.DocksAvailable;
									}
									location.Touched = true;
									
								} catch 
								{
									//do nothing
									
								}
								
								                             
								
							}
							
							
							List<BikeLocation> DocksToRemove = new List<BikeLocation>();
							foreach(BikeLocation bike in AllBikes)
							{
								if (!bike.Touched)
								{
									Util.Log("Found untouched bike: {0}", bike.Name);
									DocksToRemove.Add(bike);
								}
							}
							
							foreach(BikeLocation bike in DocksToRemove)
							{
								RemoveBike(bike);
								
							}
							
							
							
							WriteToPersistentFile();
									
							
							
							Util.Log("updated");
					
						} finally {
							Util.TurnOffNetworkActivity();
							isUpdating = false;
						}
						if (onDone != null) onDone();
					
					} else Util.Log("No network or website not reachable");
					
				}
			});
			
			return true;
		
		}
		
		public static string CurrentFilePath
		{
			get
			{
				return Path.Combine(Util.DocDir, "currentlist_new.txt");
			}
				
		}
		
		public static string BaseFilePath
		{
			get
			{
				return Path.Combine(Util.AppDir, "baselist.txt");
			}
				
		}
		
		
		
		
		
		public static void WriteToPersistentFile()
		{
			StringBuilder sb = new StringBuilder();
			
			string delimiter = "";
			foreach(BikeLocation bike in BikeLocation.AllBikes)
			{
				
				sb.Append(string.Format("{0}{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", delimiter,
				         bike.UniqueId, bike.Name,
				         bike.Lat, bike.Long, bike.IsAvailable, bike.Capacity, bike.DocksAvailable, bike.BikesAvailable));
				delimiter = "^";
				
			}

			File.WriteAllText(CurrentFilePath, sb.ToString());

		}
		
		public static double CalculateDistanceInMeters(CLLocationCoordinate2D source, CLLocationCoordinate2D dest)
		{
			return CalculateDistance(source.Latitude, source.Longitude, dest.Latitude, dest.Longitude) * 1000;
		}
		
		public static BikeLocation FindClosestBike(double _lat, double _long)
		{
			if (_lat == -1 && _long == -1) return null;
			
			var list = FindClosestBikeList(_lat, _long);
			
			int i = 0;
			
			
			
			foreach(BikeLocation bike in list) 
			{
				return bike;
				
				
			}
			
			return null;
		}
		public static IEnumerable<BikeLocation> FindClosestBikeList(double _lat, double _long)
		{
		
			foreach(BikeLocation location in AllBikes)
			{
				double distance = CalculateDistance(_lat, _long, location.Lat, location.Long);
				double heading = CalculateHeading(_lat, _long, location.Lat, location.Long);
				
				location.DistanceFromCurrentPoint = (int)Math.Round(distance * 1000);
				location.Heading = (int)Math.Round(heading);
			}
			
			var ordered = (from bike in AllBikes orderby bike.DistanceFromCurrentPoint ascending select bike);
			
			return ordered;
			
		}
		
		public static void Debug(double _latSource, double _longSource, double _latDest, double _longDest)
		{
			Console.WriteLine(CalculateDistance(_latSource, _longSource, _latDest, _longDest));
			
			Console.WriteLine(CalculateHeading(_latSource, _longSource, _latDest, _longDest));
			                  
		}
		
		private static double ToRad(double deg)
		{
			return deg * Math.PI / 180;
		}
		
		private static double ToDeg(double rad) 
		{
			return rad * 180 / Math.PI;
		}
	
		private static double CalculateDistance(double _latSource, double _longSource, double _latDest, double _longDest)
		{
			int r = 6371; //km
			
			
			double dLat = ToRad((_latDest - _latSource));
			double dLong = ToRad((_longDest - _longSource));
			
			double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
				Math.Cos(ToRad(_latSource)) * Math.Cos(ToRad(_latDest)) *
				Math.Sin(dLong/2) * Math.Sin(dLong/2);
			double c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
			
			return r * c;
			
		}
	
		private static double CalculateHeading(double _latSource, double _longSource, double _latDest, double _longDest) 
		{
			
			double dLat = ToRad((_latDest - _latSource));
			double dLong = ToRad((_longDest - _longSource));
			
			double lat1 = ToRad(_latSource);
			double lat2 = ToRad(_latDest);
			
			double y = Math.Sin(dLong) * Math.Cos(lat2);
			double x = Math.Cos(lat1) * Math.Sin(lat2) - 
				Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLong);
			
			return (ToDeg(Math.Atan2(y, x)) + 360) % 360;
		}
		
		const int subSegment = 360 / 16;
		const int segment = 360/8;
		public string HeadingAsString
		{
			get 
			{
				return HeadingToString(Heading);
			}
		}
		
		public static string HeadingToString(int heading)
		{
			string[] segmentNames = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"};
				
			int subsegment = (heading + subSegment) % 360;
			
			int headingSegment = subsegment / segment;
			
			return segmentNames[headingSegment];
		}
		
		
		public static void CheckBikeIds()
		{
			List<string> bikeIds = new List<string>();
			
			foreach(BikeLocation bike in BikeLocation.AllBikes)
			{
				string id = bike.UniqueId;
			
				Console.WriteLine(id);
				if (bikeIds.Contains(id))
				{
					Console.WriteLine(id + " ALREADY EXISTS! " + bike.Name);
					continue;
				}
				
				bikeIds.Add(id);
				
			}
			
			Console.WriteLine("Done");
		}
		
		
	
		public static void BuildBikeList()
		{
			if (allBikes != null) return;
			
			allBikes = new List<BikeLocation>();
			bikesById = new Dictionary<string, BikeLocation>();
			
			
			string fileContent = "";

			if (File.Exists(CurrentFilePath))
			{
				Util.Log("Loading initial list from downloaded data");
				fileContent = File.ReadAllText(CurrentFilePath);
				if (fileContent == "")
				{
					if (File.Exists(CurrentFilePath))
						File.Delete(CurrentFilePath);
					Util.Log("loading initial list from stored data");
					fileContent = File.ReadAllText(BaseFilePath);
				}
				
			} else {
				Util.Log("loading initial list from stored data");
				fileContent = File.ReadAllText(BaseFilePath);
			}
			
			if (fileContent == "")
			{
				if (File.Exists(CurrentFilePath))
					File.Delete(CurrentFilePath);
				return;
			}
			
			
			string[] docksArray = fileContent.Split('^');
			
			foreach(var dockstring in docksArray)
			{
				string[] dock = dockstring.Split('|');
				
				//51.48802-0.16688|Flood Street|51.48802358|-0.16687854|True|18|14
				
				if (dock[0] != "0.000000.00000") // the workshop
				{
					AddBike(new BikeLocation(dock, false));
				}
			}
			
			
			
			
			
		}
		
		public static void AddBike(BikeLocation location)
		{
			allBikes.Add(location);
			bikesById.Add(location.UniqueId, location);
		}
		
		public static void RemoveBike(BikeLocation location)
		{
			allBikes.Remove(location);
			bikesById.Remove(location.UniqueId);
		}
	
	}
	
}

