using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON; // For parsing JSON; gist available at: https://gist.github.com/1411710

using System.Net; // to support all REST verbs, we currently (unity 4.0) need to use .Net's HTTPWebRequest **NOTE: this may not work beyond "desktop" devices
using System.Text; // StringBuilder, for stream
using System.IO; // stream
using System.Threading; // for ManualResetEvent &c (which we may not end up using!)
using System; // IAsyncResult

// no namespace used, per Unity quirks (monobehaviour inheritance & unityscript compatibility)

// Delegate to use with the REST requests (if not using a coroutine?)
public delegate void GhopperCmsResponseDelegate(GhopperCmsResponse response);

// todo: move the http requester code in here, and remove the asyncState stuff?
// public class GhopperCmsRequest {
	
// }

// Class: GhopperCmsResponse
// 		Used to wrap the CMS response. 
//		Could be expanded, made more modular.
public class GhopperCmsResponse {
	public readonly WWW www = null; // this should be replaced to be more modular
	public bool isFinished = false;
	public int statusCode = -1;
	public string errorMessage = "no errors yet -- the www request has not yet been started";
	public Dictionary<string, object> data = null;
	
	public GhopperCmsResponse() {}
	
	public GhopperCmsResponse(WWW unityWWWObject, Dictionary<string, object> dataDict) {
		this.www = unityWWWObject; // this should be replaced to be more modular
		this.data = dataDict; // note: may be null if there was an error
	}
}

// Class: GhopperCms
// 		Used for RESTful communication with the Grasshopper CMS.
public class GhopperCms : MonoBehaviour 
{
    public bool detailedDebug = false;

	// NOTE: this currently resorts to platform-specific timeout values, which may be kinda long
	// the WWW class does not (currently?) offer a way to set a timeout, but with a coroutine that checks the time periodically,
	// we could do our own (shorter) timeout. see http://forum.unity3d.com/threads/84003-WWW-class-timeout and http://forum.unity3d.com/threads/118133-How-long-is-the-WWW-timeout

	private const string API_VERSION = "v1";
	private const string API_ROOT = "ghpr.automatastudios.com/api/" + API_VERSION + "/";//"localhost:8000/api/" + API_VERSION + "/";
	private const string PLAYER_API_ROOT = API_ROOT + "members/";

	// Use this for initialization
	void Start () {
//		DebugTest();
	}
	
	public GhopperCmsResponse Get(string url, GhopperCmsResponseDelegate responseHandler) 
    {
        if ( detailedDebug )
	    	DebugExtras.Log(url);
	    WWW www = new WWW (url);
		GhopperCmsResponse cmsResponse = new GhopperCmsResponse(www, null);
		
	    StartCoroutine (WaitForRequest (cmsResponse, responseHandler));
		
	    return cmsResponse; 
    }
	
	// note: might want to switch to Dictionary<string,object> for consistency? (and to save the trouble of stringifying outside?)
    public GhopperCmsResponse Post(string url, Dictionary<string,object> post, GhopperCmsResponseDelegate responseHandler) {
	    WWWForm form = new WWWForm();
		
	    foreach(KeyValuePair<string,object> arg in post) {
	       form.AddField(arg.Key, arg.Value.ToString());
	    }
		
        WWW www = new WWW(url, form);
		GhopperCmsResponse cmsResponse = new GhopperCmsResponse(www, null);
		
        StartCoroutine(WaitForRequest(cmsResponse, responseHandler));
		
	    return cmsResponse; 
    }

    private IEnumerator WaitForRequest(GhopperCmsResponse cmsResponse, GhopperCmsResponseDelegate responseHandler) {
		WWW www = cmsResponse.www;
		Dictionary<string,object> jsonDict = null;
		
        yield return www;
		
        if ( detailedDebug )
		    DebugExtras.Log("www call finished.");
		if (www.text.Length > 0) {
			jsonDict = (Json.Deserialize(www.text) as Dictionary<string,object>);
		}
		
		cmsResponse.data = jsonDict;
		
        if ( detailedDebug )
        {
    		DebugExtras.Log(cmsResponse.www.url);
    		DebugExtras.Log(cmsResponse.www.text);
        }

        // check for errors
        if (www.error == null) {
            // set flag?
        } else {
            // error handling?
        }
		
		cmsResponse.isFinished = true;
		
		if (responseHandler != null) {
			responseHandler(cmsResponse);
		}
    }
	
	public GhopperCmsResponse GetPlayerData(int playerId, GhopperCmsResponseDelegate responseHandler) {
		return Get(PLAYER_API_ROOT+playerId.ToString()+"/", responseHandler);
	}

	public void DebugTest() 
    {
		DebugExtras.Log("DebugTest");
		
//		HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(PLAYER_API_ROOT);
//		webRequest.Method = "GET";
//
//		DoWithResponse(request, (response) => {
//		    var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
//		    DebugExtras.Log(body);
//		});
//		HttpRequester.Get("http://"+PLAYER_API_ROOT);
		// try existing POST here!
		Dictionary<string,object> newPlayerData = new Dictionary<string, object>();
		
		newPlayerData.Add("first_name", "Weary");
		newPlayerData.Add("last_name", "1");
		newPlayerData.Add("guest", true);
		Post(PLAYER_API_ROOT, newPlayerData, null);
	}

	// later adjustment?
	// should address the potentially blocking steps (including DNS resolution!) that take place before the async portions start
	// from: http://stackoverflow.com/a/13963255 
	// todo: adapt it to existing code

	// void DoWithResponse(HttpWebRequest request, Action<HttpWebResponse> responseAction) { 
	// 	// note: contains lambda
	//     Action wrapperAction = () =>
	//     {
	//         request.BeginGetResponse(new AsyncCallback((iar) =>
	//         {
	//             var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
	//             responseAction(response);
	//         }), request);
	//     };

	//     wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
	//     {
	//         var action = (Action)iar.AsyncState;
	//         action.EndInvoke(iar);
	//     }), wrapperAction);
	// }
}

// The following is a start towards supporting the full set of REST verbs
// it's not been sufficiently tested to merge in yet

// The RequestState class passes data across async calls.
public class HttpRequesterState {
   const int BUFFER_SIZE = 1024;
   public StringBuilder RequestData;
   public byte[] BufferRead;
   public WebRequest Request;
   public Stream ResponseStream;
	
   // Create Decoder for appropriate enconding type.
   public Decoder StreamDecode = Encoding.UTF8.GetDecoder();
      
	public HttpRequesterState() {
		DebugExtras.Log("new RequestState");
		BufferRead = new byte[BUFFER_SIZE];
		RequestData = new StringBuilder(String.Empty);
		Request = null;
		ResponseStream = null;	
   }     
}


// issues the async request via .net WebRequest
class HttpRequester {
	public static ManualResetEvent allDone = new ManualResetEvent(false);
	const int BUFFER_SIZE = 1024;

	public static void Get(string url) {
		WebRequest request = WebRequest.Create(url);
		HttpRequesterState requestState = new HttpRequesterState();
		
		request.Method = "GET";
		
		// Put the request into the state object so it can be passed around.
		requestState.Request = request;
		
		// Issue the async request.
		request.BeginGetResponse(new AsyncCallback(RespCallback), requestState);
   }
	
	public static void Post(string url, Dictionary<string,string> postData) {
		WebRequest request = WebRequest.Create(url);
		HttpRequesterState requestState = new HttpRequesterState();
		
		request.Method = "POST";
		
        // Convert POST data to a byte array.
        string postString = Json.Serialize(postData);
        byte[] byteArray = Encoding.UTF8.GetBytes(postString);
		Stream dataStream = request.GetRequestStream();
		
        // Set the ContentType property of the WebRequest.
        request.ContentType = "application/x-www-form-urlencoded";
		
        // Set the ContentLength property of the WebRequest.
        request.ContentLength = byteArray.Length;
		
        // Write the data to the request stream.
        dataStream.Write(byteArray, 0, byteArray.Length);
		
        // Close the Stream object.
        dataStream.Close();
		
		// Put the request into the state object so it can be passed around.
		requestState.Request = request;
		
		// Issue the async request.
		request.BeginGetResponse(new AsyncCallback(RespCallback), requestState);
	}

   private static void RespCallback(IAsyncResult asyncResult) {
		DebugExtras.Log("ClientGetAsync.RespCallback");
		// Get the RequestState object from the async result.
		HttpRequesterState requestState = (HttpRequesterState) asyncResult.AsyncState;
		// Get the WebRequest from RequestState.
		WebRequest request = requestState.Request;
		
		try {
			// Get response
			WebResponse webResponse = request.EndGetResponse(asyncResult);
			
			//  Start reading data from the response stream.
			Stream responseStream = webResponse.GetResponseStream();
			
			// Store the response stream in RequestState to read the stream asynchronously.
			requestState.ResponseStream = responseStream;
			
			//  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
			responseStream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), requestState); 
		} catch (WebException e) {
			HttpWebResponse response = (HttpWebResponse)e.Response;
			DebugExtras.LogError(string.Format("error handling response: {0} -- {1}", (int)response.StatusCode, e.Message)); 

			// todo: error handling?
			
		}
   }


   private static void ReadCallBack(IAsyncResult asyncResult) {
		DebugExtras.Log(">>>> ClientGetAsync.ReadCallBack");
		// Get the RequestState object from AsyncResult.
		HttpRequesterState rs = (HttpRequesterState)asyncResult.AsyncState;
		
		// Retrieve the ResponseStream that was set in RespCallback. 
		Stream responseStream = rs.ResponseStream;
		
		// Read rs.BufferRead to verify that it contains data. 
		int read = responseStream.EndRead( asyncResult );
		if (read > 0) {
			// there's more to finish reading
			DebugExtras.Log("there's more stream to get, I guess");
			// Prepare a Char array buffer for converting to Unicode.
			Char[] charBuffer = new Char[BUFFER_SIZE];
			
			// Convert byte stream to Char array and then to String.
			// len contains the number of characters converted to Unicode.
			// int len = rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);
            rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

			// String str = new String(charBuffer, 0, len);
			
			// Append the recently read data to the RequestData stringbuilder
			// object contained in RequestState.
			rs.RequestData.Append(
			Encoding.ASCII.GetString(rs.BufferRead, 0, read));         
			
			// Continue reading data until 
			// responseStream.EndRead returns –1.
			responseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), rs);
			
		} else {
			
			// we're done!
			if (rs.RequestData.Length>0) {
				//  Display data to the console.
				string strContent;                  
				strContent = rs.RequestData.ToString();
				DebugExtras.Log("We got data! "+strContent);
				DebugExtras.Log(strContent.Length);
			} else {
				DebugExtras.Log("We didn't get any data...");
			}
				
			// Close down the response stream.
			responseStream.Close();
			
			// Set the ManualResetEvent so the main thread can exit. <-- (not quite right, as we aren't blocking this way)
			// allDone.Set();                           
		}
	}    
}


// below reference is lightly adapted from: http://msdn.microsoft.com/en-us/library/86wf6409%28v=vs.71%29.aspx

// // The RequestState class passes data across async calls.
// public class RequestState {
//    const int BUFFER_SIZE = 1024;
//    public StringBuilder RequestData;
//    public byte[] BufferRead;
//    public WebRequest Request;
//    public Stream ResponseStream;
	
//    // Create Decoder for appropriate enconding type.
//    public Decoder StreamDecode = Encoding.UTF8.GetDecoder();
      
//    public RequestState() {
// 		DebugExtras.Log("new RequestState");
//       BufferRead = new byte[BUFFER_SIZE];
//       RequestData = new StringBuilder(String.Empty);
//       Request = null;
//       ResponseStream = null;
		
//    }     
// }


// // ClientGetAsync issues the async request.
// class ClientGetAsync {
//    public static ManualResetEvent allDone = new ManualResetEvent(false);
//    const int BUFFER_SIZE = 1024;

//    // public static void Main(string[] args) {
//    	public static void OrigDemo(string url) {
// 		DebugExtras.Log("ClientGetAsync.OrigDemo");
// 		Uri uri = new Uri(url);
//       // Create the request object.
//       WebRequest wreq = WebRequest.Create(url);
        
//       // Create the state object.
//       RequestState rs = new RequestState();

//       // Put the request into the state object so it can be passed around.
//       rs.Request = wreq;
// 		DebugExtras.Log(wreq.RequestUri);
// 		DebugExtras.Log(wreq.Method);
// 		DebugExtras.Log(rs.RequestData);

//       // Issue the async request.
//       IAsyncResult r = (IAsyncResult) wreq.BeginGetResponse(
//          new AsyncCallback(RespCallback), rs);

//       // Wait until the ManualResetEvent is set so that the application 
//       // does not exit until after the callback is called.
// //      allDone.WaitOne(); // NOTE: This does not seem to play nicely with Unity -- it seems to cause the app to stall.
// 		// although the situation it seeks to prevent seems unlikely in normal use, it's worth investigating the consequences
//    }

//    private static void RespCallback(IAsyncResult ar) {
// 		DebugExtras.Log("ClientGetAsync.RespCallback");
// 		// Get the RequestState object from the async result.
// 		RequestState rs = (RequestState) ar.AsyncState;
// 		// Get the WebRequest from RequestState.
// 		WebRequest req = rs.Request;
		
// 		try {
			
// 			// Call EndGetResponse, which produces the WebResponse object
// 			//  that came from the request issued above.
// 			WebResponse resp = req.EndGetResponse(ar);
			
// 			//  Start reading data from the response stream.
// 			Stream ResponseStream = resp.GetResponseStream();
			
// 			// Store the response stream in RequestState to read 
// 			// the stream asynchronously.
// 			rs.ResponseStream = ResponseStream;
			
// 			//  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
// 			IAsyncResult iarRead = ResponseStream.BeginRead(rs.BufferRead, 0, 
// 			 BUFFER_SIZE, new AsyncCallback(ReadCallBack), rs); 
// 		} catch (WebException e) {
			
// 			DebugExtras.LogError(string.Format("error handling response: {0} -- {1}", e.Status, e.Message)); 
// 			// note: status != error code; handling may have to compare against the enum 'WebExceptionStatus'
// 			// e.g. a 404 seems to be WebExceptionStatus.ProtocolError
			
// 		}
//    }


//    private static void ReadCallBack(IAsyncResult asyncResult) {
// 		DebugExtras.Log(">>>> ClientGetAsync.ReadCallBack");
//       // Get the RequestState object from AsyncResult.
//       RequestState rs = (RequestState)asyncResult.AsyncState;
		
//       // Retrieve the ResponseStream that was set in RespCallback. 
//       Stream responseStream = rs.ResponseStream;

//       // Read rs.BufferRead to verify that it contains data. 
//       int read = responseStream.EndRead( asyncResult );
//       if (read > 0) {
// 			DebugExtras.Log("there's more stream to get, I guess");
//          // Prepare a Char array buffer for converting to Unicode.
//          Char[] charBuffer = new Char[BUFFER_SIZE];
         
//          // Convert byte stream to Char array and then to String.
//          // len contains the number of characters converted to Unicode.
//       	 int len = rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

//          String str = new String(charBuffer, 0, len);

//          // Append the recently read data to the RequestData stringbuilder
//          // object contained in RequestState.
//          rs.RequestData.Append(
//             Encoding.ASCII.GetString(rs.BufferRead, 0, read));         

//          // Continue reading data until 
//          // responseStream.EndRead returns –1.
//          IAsyncResult ar = responseStream.BeginRead( 
//             rs.BufferRead, 0, BUFFER_SIZE, 
//             new AsyncCallback(ReadCallBack), rs);
//       } else {
//          if (rs.RequestData.Length>0) {
//             //  Display data to the console.
//             string strContent;                  
//             strContent = rs.RequestData.ToString();
			
//          }
//          // Close down the response stream.
//          responseStream.Close();         
//          // Set the ManualResetEvent so the main thread can exit. <-- (not quite right, as we aren't blocking this way)
// //         allDone.Set();                           
//       }
//       return;
//    }    
// }