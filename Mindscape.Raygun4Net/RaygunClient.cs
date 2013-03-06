﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Phone.Net.NetworkInformation;
using Mindscape.Raygun4Net.Messages;
#if WINRT
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
#elif SILVERLIGHT
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Phone.Info;
#else
using System.Web;
using System.Threading;
#endif

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

#if WINRT
  /// <summary>
  /// Sends the exception from an UnhandledException event to Raygun.io, optionally with a list of tags
  /// for identification.
  /// </summary>
  /// <param name="unhandledExceptionEventArgs">The event args from UnhandledException, containing the thrown exception and its message.</param>
  /// <param name="tags">An optional list of strings to identify the message to be transmitted.</param>
  /// <param name="userCustomData">A key-value collection of custom data that is to be sent along with the message</param>
    public void Send(UnhandledExceptionEventArgs unhandledExceptionEventArgs, [Optional] IList<string> tags, [Optional] IDictionary userCustomData)
    {
      if (ValidateApiKey())
      {
        var exception = unhandledExceptionEventArgs.Exception;
        exception.Data.Add("Message", unhandledExceptionEventArgs.Message);

        Send(CreateMessage(exception, tags, userCustomData));
      }
    }

    /// <summary>
    /// To be called by Wrap() - little point in allowing users to send exceptions in WinRT
    /// as the object contains little useful information besides the exception name and description
    /// </summary>
    /// <param name="exception">The exception thrown by the wrapped method</param>
    /// <param name="tags">A list of string tags relating to the message to identify it</param>
    /// <param name="userCustomData">A key-value collection of custom data that is to be sent along with the message</param>
    private void Send(Exception exception, [Optional] IList<string> tags, [Optional] IDictionary userCustomData)
    {
      if (ValidateApiKey())
      {
        Send(CreateMessage(exception, tags, userCustomData));
      }
    }

    public async void Send(RaygunMessage raygunMessage)
    {
      HttpClientHandler handler = new HttpClientHandler {UseDefaultCredentials = true};

      var client = new HttpClient(handler);
      {
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("raygun4net-winrt", "1.0.0"));

        HttpContent httpContent = new StringContent(SimpleJson.SerializeObject(raygunMessage));
        //HttpContent httpContent = new StringContent(JObject.FromObject(raygunMessage, new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore }).ToString());
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-raygun-message");
        httpContent.Headers.Add("X-ApiKey", _apiKey);

        try
        {
          await PostMessageAsync(client, httpContent, RaygunSettings.Settings.ApiEndpoint);
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine(string.Format("Error Logging Exception to Raygun.io {0}", ex.Message));
        }
      }
    }

    private RaygunMessage CreateMessage(Exception exception, [Optional] IList<string> tags, [Optional] IDictionary userCustomData)
    {
      var message = RaygunMessageBuilder.New
          .SetEnvironmentDetails()
          .SetMachineName(NetworkInformation.GetHostNames()[0].DisplayName)
          .SetExceptionDetails(exception)
          .SetClientDetails()
          .SetVersion()
          .Build();

      if (tags != null)
      {
        message.Details.Tags = tags;
      }
      if (userCustomData != null)
      {
        message.Details.UserCustomData = userCustomData;
      }
      return message;
    }

#pragma warning disable 1998
    private async Task PostMessageAsync(HttpClient client, HttpContent httpContent, Uri uri)
#pragma warning restore 1998
    {
      HttpResponseMessage response;
      response = client.PostAsync(uri, httpContent).Result;
      client.Dispose();
    }

    public void Wrap(Action func, [Optional] List<string> tags)
    {
      try
      {
        func();
      }
      catch (Exception ex)
      {
        Send(ex);
        throw;
      }
    }

    public TResult Wrap<TResult>(Func<TResult> func, [Optional] List<string> tags)
    {
      try
      {
        return func();
      }
      catch (Exception ex)
      {
        Send(ex);
        throw;
      }
    }
#elif SILVERLIGHT
    public void Send(ApplicationUnhandledExceptionEventArgs args)
    {
      if (ValidateApiKey())
      {
        var exception = args.ExceptionObject;
        exception.Data.Add("Message", exception.Message);

        Send(CreateMessage(exception, null, null));
      }
    }

    private WebClient _client;
    private string _message;
    private bool _running;
    private bool _running2;

    private ManualResetEvent _manualReset = new ManualResetEvent(false);

    public void Send(RaygunMessage raygunMessage)
    {
      if (ValidateApiKey())
      {
        //_client = new WebClient();
        //_client.Headers["X-ApiKey"] =  _apiKey;
        //_client.Headers[HttpRequestHeader.ContentType] = "application/x-raygun-message";
        //_client.Encoding = System.Text.Encoding.UTF8;
        //_client.UploadStringCompleted += ClientOnUploadStringCompleted;
        //_client.UploadProgressChanged += ClientOnUploadProgressChanged;

        try
        {
          if (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None)
          {
            _message = SimpleJson.SerializeObject(raygunMessage);
            //_client.Headers[HttpRequestHeader.ContentLength] = _message.Length.ToString();

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(RaygunSettings.Settings.ApiEndpoint);
            httpWebRequest.ContentType = "application/x-raygun-message";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers["X-Apikey"] = _apiKey;
            _running = true;
            httpWebRequest.BeginGetRequestStream(RequestReady, httpWebRequest);

            while (_running)
            {
              Thread.Sleep(1000);
            }

            _running = true;
            try
            {
              httpWebRequest.BeginGetResponse(ResponseReady, httpWebRequest);
              while (_running)
              {
                Thread.Sleep(1000);
              }
            }
            catch (Exception ex)
            {
              Debug.WriteLine("Error Logging Exception to Raygun.io " + ex.Message);
            }

            /*_running = true;
            Thread thread = new Thread(PostMessageAsync);
            thread.Start();
            //PostMessageAsync();
            //WaitHandle.WaitAny(new WaitHandle[] {new ManualResetEvent(false) });
            int count = 0;
            while (_client.IsBusy)
            {
              _manualReset.WaitOne();
            }
            /*while (_running)
            {
              Debug.WriteLine(_client.IsBusy);
              Debug.WriteLine(thread.IsAlive);
              Debug.WriteLine(thread.IsBackground);
              Debug.WriteLine(thread.ThreadState);
              Debug.WriteLine("");
              Thread.Sleep(1000);
              count++;
              if (count > 10)
              {
                //break;
              }
            }*/
          }
          else
          {
            throw new InvalidOperationException("No internet connection!");
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(string.Format("Error Logging Exception to Raygun.io {0}", ex.Message));
        }
      }

      
      /*
      wc.UploadStringAsync();

      var httpWebRequest = (HttpWebRequest)WebRequest.Create(RaygunSettings.Settings.ApiEndpoint);
      httpWebRequest.ContentType = "application/x-raygun-message";
      httpWebRequest.Method = "POST";
      httpWebRequest.
      using (var stream = await Task)

      HttpClientHandler handler = new HttpClientHandler { UseDefaultCredentials = true };

      var client = new HttpClient(handler);
      {
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("raygun4net-winrt", "1.0.0"));

        HttpContent httpContent = new StringContent(SimpleJson.SerializeObject(raygunMessage));
        //HttpContent httpContent = new StringContent(JObject.FromObject(raygunMessage, new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore }).ToString());
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-raygun-message");
        httpContent.Headers.Add("X-ApiKey", _apiKey);

        try
        {
          await PostMessageAsync(client, httpContent, RaygunSettings.Settings.ApiEndpoint);
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.WriteLine(string.Format("Error Logging Exception to Raygun.io {0}", ex.Message));
        }
      }*/
    }

    private void RequestReady(IAsyncResult asyncResult)
    {
      HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;

      using (Stream stream = request.EndGetRequestStream(asyncResult))
      {
        using (StreamWriter writer = new StreamWriter(stream))
        {
          writer.Write(_message);
          writer.Flush();
          writer.Close();
        }
      }

      _running = false;

      _running2 = true;
      while (_running2)
      {
        Thread.Sleep(1000);
      }
    }

    private void ResponseReady(IAsyncResult asyncResult)
    {
      HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
      HttpWebResponse response = null;

      try
      {
        response = (HttpWebResponse)request.EndGetResponse(asyncResult);

        string result = string.Empty;
        using (Stream responseStream = response.GetResponseStream())
        {
          using (StreamReader reader = new StreamReader(responseStream))
          {
            result = reader.ReadToEnd();
          }
        }

        /*if (DownloadStringCompleted != null)
        {
          System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
          {
            DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(result));
          });
        }*/
      }
      catch
      {
        /*if (DownloadStringCompleted != null)
        {
          System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
          {
            DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(new Exception("Error getting HTTP web response.")));
          });
        }*/
      }
      _running = false;
      _running2 = false;
    }

    private void PostMessageAsync()
    {
      _client.UploadStringAsync(RaygunSettings.Settings.ApiEndpoint, "POST", _message);
      //_manualReset.WaitOne();
      /*while (_client.IsBusy)
      {
        Thread.Sleep(1000);
      }*/
    }

    private void ClientOnUploadProgressChanged(object sender, UploadProgressChangedEventArgs uploadProgressChangedEventArgs)
    {
      Debug.WriteLine(string.Format("Progress: {0} ", uploadProgressChangedEventArgs.ProgressPercentage));
    }

    private void ClientOnUploadStringCompleted(object sender, UploadStringCompletedEventArgs uploadStringCompletedEventArgs)
    {
      _manualReset.Set();
      _running = false;
    }

    private RaygunMessage CreateMessage(Exception exception, IList<string> tags, IDictionary userCustomData)
    {
      object deviceName;
      DeviceExtendedProperties.TryGetValue("DeviceName", out deviceName);

      var message = RaygunMessageBuilder.New
          .SetEnvironmentDetails()
          .SetMachineName(deviceName.ToString())
          .SetExceptionDetails(exception)
          .SetClientDetails()
          .SetVersion()
          .Build();

      if (tags != null)
      {
        message.Details.Tags = tags;
      }
      if (userCustomData != null)
      {
        message.Details.UserCustomData = userCustomData;
      }
      return message;
    }
#else
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
        .SetHttpDetails(HttpContext.Current)
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
            System.Diagnostics.Trace.WriteLine(string.Format("Error Logging Exception to Raygun.io {0}", ex.Message));
          }
        }
      }
    }
#endif
  }
}
