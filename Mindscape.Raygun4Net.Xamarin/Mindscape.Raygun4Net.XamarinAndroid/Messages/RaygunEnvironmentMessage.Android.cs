using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Android.Util;




namespace Mindscape.Raygun4Net.Messages
{
	
	
	
	public class RaygunEnvironmentMessage
	{
		private List<double> _diskSpaceFree = new List<double>();
		
		public RaygunEnvironmentMessage()
		{


			OSVersion = Android.OS.Build.VERSION.Sdk;

			Locale = CultureInfo.CurrentCulture.DisplayName;
			PackageVersion = RaygunClient.PackageVersion;


			//need a context to work out the screensize :(
//			WindowBoundsWidth = SystemInformation.VirtualScreen.Height;
//			WindowBoundsHeight = SystemInformation.VirtualScreen.Width;

			Architecture = Android.OS.Build.CpuAbi;
			DeviceName = string.Format ("{0} / {1} / {2}", 
			                            Android.OS.Build.Model,
			                            Android.OS.Build.Brand,
			                            Android.OS.Build.Manufacturer);


		}
		

		
		public int ProcessorCount { get; private set; }
		
		public string OSVersion { get; private set; }
		
		public double WindowBoundsWidth { get; private set; }
		
		public double WindowBoundsHeight { get; private set; }
		
		public string ResolutionScale { get; private set; }
		
		public string CurrentOrientation { get; private set; }
		
		public string Cpu { get; private set; }
		
		public string PackageVersion { get; private set; }
		
		public string Architecture { get; private set; }
		
		[Obsolete("Use Locale instead")]
		public string Location { get; private set; }
		
		public ulong TotalPhysicalMemory { get; private set; }
		
		public ulong AvailablePhysicalMemory { get; private set; }
		
		public ulong TotalVirtualMemory { get; private set; }
		
		public ulong AvailableVirtualMemory { get; private set; }
		
		public List<double> DiskSpaceFree
		{
			get { return _diskSpaceFree; }
			set { _diskSpaceFree = value; }
		}
		
		public string DeviceName { get; private set; }
		
		// Refactored properties
		
		public string Locale { get; private set; }
	}
}