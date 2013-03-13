using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Mindscape.Raygun4Net;

namespace Xamarin.Android.Sample
{
	[Activity (Label = "Xamarin.Android.Sample", MainLauncher = true)]
	public class Activity1 : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			RaygunSettings.Settings.ApiKey = "VYZuTHHojdXvpuhWPQcesA==";


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate
			{

				new RaygunClient().Send(new Exception("From Android??"));


				button.Text = string.Format ("{0} errors logged!", count++);
			};
		}
	}
}


