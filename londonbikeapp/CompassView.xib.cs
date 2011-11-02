
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LondonBike
{
	public partial class CompassView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public CompassView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public CompassView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public CompassView () : base("CompassView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.Alpha = 0.5f;
			View.Layer.CornerRadius = 48f;
		}
		
		public void UpdateCompassLabel(string newText)
		{
			CompassLabel.Text = newText;
		}
		
		
	}
}

