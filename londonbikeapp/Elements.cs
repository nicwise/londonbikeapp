using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;

using MonoTouch.ObjCRuntime;

namespace LondonBike
{

	public class DetailStringElement : StringElement {
		static NSString skey = new NSString ("DetailStringElement");
		
		public UIColor DetailTextColor = UIColor.Black;
		
		public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;
		public DetailStringElement (string caption) : base (caption)
		{
			
		}

		public DetailStringElement (string caption, string value) : base (caption, value)
		{
			
		}
		
		public DetailStringElement (string caption,  string value, NSAction tapped) : base (caption, tapped)
		{
			Value = value;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (skey);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Subtitle, skey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			
			cell.Accessory = Accessory;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null) 
			{
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
				cell.DetailTextLabel.TextColor = DetailTextColor;
			}
			
			return cell;
		}
	
	}
	
	public class TripLogElement : DetailStringElement
	{
		TripLog Trip = null;
		public TripLogElement(TripLog tripLog, NSAction action) : base("","",action)
		{
			Trip = tripLog;
			Caption = string.Format("{0} to {1}", tripLog.StartStation, tripLog.EndStation);
			Value = GetValueString();
			Accessory = UITableViewCellAccessory.DisclosureIndicator;
		}
		
		public string GetValueString()
		{
			return string.Format("{0}, {1} on {2}", Trip.DistanceForDisplay, Trip.TimeForDisplay, Trip.DateForDisplay);
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = GetValueString();
			return base.GetCell (tv);
		}
		
	}
	
	public class TflTripLogElement : DetailStringElement
	{
		TflTripLog Trip = null;
		public TflTripLogElement(TflTripLog tripLog) : base("","",null)
		{
			Trip = tripLog;
			
			if (Trip == null)
			{
				Caption = "Error downloading trip log";
				
			} else {
			
				if (Trip.Command == "Admin" || Trip.Command == "Payment" || Trip.Command == "Subscription" || Trip.Command == "Balance")
				{
					Caption = Trip.Command;
				} else 
				{
					Caption = string.Format("{0} to {1}", Trip.StartStation, Trip.EndStation);
				}
			}
			
			Value = GetValueString();
			Accessory = UITableViewCellAccessory.None;
			
		}
		
		public string GetValueString()
		{
			if (Trip == null)
			{
				return "Please try again later";
				
			} else {
			
				if (Trip.Command == "Balance")
				{
					return string.Format("£{1}", Trip.StartTime, Trip.Cost);
				} else if (Trip.Command == "Admin" || Trip.Command == "Payment" || Trip.Command == "Subscription" )
				{
					return string.Format("{0}, £{1}", Trip.StartTime, Trip.Cost);
				} else 
				{
					return string.Format("{0}-{1}, £{2}", Trip.StartTime, Trip.EndTime, Trip.Cost);
				}
			}
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = GetValueString();
			var cell = base.GetCell (tv);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			return cell;
		}
		
	}
	
	
	public class BikeElement : DetailStringElement
	{
		BikeLocation Bike = null;
		public BikeElement(BikeLocation bike, NSAction action) : base("","",action)
		{
			Bike = bike;
			Caption = bike.Name;
			Value = GetValueString();
			Accessory = UITableViewCellAccessory.DisclosureIndicator;
		}
		
		public string GetValueString()
		{
			string subitem = "";
			if (!Bike.IsAvailable) 
			{
				subitem = "Docking point may not be available yet";
			} else if (Bike.DocksAvailable == -1)
			{
				subitem = string.Format("{0} {1}, {2} docks.", Bike.DistanceFromCurrentPointForDisplay, Bike.HeadingAsString, Bike.Capacity);
			} else {
				subitem = string.Format("{0} {1}, {2} docks, {3} bikes/{4} free docks", Bike.DistanceFromCurrentPointForDisplay, Bike.HeadingAsString, Bike.Capacity, Bike.BikesAvailable,  Bike.DocksAvailable);
				
				if (Bike.BikesAvailable == 0 || Bike.DocksAvailable == 0)
				{
					DetailTextColor = UIColor.Red;
				} else if (Bike.BikesAvailable < 5 || Bike.DocksAvailable < 5)
				{
					DetailTextColor = UIColor.Purple;
				} else{
					DetailTextColor = UIColor.Black;
				}
			}
			return subitem;
			
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			Value = GetValueString();
			
			return base.GetCell (tv);
		}
		
	}
	
	public class StyledMultilineElement : StyledStringElement, IElementSizing {
		public StyledMultilineElement (string caption) : base (caption)
		{
		}
		
		public StyledMultilineElement (string caption, string value) : base (caption, value)
		{
		}
		
		public StyledMultilineElement (string caption, NSAction tapped) : base (caption, tapped)
		{
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);				
			var tl = cell.TextLabel;
			tl.LineBreakMode = UILineBreakMode.WordWrap;
			tl.Lines = 0;
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			return cell;
		}
		
		public virtual float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			SizeF size = new SizeF (280, float.MaxValue);
			
			return tableView.StringSize (Caption, Font, size, UILineBreakMode.WordWrap).Height + 10;
		}
	}
	
}