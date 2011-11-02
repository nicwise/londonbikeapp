
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.CoreLocation;

namespace LondonBike
{
	public partial class TripLogDetailViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		public TripLog TripLog;
		
		public TripLogDetailViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public TripLogDetailViewController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public TripLogDetailViewController () : base("TripLogDetailViewController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		static NSString MapPin = new NSString("cyclemappin");
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			Title = "Trip";
			
			StartLabel.Text = TripLog.StartStation;
			EndLabel.Text = TripLog.EndStation;
			TimeLabel.Text = string.Format("{0}, {1}", TripLog.DistanceForDisplay, TripLog.TimeForDisplay);
			
			
			MapView.GetViewForAnnotation = delegate(MKMapView mapViewForAnnotation, NSObject annotation) {
					if (annotation is MKUserLocation) return null;
					
					if (annotation is PinAnnotation)
					{
						var mapAnnotation = annotation as PinAnnotation;
						if (mapAnnotation == null) return null;
						
						
						MKPinAnnotationView pinView = (MKPinAnnotationView)mapViewForAnnotation.DequeueReusableAnnotation(MapPin);
						if (pinView == null) 
						{
							pinView = new MKPinAnnotationView(mapAnnotation, MapPin);
						} else {
							pinView.Annotation = annotation;
						}
						
						if (mapAnnotation.IsStart) {
							pinView.PinColor = MKPinAnnotationColor.Green;
						} else {
							pinView.PinColor = MKPinAnnotationColor.Red;
						}
						
						mapAnnotation.PinView = pinView;
						
						
						pinView.CanShowCallout = true;
						return pinView;
					}
					
					
					
					return null;
					
				};
			
			
			
			
			
			List<MKAnnotation> locations = new List<MKAnnotation>();
				
			double minLon = 200, minLat = 200, maxLon = -200, maxLat = -200;
				
			if (TripLog.StartLat != -1 && TripLog.StartLon != -1) 
			{
				locations.Add(new PinAnnotation(TripLog.StartLat, TripLog.StartLon, TripLog.StartStation, true));
			}
			
			if (TripLog.EndLat != -1 && TripLog.EndLon != -1) 
			{
				locations.Add(new PinAnnotation(TripLog.EndLat, TripLog.EndLon, TripLog.EndStation, false));
			}
			
				
			foreach(var location in locations)
			{
				if (location.Coordinate.Longitude < minLon) minLon = location.Coordinate.Longitude;
				if (location.Coordinate.Latitude < minLat) minLat = location.Coordinate.Latitude;
				
				if (location.Coordinate.Longitude < maxLon) maxLon = location.Coordinate.Longitude;
				if (location.Coordinate.Latitude > maxLat) maxLat = location.Coordinate.Latitude;
				
				
			}
			
			
			CLLocationCoordinate2D tl, br;
			
			if (locations.Count > 0)
			{
			
				MapView.AddAnnotation(locations.ToArray());
				
				
				
				tl = new CLLocationCoordinate2D(-90, 180);
				br = new CLLocationCoordinate2D(90, -180);
				
				foreach(MKAnnotation an in MapView.Annotations)
				{
					tl.Longitude = Math.Min(tl.Longitude, an.Coordinate.Longitude);
					tl.Latitude = Math.Max(tl.Latitude, an.Coordinate.Latitude);
					
					br.Longitude = Math.Max(br.Longitude, an.Coordinate.Longitude);
					br.Latitude = Math.Min(br.Latitude, an.Coordinate.Latitude);
					
				}
			} else {
				tl = new CLLocationCoordinate2D(51.5282, -0.1669);
				br = new CLLocationCoordinate2D(51.4898, -0.0680);
			}
				
			var center = new CLLocationCoordinate2D {
				Latitude = tl.Latitude - (tl.Latitude - br.Latitude)  *0.5,
				Longitude = tl.Longitude - (tl.Longitude - br.Longitude) *0.5
			};

			var span = new MKCoordinateSpan
			{
				LatitudeDelta = Math.Abs(tl.Latitude - br.Latitude) * 1.05f,
				LongitudeDelta = Math.Abs(tl.Longitude - br.Longitude)  * 1.05f
				
			};
			
				
				                                                             
			MKCoordinateRegion region = new MKCoordinateRegion (center, span );
			
				
			region = MapView.RegionThatFits(region);
			
			MapView.SetRegion(region, true);
			
			
			
		}
		
		#endregion
	}
	
	
	
	public class PinAnnotation : MKAnnotation
	{
	
		
		public MKPinAnnotationView PinView;
		public double Latitude;
		public double Longitude;
		public string DisplayTitle;
		public bool IsStart = false;
		
		public PinAnnotation(double lat, double lon, string title, bool isStart) : base()
		{
			Latitude = lat;
			Longitude = lon;
			DisplayTitle = title;
			IsStart = isStart;
		}
		public override string Title {
			get {
				return DisplayTitle;
			}
		}
		
		public override string Subtitle {
			get {
				
				return "";
				
			}
		}
		
		public override CLLocationCoordinate2D Coordinate {
			get {
				return new CLLocationCoordinate2D(Latitude, Longitude);
					
			}
			set {
			}
		}

	}
	
}

