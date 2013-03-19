using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Mindscape.Raygun4Net;

namespace Xamarin.iOS.Sample
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.

			AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => {
				if (e.ExceptionObject is Exception)
					new RaygunClient().Send(e.ExceptionObject as Exception);
			};

			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}
