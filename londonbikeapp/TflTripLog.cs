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
	
	public enum ErrorType
	{
		None,
		Login,
		SiteError,
		Unknown
	}
	
	public class TflTripLog
	{
		public TflTripLog (string[] bits)
		{
			/*
			 * 
			 * 25 Aug 2010, 22:03||Payment|£-1.00||
			 * 21 Aug 2010, 14:45|21 Aug 2010, 15:10|Hire|£0.00|Tavistock Street, Covent Garden|Tower Gardens , Tower
			 * 21 Aug 2010, 12:59||Subscription|£1.00||
			 * 11 Aug 2010, 12:31|11 Aug 2010, 12:48|Hire|0.00|Colombo Street, Southwark|Park Street, Bankside
			 * 11 Aug 2010, 12:12|11 Aug 2010, 12:20|Hire|0.00|Park Street, Bankside|Colombo Street, Southwark
			 * 11 Aug 2010, 10:51|11 Aug 2010, 11:00|Hire|0.00|Colombo Street, Southwark|Park Street, Bankside|||0.00||
			 * 28 Jul 2010, 22:03||Payment|£-1.00||
			 * 28 Jul 2010, 22:03||Payment|£-3.00||
			 * 24 Jul 2010, 09:16||Admin|£3.00||
			 * 24 Jul 2010, 09:16||Subscription|£1.00||
			 */
			
			if (bits.Length == 2)
			{
				StartTime = "";
				EndTime = "";
				Command = bits[0];
				Cost = bits[1];
				
			} else {
			
				StartTime = bits[0];
				if (!string.IsNullOrEmpty(bits[1]))
				{
					EndTime = bits[1].Split(',')[1].Trim();
				}
				Command = bits[2];
				Cost = bits[3];
				
				if (!string.IsNullOrEmpty(bits[4]))
				{
					StartStation = bits[4].Split(',')[0].Trim();
				}
				if (!string.IsNullOrEmpty(bits[5]))
				{
					EndStation = bits[5].Split(',')[0].Trim();
				}
			}
			
			
		}
		
		public string StartStation;
		public string EndStation;
		public string StartTime;
		public string EndTime;
		public string Command;
		public string Cost;
		
		public static List<TflTripLog> all = null;
		public static bool LastWasError = false;
		public static ErrorType ErrorType = ErrorType.None;
		
		public static void ClearTflData()
		{
			all = null;
		}
		
		public static bool HasUsernamePassword
		{
			get
			{
				return (!String.IsNullOrEmpty(TflUsername) && !String.IsNullOrEmpty(TflPassword));
			}
		}
		
		public static string TflUsername
		{
			get
			{
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					return defaults.StringForKey("tfl_username");
				}
			}
			set
			{
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					defaults.SetString(value, "tfl_username");
					defaults.Synchronize();
				}
			}
		}
		
		public static string TflPassword
		{
			get
			{
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					return defaults.StringForKey("tfl_password");
				}
			}
			set
			{
				using (var defaults = NSUserDefaults.StandardUserDefaults)
				{
					defaults.SetString(value, "tfl_password");
					defaults.Synchronize();
				}
			}
		}
		
		public static void RefreshTripLogList(NSAction onDone)
		{
			
			if (HasUsernamePassword)
			{
			
			
				ThreadPool.QueueUserWorkItem(delegate {
					
					using (NSAutoreleasePool pool = new NSAutoreleasePool()) 
					{
						
						LastWasError = false;
						ErrorType = ErrorType.None;
						var newTripList = new List<TflTripLog>();
						
						Util.Log("updating trip log.....");
							
						if (Util.IsReachable("londonbikeapp.appspot.com"))
						{
							
							Util.TurnOnNetworkActivity();
							
							try {
							
								var wc = new WebClient();
								
								// this used to talk to AppEngine, but this now hits a static file
								string url = string.Format("http://www.fastchicken.co.nz/lba/accountinfo.txt");
								
								//url = string.Format("http://localhost:8085/account/get?key=foo");
								
								
								
								string logindata = string.Format("username={0}&password={1}", HttpUtility.UrlEncode(TflUsername),
								                                 HttpUtility.UrlEncode(TflPassword));
								
								//Util.Log(logindata);
								
								string s = wc.UploadString(url, "POST", logindata);
								
								Util.Log (s);
								
								//Util.Log(s);
								
								if (s.StartsWith("ERROR"))
								{
									
									
									LastWasError = true;
									
									switch (s.Trim())
									{
										case "ERROR-SITEERROR":
											ErrorType = ErrorType.SiteError;
											break;
										case "ERROR-LOGGINGIN":
										case "ERROR-LOGGINGIN2":
											ErrorType = ErrorType.Login;
											break;
										case "ERROR-OUTPUT":
										case "ERROR-LOGINPAGE":
											ErrorType = ErrorType.Unknown;
											break;
									}
									
									
									all = null;
									
									onDone();
									Util.Log("Trip log error: " + s + " " + url);
									return;
								}
								string[] items = s.Split('^');
								
								foreach(var item in items)
								{
									
									
									string[] bits = item.Split('|');
									
									try {
										
										TflTripLog log = new TflTripLog(bits);
										newTripList.Add(log);
									} catch 
									{
										//do nothing
										
									}
									
									//51.48802-0.16688|Flood Street|51.48802358|-0.16687854|True|18|14                              
									
								}
							
								all = newTripList;
								
								onDone();
								
								Util.Log("updated trip log");
								
							} catch (Exception ex)
							{
								LastWasError = true;
								ErrorType = ErrorType.Unknown;
								all = null;
								
								onDone();
						
							} finally {
								Util.TurnOffNetworkActivity();
								
							}
							
						} else Util.Log("No network or website not reachable");	
					}
				
				});
			}
		}
		
		public static List<TflTripLog> All
		{
			get
			{
				
				return all;
				
				
			}
		}
			
	}
}

