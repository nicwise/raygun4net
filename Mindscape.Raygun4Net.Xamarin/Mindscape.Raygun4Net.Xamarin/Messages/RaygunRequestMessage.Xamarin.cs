using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;


namespace Mindscape.Raygun4Net.Messages
{
	public class RaygunRequestMessage
	{
		public RaygunRequestMessage()
		{

		}
		
		private static IDictionary ToDictionary(NameValueCollection nameValueCollection)
		{
			var dictionary = new Dictionary<string, string>();
			

			return dictionary;
		}
		
		public string HostName { get; set; }
		
		public string Url { get; set; }
		
		public string HttpMethod { get; set; }
		
		public string IPAddress { get; set; }
		
		public IDictionary QueryString { get; set; }
		
		public IDictionary Data { get; set; }
		
		public NameValueCollection Form { get; set; }
		
		public string RawData { get; set; }
		
		public IDictionary Headers { get; set; }
		
	}
}
