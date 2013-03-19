using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;




namespace Mindscape.Raygun4Net.Messages
{



	public class RaygunEnvironmentMessage
	{
		private List<double> _diskSpaceFree = new List<double>();
		
		public RaygunEnvironmentMessage()
		{

	
			/*




			ProcessorCount = Environment.ProcessorCount;
			

			WindowBoundsWidth = SystemInformation.VirtualScreen.Height;
			WindowBoundsHeight = SystemInformation.VirtualScreen.Width;
			ComputerInfo info = new ComputerInfo();
			Locale = CultureInfo.CurrentCulture.DisplayName;
			OSVersion = info.OSVersion;
			
			if (!RaygunSettings.Settings.MediumTrust)
			{
				try
				{
					Architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
					TotalPhysicalMemory = (ulong) info.TotalPhysicalMemory/0x100000; // in MB
					AvailablePhysicalMemory = (ulong) info.AvailablePhysicalMemory/0x100000;
					TotalVirtualMemory = info.TotalVirtualMemory/0x100000;
					AvailableVirtualMemory = info.AvailableVirtualMemory/0x100000;
					GetDiskSpace();
					Cpu = GetCpu();
				}
				catch (SecurityException)
				{
					System.Diagnostics.Trace.WriteLine("RaygunClient error: couldn't access environment variables. If you are running in Medium Trust, in web.config in RaygunSettings set mediumtrust=\"true\"");
				}
			}
			*/
		}
		
	
		private string GetCpu()
		{

			return "";


		}
		
		private void GetDiskSpace()
		{
			foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				if (drive.IsReady)
				{
					DiskSpaceFree.Add((double)drive.AvailableFreeSpace / 0x40000000); // in GB
				}
			}
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