using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.CoreLocation;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.ObjCRuntime;



namespace LondonBike
{
	public enum DisplayMode {
		Bikes,
		Docks
	}
	
	/// <summary>
	/// Shows the map - ment to work inside the tabview controller
	/// </summary>
	public class MapViewController : UIViewController
	{
		MKMapView mapView = null;
		UIAlertView alert;
		ActivityIndicatorLoadingView loadingView = null;
		CSRouteAnnotation routeAnnotation = null;
		bool DistanceViewIsShowing = false;
		UISegmentedControl segmentControl = null;
		DisplayMode CurrentDisplayMode = DisplayMode.Bikes;
		UIBarButtonItem gpsButton = null;
		
		
		public MapViewController () : base()
		{
			BuildView();
			
			NavigationItem.Title = "Map";
			
			
			
			segmentControl = new UISegmentedControl(new RectangleF(0,0,200,25));
			segmentControl.InsertSegment("Find bikes", 0, false);
			segmentControl.InsertSegment("Find docks", 1, false);
			segmentControl.SelectedSegment = 0;
			segmentControl.ControlStyle = UISegmentedControlStyle.Bar;
			segmentControl.ValueChanged += delegate(object sender, EventArgs e) {
				if (segmentControl.SelectedSegment == 0) 
				{
					CurrentDisplayMode = DisplayMode.Bikes;
					Analytics.TrackEvent(Analytics.CATEGORY_ACTION, Analytics.ACTION_MAP_BIKES, "", 1);
				} else
				{
					CurrentDisplayMode = DisplayMode.Docks;
					Analytics.TrackEvent(Analytics.CATEGORY_ACTION, Analytics.ACTION_MAP_DOCKS, "", 1);
				}
				
				RefreshPinColours();
			
				BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
							RefreshPinColours();
							//RefreshData();	
						});
					});
			};
			
			
			NavigationItem.TitleView = segmentControl;
			
			
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Resources.Routing, UIBarButtonItemStyle.Bordered, delegate {
				
				//nulls, think about the nulls!
				
				if (!CLLocationManager.LocationServicesEnabled) 
				{
					alert = new UIAlertView("No Location Available", "Sorry, no location services are available. However, you may still be able to use the timer and map.", null, "Ok");
					alert.Show();
					return;
				}
				
				if (mapView.UserLocation == null || mapView.UserLocation.Location == null)
				{
					alert = new UIAlertView("No Location Available", "Sorry, your location is not yet available.", null, "Ok");
					alert.Show();
					return;
				}
				
				
				
				NSObject[] selectedPins = mapView.SelectedAnnotations;
				
				if (selectedPins == null || selectedPins.Length > 1)
				{
					alert = new UIAlertView("Select a dock", "Please pick a cycle docking station to route to.", null, "Ok");
					alert.Show();
					return;
				}
				
				
				
				if (selectedPins.Length == 1) 
				{
					CycleAnnotation ca = selectedPins[0] as CycleAnnotation;
					if (ca != null) 
					{
						HideDistanceView();
						
						var location = mapView.UserLocation.Location.Coordinate;
#if DEBUG
					location = Locations.BanksideMix;
#endif
						
						double distance = BikeLocation.CalculateDistanceInMeters(location, ca.Coordinate);
						if (distance > 50000)
						{
							alert = new UIAlertView("Sorry, your route is too long", "We can only plot cycle routes up to 50km.", null, "Ok");
							alert.Show();
							
							
							return;
						}
						
						
						
						loadingView = new ActivityIndicatorLoadingView();
						loadingView.Show("Finding your route");
						Util.TurnOnNetworkActivity();
						
						
						RemoveRouteAnnotation();
						
						
							
						
						ThreadPool.QueueUserWorkItem(delegate {
							
							using (NSAutoreleasePool newPool = new NSAutoreleasePool())
							{
								location = mapView.UserLocation.Location.Coordinate;
#if DEBUG
					location = Locations.BanksideMix;
#endif
								
								MapRouting routing = new MapRouting(location, ca.Coordinate);

								
								
								
								//routing.FindRoute(delegate {
								routing.FindRoute(delegate {
									InvokeOnMainThread(delegate {
										//Console.WriteLine("updating");
										loadingView.Hide();
										Util.TurnOffNetworkActivity();
										if (routing.HasRoute) 
										{
											
											
										    routeAnnotation = new CSRouteAnnotation(routing.PointsList);
											mapView.AddAnnotation(routeAnnotation);
											
											var region = routeAnnotation.Region;
											
											region.Span = new MKCoordinateSpan(region.Span.LatitudeDelta * 1.1f, region.Span.LongitudeDelta * 1.1f);
											
											
											mapView.SetRegion(region, true);
											
											
											//need to animate the distance etc here.
											
											
											ShowDistanceView(routing.DistanceForDisplay, routing.TimeForDisplay);
											
											BikeLocation.LogRoute();
											
											
										} else {
											alert = new UIAlertView("No route found", "Sorry, no route could be found or the route is too long.", null, "Ok");
											alert.Show();
											
											
										}
									});
								});
							
							}
							
							
						});
						
						
						
					}
				}
			});
			
			//NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
			gpsButton = new UIBarButtonItem(Resources.Gps, UIBarButtonItemStyle.Bordered, delegate {
			
				
				if (!CLLocationManager.LocationServicesEnabled) 
				{
					alert = new UIAlertView("No Location Available", "Sorry, no location services are available. However, you may still be able to use the timer and map.", null, "Ok");
					alert.Show();
					return;
				}
					
				if (mapView.UserLocation != null)
				{
					if (mapView.UserLocation.Location != null) 
					{
						//NavigationItem.RightBarButtonItem = activityButton;
						
						BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
								RefreshPinColours();
								//NavigationItem.RightBarButtonItem = gpsButton;
							});
						});
					
						CLLocationCoordinate2D location = mapView.UserLocation.Location.Coordinate;
#if DEBUG
					location = Locations.BanksideMix;
#endif
						MKCoordinateRegion region = new MKCoordinateRegion(location, new MKCoordinateSpan(0.01, 0.01));
						
						region = mapView.RegionThatFits(region);
						mapView.SetRegion(region, true);
						
					}
				}
			});
			
			
			NavigationItem.RightBarButtonItem = gpsButton;
			
			
		}
		
		public void ShowDistanceView(string distance, string time)
		{
			DV.TextToShow(string.Format("{0},  {1}", distance, time));
			DV.View.Alpha = 0;
			
			DistanceViewIsShowing = true;
			
			
			mapView.AddSubview(DV.View);
			mapView.BringSubviewToFront(DV.View);
			
			
			UIView.BeginAnimations("foo");
			UIView.SetAnimationDuration(0.25f);
			
			DV.View.Alpha = 0.75f;
			var frame = DV.View.Frame;
			//frame.Y -= DV.View.Frame.Height;
			frame.Y += DV.View.Frame.Height;
			
			DV.View.Frame = frame;
			
			UIView.CommitAnimations();
			
			
		}
		
		public void HideDistanceView()
		{
			if (!DistanceViewIsShowing) return;
			
			if (DV != null)
			{
				if (DV.View.Alpha != 0f) 
				{
					
					
					UIView.BeginAnimations("foo2");
					UIView.SetAnimationDuration(0.25f);
					UIView.SetAnimationDidStopSelector(new Selector("removedistanceview"));
					
					DV.View.Alpha = 0;
					
					var frame = DV.View.Frame;
					//frame.Y += DV.View.Frame.Height;
				
					frame.Y -= DV.View.Frame.Height;
					
					DV.View.Frame = frame;
					
					
					UIView.CommitAnimations();
				
				}
				
				
			}
		}
		
		[Export("removedistanceview")]
		public void RemoveDistanceView()
		{
			DistanceViewIsShowing = false;
			
			DV.View.RemoveFromSuperview();
		}
			
		
		public override void ViewDidAppear (bool animated)
		{
			Util.Log("MapView: Appear");
			NavigationController.NavigationBar.TintColor = Resources.DarkBlue;
			
			
			//if (gpsButton != null) NavigationItem.RightBarButtonItem = gpsButton;
			
			BikeLocation.UpdateFromWebsite(delegate {
						InvokeOnMainThread(delegate{
							RefreshPinColours();
							//RefreshData();	
						});
					});
			
			if (userLocationWasOn) mapView.ShowsUserLocation = true;
			userLocationWasOn = false;
			
		}
		
		bool userLocationWasOn = false;
		
		public override void ViewWillDisappear (bool animated)
		{
			Util.Log("MapDVC: Disappear");
			base.ViewWillDisappear (animated);
			userLocationWasOn = false;
			if (mapView != null) 
			{
				if (mapView.ShowsUserLocation) userLocationWasOn = true;
				mapView.ShowsUserLocation = false;
			}
		}
		
		public void FocusOnLocation(BikeLocation bike)
		{
			HideDistanceView();
			
			RemoveRouteAnnotation();
			
			foreach(CycleAnnotation an in mapView.Annotations)
			{
				if (an.Bike == bike)
				{
					
					MKCoordinateRegion region = new MKCoordinateRegion(an.Bike.Location, new MKCoordinateSpan(0.01, 0.01));
					
					region = mapView.RegionThatFits(region);
					
					mapView.SetRegion(region, true);
					mapView.SelectAnnotation(an, true);
					
					
					
					mapView.ShowsUserLocation = true;
					return;
				}
			}
		}
		static NSString MapPin = new NSString("cyclemappin");
		
		public CSRouteView routeView = null;
		public DistanceView DV = null;
		
		public void RemoveRouteAnnotation()
		{
			if (routeAnnotation != null)
			{
				mapView.RemoveAnnotation(routeAnnotation);
				routeAnnotation = null;
			}
		}
		
		public void RefreshPinColours()
		{
			if (mapView != null)
			{
				if (mapView.Annotations != null) 
				{
					foreach(var annotation in mapView.Annotations) 
					{
						if (annotation is CycleAnnotation)
						{
							
							var mapAnnotation = annotation as CycleAnnotation;
							if (mapAnnotation == null) continue;
							
							
							if (mapAnnotation.Bike.Touched == false)
							{
								mapView.RemoveAnnotation(mapAnnotation);
								continue;
							}
							int valueToCheck = 0;
							if (CurrentDisplayMode == DisplayMode.Bikes)
							{
								valueToCheck = mapAnnotation.Bike.BikesAvailable;
							} else {
								valueToCheck = mapAnnotation.Bike.DocksAvailable;
							}
							
							if (mapAnnotation != null && mapAnnotation.PinView != null) 
							{
								if ((valueToCheck < 5 && valueToCheck != -1)) {
									if (valueToCheck == 0)
									{
										mapAnnotation.PinView.PinColor = MKPinAnnotationColor.Red;
									} else {
										mapAnnotation.PinView.PinColor = MKPinAnnotationColor.Purple;
									}
								} else {
									mapAnnotation.PinView.PinColor = MKPinAnnotationColor.Green;
								}
							}
							
						}
					}
				}
					
				mapView.SetNeedsDisplay(); 
				
			}
		}
		
		public void BuildView()
		{
			if (mapView == null) 
			{
				
				mapView = new MKMapView();
				RectangleF frame = new RectangleF(0,0,320,367);
				
				mapView.Frame = frame;
				
				
				
				DV = new DistanceView();
				
				frame = DV.View.Frame;
				//frame.Y = mapView.Frame.Bottom;
				frame.Y = -DV.View.Frame.Height;
				
				
				DV.View.Frame = frame;
				
				DV.TouchUpInside += delegate(object sender, EventArgs e) {
					
					RemoveRouteAnnotation();
					
					HideDistanceView();
				};
				
				mapView.RegionWillChange += delegate(object sender, MKMapViewChangeEventArgs e) {
					if (routeView != null)
					{
						routeView.Hidden = true;
					}
				};
				
				mapView.RegionChanged += delegate(object sender, MKMapViewChangeEventArgs e) {
					if (routeView != null)
					{
						routeView.Hidden = false;
						routeView.RegionChanged();
					}
				};
				
				mapView.GetViewForAnnotation = delegate(MKMapView mapViewForAnnotation, NSObject annotation) {
					if (annotation is MKUserLocation) return null;
					
					if (annotation is CycleAnnotation)
					{
						var mapAnnotation = annotation as CycleAnnotation;
						if (mapAnnotation == null) return null;
						
						
						MKPinAnnotationView pinView = (MKPinAnnotationView)mapViewForAnnotation.DequeueReusableAnnotation(MapPin);
						if (pinView == null) 
						{
							pinView = new MKPinAnnotationView(mapAnnotation, MapPin);
						} else {
							pinView.Annotation = annotation;
						}
						
						
						
						int valueToCheck = 0;
						if (CurrentDisplayMode == DisplayMode.Bikes)
						{
							valueToCheck = mapAnnotation.Bike.BikesAvailable;
						} else {
							valueToCheck = mapAnnotation.Bike.DocksAvailable;
						}
						
						if ((valueToCheck < 5 && valueToCheck != -1)) {
							if (valueToCheck == 0)
							{
								pinView.PinColor = MKPinAnnotationColor.Red;
							} else {
								pinView.PinColor = MKPinAnnotationColor.Purple;
							}
						} else {
							pinView.PinColor = MKPinAnnotationColor.Green;
						}
						
						mapAnnotation.PinView = pinView;
						
						
						pinView.CanShowCallout = true;
						return pinView;
					}
					
					if (annotation is CSRouteAnnotation)
					{
						var routeAnnotation = annotation as CSRouteAnnotation;
						MKAnnotationView annotationView = null;
						
						
						if (annotationView == null)
						{
							routeView = new CSRouteView(new RectangleF (0,0, mapView.Frame.Size.Width, mapView.Frame.Size.Height));
							routeView.Annotation = routeAnnotation;
							routeView.MapView = mapViewForAnnotation;
							annotationView = routeView;
						}
						
						return annotationView;
					}
					
					return null;
					
				};
				
				
				
				
				List<MKAnnotation> locations = new List<MKAnnotation>();
				
				double minLon = 200, minLat = 200, maxLon = -200, maxLat = -200;
				
				
				foreach(var bike in BikeLocation.AllBikes)
				{
					if (bike.Location.Longitude < minLon) minLon = bike.Location.Longitude;
					if (bike.Location.Latitude < minLat) minLat = bike.Location.Latitude;
					
					if (bike.Location.Longitude < maxLon) maxLon = bike.Location.Longitude;
					if (bike.Location.Latitude > maxLat) maxLat = bike.Location.Latitude;
					
					locations.Add(new CycleAnnotation(bike));
				}
				
				
				
				
				if (locations.Count > 0)
				{
					mapView.AddAnnotation(locations.ToArray());
				
				
				
					var tl = new CLLocationCoordinate2D(-90, 180);
					var br = new CLLocationCoordinate2D(90, -180);
					
					foreach(MKAnnotation an in mapView.Annotations)
					{
						tl.Longitude = Math.Min(tl.Longitude, an.Coordinate.Longitude);
						tl.Latitude = Math.Max(tl.Latitude, an.Coordinate.Latitude);
						
						br.Longitude = Math.Max(br.Longitude, an.Coordinate.Longitude);
						br.Latitude = Math.Min(br.Latitude, an.Coordinate.Latitude);
						
					}
					
					var center = new CLLocationCoordinate2D {
						Latitude = tl.Latitude - (tl.Latitude - br.Latitude) *0.5,
						Longitude = tl.Longitude - (tl.Longitude - br.Longitude) *0.5
					};
	
					var span = new MKCoordinateSpan
					{
						LatitudeDelta = Math.Abs(tl.Latitude - br.Latitude) *0.5,
						LongitudeDelta = Math.Abs(tl.Longitude - br.Longitude) *0.5
						
					};
				
				
				
				                                                             
					MKCoordinateRegion region = new MKCoordinateRegion (center, span );
					
						
					region = mapView.RegionThatFits(region);
					
					mapView.SetRegion(region, true);
				}
				
				mapView.ShowsUserLocation = true;

				
				View.AddSubview(mapView);
			}
		}
		
		
		
		public class CycleAnnotation : MKAnnotation
		{
		
			public BikeLocation Bike;
			public MKPinAnnotationView PinView;
			
			public CycleAnnotation(BikeLocation bike) : base()
			{
				Bike = bike;
			}
			public override string Title {
				get {
					return Bike.Name;
				}
			}
			
			public override string Subtitle {
				get {
					
					if (!Bike.IsAvailable) 
					{
						return "This dock may not be available";
					} else if (Bike.DocksAvailable == -1)
					{
						return string.Format("{0} docks. Availability unknown.", Bike.Capacity);
					} else {
						return string.Format("{0} docks. {1} bikes/{2} free docks", Bike.Capacity, Bike.BikesAvailable,  Bike.DocksAvailable);
					}
					
				}
			}
			
			public override CLLocationCoordinate2D Coordinate {
				get {
					return Bike.Location;
				}
				set {
				}
			}

		}
	}
}

