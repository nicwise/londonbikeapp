using System;
using MonoTouch.CoreLocation;

namespace LondonBike
{
	public class Locations
	{
#if DEBUG
		public static CLLocationCoordinate2D HydePark = new CLLocationCoordinate2D(51.50311799, -0.153520935);
		public static CLLocationCoordinate2D BanksideMix = new CLLocationCoordinate2D(51.50581776, -0.100186337);
#endif
	}
}

