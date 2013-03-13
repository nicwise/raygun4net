using System;

namespace Mindscape.Raygun4Net
{
	public class RaygunSettings
	{
		private static RaygunSettings settings;
		private const string DefaultApiEndPoint = "https://api.raygun.io/entries";
		
		public static RaygunSettings Settings
		{
			get
			{
				// If no configuration setting is provided then return the default values
				return settings ?? (settings = new RaygunSettings { ApiKey = "", ApiEndpoint = new Uri(DefaultApiEndPoint), MediumTrust = false });
			}
		}
		
		public string ApiKey { get; set; }

		public Uri ApiEndpoint { get; set; }
		
		public bool MediumTrust { get; set; }
	}
}

