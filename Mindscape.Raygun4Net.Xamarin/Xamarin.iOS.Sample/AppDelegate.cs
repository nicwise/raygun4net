using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Mindscape.Raygun4Net;

namespace Xamarin.iOS.Sample
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController viewController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			var vc = new UIViewController();
			vc.View.BackgroundColor = UIColor.White;
			vc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate {
				new RaygunClient().Send(new Exception("From iOS??"));

			});

			vc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem("Crash", UIBarButtonItemStyle.Plain, delegate {
				throw new Exception("This should kill the app");
				
			});

			viewController = new UINavigationController(vc);



			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

