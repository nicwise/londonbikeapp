//
//  CSRouteView.m
//  mapLines
//
//  Created by Craig on 5/15/09.
//  Copyright 2009 Craig Spitzkoff. All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using MonoTouch.CoreGraphics;

namespace LondonBike
{
	public class CSRouteView : MKAnnotationView
	{
		MKMapView _mapView;
		CSRouteViewInternal _internalRouteView;

		public MKMapView MapView
		{
			get{ return _mapView; }
			set	
			{
				_mapView = value;
				RegionChanged();
			}
		}
		
		public CSRouteView (RectangleF frame) : base ()
		{
			Frame = frame;
			BackgroundColor = UIColor.Clear;
			ClipsToBounds = false;

			_internalRouteView = new CSRouteViewInternal();
			_internalRouteView.RouteView = this;
			AddSubview(_internalRouteView);
		}
		public void RegionChanged ()
		{
			//Debug.WriteLine("RegionChanged");

			PointF origin = new PointF(0,0);
			origin = _mapView.ConvertPointToView (origin, this);

			_internalRouteView.Frame = new RectangleF(origin.X, origin.Y, _mapView.Frame.Size.Width, _mapView.Frame.Size.Height);
			_internalRouteView.SetNeedsDisplay();
		}
		



		class CSRouteViewInternal : UIView
		{
			public CSRouteView RouteView {get;set;}

			public override void Draw (RectangleF rect)
			{
				CSRouteAnnotation routeAnnotation = RouteView.Annotation as CSRouteAnnotation;

				if (!this.Hidden && routeAnnotation.Points != null && routeAnnotation.Points.Count > 0)
				{
					CGContext context = UIGraphics.GetCurrentContext();
					if (routeAnnotation.LineColor != null)
						routeAnnotation.LineColor = UIColor.Blue;
					

					context.SetStrokeColorWithColor(routeAnnotation.LineColor.CGColor);
					context.SetRGBFillColor (0.0f,0.0f,1.0f,1.0f);

					// Draw them with a 2.0 stroke width so they are a bit more visible
					context.SetLineWidth(5.0f);
					context.SetAlpha(0.40f);
					
					for (int idx = 0; idx < routeAnnotation.Points.Count; idx++)
					{
						CLLocation location = routeAnnotation.Points[idx];
						PointF point = RouteView.MapView.ConvertCoordinate(location.Coordinate, this);

						//Debug.WriteLine("Point: {0}, {1}", point.X, point.Y);

						if (idx == 0)
						{
							context.MoveTo(point.X, point.Y);
						}
						else
						{
							context.AddLineToPoint(point.X, point.Y);
						}
					}
					context.StrokePath ();		

					// debug. Draw the line around our view. 
					/*
					CGContextMoveToPoint(context, 0, 0);
					CGContextAddLineToPoint(context, 0, self.frame.size.height);
					CGContextAddLineToPoint(context, self.frame.size.width, self.frame.size.height);
					CGContextAddLineToPoint(context, self.frame.size.width, 0);
					CGContextAddLineToPoint(context, 0, 0);
					CGContextStrokePath(context);
					*/
				}
			}
			public CSRouteViewInternal ()
			{
				BackgroundColor = UIColor.Clear;
				ClipsToBounds = false;
			}
		}
	}
}
