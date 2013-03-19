using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using MonoTouch.UIKit;
using MonoTouch.Foundation;



namespace Mindscape.Raygun4Net.Messages
{
	
	public class DeviceHardware
	{
		public const string HardwareProperty = "hw.machine";
		public const string PhysicalMemoryProperty = "hw.physmem";
		public const string PhysicalMemoryFreeProperty = "hw.physmem";
		public const string CpuCountProperty = "hw.ncpu";
		
		
		
		// Changing the constant to "/usr/bin/libSystem.dylib" allows this P/Invoke to work on Mac OS X
		// Using "hw.model" as property gives Macintosh model, "hw.machine" kernel arch (ppc, ppc64, i386, x86_64)
		[DllImport(global::MonoTouch.Constants.SystemLibrary)]
		internal static extern int sysctlbyname( [MarshalAs(UnmanagedType.LPStr)] string property, // name of the property
		                                        IntPtr output, // output
		                                        IntPtr oldLen, // IntPtr.Zero
		                                        IntPtr newp, // IntPtr.Zero
		                                        uint newlen // 0
		                                        );
		
		public static string Version
		{
			get
			{
				// get the length of the string that will be returned
				var pLen = Marshal.AllocHGlobal(sizeof(int));
				sysctlbyname(DeviceHardware.HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);
				
				var length = Marshal.ReadInt32(pLen);
				
				// check to see if we got a length
				if (length == 0)
				{
					Marshal.FreeHGlobal(pLen);
					return "Unknown";
				}
				
				// get the hardware string
				var pStr = Marshal.AllocHGlobal(length);
				sysctlbyname(DeviceHardware.HardwareProperty, pStr, pLen, IntPtr.Zero, 0);
				
				// convert the native string into a C# string
				var hardwareStr = Marshal.PtrToStringAnsi(pStr);
				
				var ret = hardwareStr;
				
				// cleanup
				Marshal.FreeHGlobal(pLen);
				Marshal.FreeHGlobal(pStr);
				
				return ret;
			}
		}

		public static uint PhysicalMemory
		{
			get
			{
				return GetIntSysCtl(DeviceHardware.PhysicalMemoryProperty);
			}
		}

		public static uint PhysicalMemoryFree
		{
			get
			{
				return GetIntSysCtl(DeviceHardware.PhysicalMemoryFreeProperty);
			}
		}

		public static int CpuCount
		{
			get
			{
				return (int)GetIntSysCtl(DeviceHardware.CpuCountProperty);


			}
		}

		static uint GetIntSysCtl(string propertyName)
		{
			// get the length of the string that will be returned
			var pLen = Marshal.AllocHGlobal(sizeof(int));
			sysctlbyname(propertyName, IntPtr.Zero, pLen, IntPtr.Zero, 0);
			
			var length = Marshal.ReadInt32(pLen);
			
			// check to see if we got a length
			if (length == 0)
			{
				Marshal.FreeHGlobal(pLen);
				return 0;
			}
			
			// get the hardware string
			var pStr = Marshal.AllocHGlobal(length);
			sysctlbyname(propertyName, pStr, pLen, IntPtr.Zero, 0);
			
			// convert the native string into a C# string
			
			var memoryCount = Marshal.ReadInt32(pStr);
			uint memoryVal = (uint)memoryCount;
			
			if (memoryCount < 0)
			{
				memoryVal = (uint)((uint)int.MaxValue + (-memoryCount));
			}
			
			var ret = memoryVal;
			
			// cleanup
			Marshal.FreeHGlobal(pLen);
			Marshal.FreeHGlobal(pStr);
			
			return ret;
		}

	}
	
	
	public class RaygunEnvironmentMessage
	{
		private List<double> _diskSpaceFree = new List<double>();

		public static string AppVersion
		{
			get
			{
				return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
			}
		}
		
		public RaygunEnvironmentMessage()
		{
			
			ProcessorCount = DeviceHardware.CpuCount;
			WindowBoundsWidth = UIScreen.MainScreen.Bounds.Width;
			WindowBoundsHeight = UIScreen.MainScreen.Bounds.Height;
			Locale = CultureInfo.CurrentCulture.DisplayName;
			OSVersion = UIDevice.CurrentDevice.SystemVersion;
			Architecture = UIDevice.CurrentDevice.SystemName;
			Cpu = DeviceHardware.Version;

			TotalPhysicalMemory = DeviceHardware.PhysicalMemory;
			//AvailablePhysicalMemory = DeviceHardware.PhysicalMemoryFree;
			DeviceName = Environment.MachineName;
			PackageVersion = AppVersion;
  	
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