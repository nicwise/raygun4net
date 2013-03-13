using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Mindscape.Raygun4Net.Messages;

using System.Threading;

namespace Mindscape.Raygun4Net
{
	public class RaygunClient
	{
		private readonly string _apiKey;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RaygunClient" /> class.
		/// </summary>
		/// <param name="apiKey">The API key.</param>
		public RaygunClient(string apiKey)
		{
			_apiKey = apiKey;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RaygunClient" /> class.
		/// Uses the ApiKey specified in the config file.
		/// </summary>
		public RaygunClient()
			: this(RaygunSettings.Settings.ApiKey)
		{
		}
		
		private bool ValidateApiKey()
		{
			if (string.IsNullOrEmpty(_apiKey))
			{
				System.Diagnostics.Debug.WriteLine("ApiKey has not been provided, exception will not be logged");
				return false;
			}
			return true;
		}
		

		/// <summary>
		/// Transmits an exception to Raygun.io synchronously, using the version number of the originating assembly.
		/// </summary>
		/// <param name="exception">The exception to deliver</param>
		public void Send(Exception exception)
		{      
			Send(BuildMessage(exception));
		}
		
		/// <summary>
		/// Transmits an exception to Raygun.io synchronously specifying a list of string tags associated
		/// with the message for identification. This uses the version number of the originating assembly.
		/// </summary>
		/// <param name="exception">The exception to deliver</param>
		/// <param name="tags">A list of strings associated with the message</param>
		public void Send(Exception exception, IList<string> tags)
		{
			var message = BuildMessage(exception);
			message.Details.Tags = tags;
			Send(message);
		}       
		
		/// <summary>
		/// Transmits an exception to Raygun.io synchronously specifying a list of string tags associated
		/// with the message for identification, as well as sending a key-value collection of custom data.
		/// This uses the version number of the originating assembly.
		/// </summary>
		/// <param name="exception">The exception to deliver</param>
		/// <param name="tags">A list of strings associated with the message</param>
		/// <param name="userCustomData">A key-value collection of custom data that will be added to the payload</param>
		public void Send(Exception exception, IList<string> tags, IDictionary userCustomData)
		{
			var message = BuildMessage(exception);
			message.Details.Tags = tags;      
			message.Details.UserCustomData = userCustomData;
			Send(message);
		}
		
		/// <summary>
		/// Transmits an exception to Raygun.io synchronously specifying a list of string tags associated
		/// with the message for identification, as well as sending a key-value collection of custom data.
		/// This specifies a custom version identification number.
		/// </summary>
		/// <param name="exception">The exception to deliver</param>
		/// <param name="tags">A list of strings associated with the message</param>
		/// <param name="userCustomData">A key-value collection of custom data that will be added to the payload</param>
		/// <param name="version">A custom version identifiction, associated with a particular build of your project.</param>
		public void Send(Exception exception, IList<string> tags, IDictionary userCustomData, string version)
		{
			var message = BuildMessage(exception);
			message.Details.Tags = tags;
			message.Details.UserCustomData = userCustomData;
			message.Details.Version = version;
			Send(message);
		} 
		
		/// <summary>
		/// Asynchronously transmits a message to Raygun.io.
		/// </summary>
		/// <param name="exception"></param>
		public void SendInBackground(Exception exception)
		{
			var message = BuildMessage(exception);
			
			ThreadPool.QueueUserWorkItem(c => Send(message));
		}
		
		public void SendInBackground(Exception exception, List<string> tags)
		{
			var message = BuildMessage(exception);
			message.Details.Tags = tags;
			ThreadPool.QueueUserWorkItem(c => Send(message));
		}
		
		public void SendInBackground(Exception exception, List<string> tags, string version)
		{
			var message = BuildMessage(exception);
			message.Details.Tags = tags;
			message.Details.Version = version;
			ThreadPool.QueueUserWorkItem(c => Send(message));
		}    
		
		
		internal RaygunMessage BuildMessage(Exception exception)
		{
			var message = RaygunMessageBuilder.New
				.SetEnvironmentDetails()
					.SetMachineName(Environment.MachineName)
					.SetExceptionDetails(exception)
					.SetClientDetails()
					.SetVersion()
					.Build();      
			return message;
		}
		
		public void SendInBackground(RaygunMessage raygunMessage)
		{
			ThreadPool.QueueUserWorkItem(c => Send(raygunMessage));
		}
		
		/// <summary>
		/// Posts a RaygunMessage to the Raygun.io api endpoint.
		/// </summary>
		/// <param name="raygunMessage">The RaygunMessage to send. This needs its OccurredOn property
		/// set to a valid DateTime and as much of the Details property as is available.</param>
		public void Send(RaygunMessage raygunMessage)
		{
			if (ValidateApiKey())
			{
				using (var client = new WebClient())
				{
					client.Headers.Add("X-ApiKey", _apiKey);
					client.Encoding = System.Text.Encoding.UTF8;
					
					try
					{            
						var message = SimpleJson.SerializeObject(raygunMessage);
						client.UploadString(RaygunSettings.Settings.ApiEndpoint, message);
					}
					catch (Exception ex)
					{
						Console.WriteLine(string.Format("Error Logging Exception to Raygun.io {0}", ex.Message));
					}
				}
			}
		}

	}
}
