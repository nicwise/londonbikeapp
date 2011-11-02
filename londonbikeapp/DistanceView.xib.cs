
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LondonBike
{
	public partial class DistanceView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public DistanceView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public DistanceView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public DistanceView () : base("DistanceView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		public void TextToShow(string text)
		{
			DistanceLabel.Text = text;
		}
		
		public event EventHandler TouchUpInside;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			
			
			DismissButton.TouchUpInside += delegate(object sender, EventArgs e) {
				if (TouchUpInside != null) TouchUpInside(sender, e);
			};
		}
		
		#endregion
	}
}

