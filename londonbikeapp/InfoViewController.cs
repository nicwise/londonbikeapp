using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;


using MonoTouch.ObjCRuntime;

namespace LondonBike
{
	/// <summary>
	/// The last tab - just a launch pad to get the user to look at our other stuff.
	/// </summary>
	public class InfoViewController : DialogViewController
	{
		
		
		public InfoViewController () : base(null, false)
		{
			Title = "Info";
			
			this.Style = MonoTouch.UIKit.UITableViewStyle.Grouped;
			
			
		}
		
		
		public override void ViewDidAppear (bool animated)
		{
			NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
			
			var root = new RootElement("Information");
			
			var section = new Section();
			
			section.Add(new DetailStringElement("London Bike App", "Find out more information about this app.", delegate {
				
					Util.LoadUrl("http://www.fastchicken.co.nz/londonbikeapp");
					
				}));
			
			section.Add(new DetailStringElement("Boris Bikes", "Talk with others about the bikes.", delegate {
				
					Util.LoadUrl("http://www.borisbikes.co.uk/");
					
				}));
			section.Add(new DetailStringElement("CycleStreets", "Provides the OSM-based routing.", delegate {
					
					Util.LoadUrl("http://www.cyclestreets.net");
				}));
			
			                     
			                        
			root.Add(section);
			
			Root = root;
			
			LoadFromWeb();
		}
		
		
		public void LoadFromWeb()
		{
			ThreadPool.QueueUserWorkItem(delegate {
				using (NSAutoreleasePool pool = new NSAutoreleasePool())
				{
					try {
						Util.TurnOnNetworkActivity();
						
						if (Reachability.IsHostReachable("app.londonbikeapp.com"))
						{
						
							Util.Log("Downloading info");
							WebClient client = new WebClient();
							string infopage = client.DownloadString("http://www.fastchicken.co.nz/lba/infopage.xml");
							string updateinfo = client.DownloadString("http://www.fastchicken.co.nz/lba/update_times.txt");
							
							string minTime = "";
							string maxTime = "";
							string lastUpdate = "";
							XElement root = null;
							try 
							{
								root = XElement.Parse(updateinfo);
								minTime = root.Element("min_update_time").Value;
								maxTime = root.Element("max_update_time").Value;
								
							
								Util.Log("min: {0} max: {1}", minTime, maxTime);
							} catch 
							{
								minTime = "";
								maxTime = "";
							}
							
							Util.Log("parsing");
							root = XElement.Parse(infopage);
							
							RootElement rootElement = new RootElement("Information");
							
							bool isFirst = true;
							
							foreach(var xmlSection in root.Descendants("section"))
							{
								Section section = new Section();
						
								try 
								{
									maxTime = maxTime.Substring(0, maxTime.LastIndexOf(".") ) + " +00:00";
									DateTime maxDatetime = DateTime.ParseExact(maxTime, "yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
									
									string footer = string.Format("Realtime data updated  @ {0}", maxDatetime.ToString("HH:mm"));
									
									if (isFirst)
									{
										section = new Section("", footer);
									}
								} catch(Exception ex)
								{
									
								}
								
								var items = from item in xmlSection.Elements("item")
									select new {
											Title = item.Attribute("title") == null ? "" : item.Attribute("title").Value,
											Subtitle = item.Attribute("subtitle") == null ? "" : item.Attribute("subtitle").Value,
											Url = item.Attribute("url") == null ? "" : item.Attribute("url").Value,
											LongText = item.Attribute("longtext") == null ? "" : item.Attribute("longtext").Value
										};
								
								isFirst = false;
								
								foreach(var item in items)
								{
									var localItem = item;
									
									//Util.Log("found item: {0}, {1}", localItem.Title, localItem.Subtitle);
									
									if (String.IsNullOrEmpty(localItem.LongText)) 
									{
									
										section.Add(new DetailStringElement(localItem.Title, localItem.Subtitle, delegate {
											Util.LoadUrl(localItem.Url);
										}));
									} else 
									{
										section.Add(new StyledMultilineElement(localItem.LongText)
										            {
											Font = Resources.DetailFont //, TextColor = Resources.DetailColor
										});
									}
								}
								
								
								
								rootElement.Add(section);
							}
							
							InvokeOnMainThread(delegate { Root = rootElement; });
						
						}
						
					} finally {
							            
						Util.TurnOffNetworkActivity();		   
					}
				}
				
			});
		}
		
		
	}
}
