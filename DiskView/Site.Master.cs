using System;
using System.Diagnostics;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace DiskView
{
	public partial class Site : System.Web.UI.MasterPage
	{
		#region " Anti-XSRF Code "
		// Adapted from Visual Studio-generated code
		private const string AntiXsrfTokenKey = "__AntiXsrfToken";
		private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
		private string _antiXsrfTokenValue;

		protected void Page_Init(object sender, EventArgs e)
		{
			// The code below helps to protect against XSRF attacks
			var requestCookie = Request.Cookies[AntiXsrfTokenKey];
			if (requestCookie != null && Guid.TryParse(requestCookie.Value, out Guid requestCookieGuidValue))
			{
				// Use the Anti-XSRF token from the cookie
				_antiXsrfTokenValue = requestCookie.Value;
				Page.ViewStateUserKey = _antiXsrfTokenValue;
			}
			else
			{
				// Generate a new Anti-XSRF token and save to the cookie
				_antiXsrfTokenValue = Guid.NewGuid().ToString("N");
				Page.ViewStateUserKey = _antiXsrfTokenValue;

				var responseCookie = new HttpCookie(AntiXsrfTokenKey)
				{
					HttpOnly = true,
					Value = Server.HtmlEncode(_antiXsrfTokenValue), //Encoder.HtmlEncode(_antiXsrfTokenValue),
					Secure = FormsAuthentication.RequireSSL && Request.IsSecureConnection //|| AppSetting.Env == EnvironmentOption.PROD)
				};

				Response.Cookies.Set(responseCookie);
			}

			Page.PreLoad += Page_PreLoad;

			//CheckBrowserRequirements();

			/* Portcullis recommendation 4.16 - "Web application allows client browser to cache sensitive information"
				All of these control page caching - which should be used?

				Set Using Response									Set Using <META> Tag
				**************************************************************************************************************************
				Response.AddHeader("Cache-Control", "no-cache")		<meta http-equiv="Cache-Control" content="no-cache" />
				Response.AddHeader("Pragma", "no-cache")			<meta http-equiv="Pragma" content="no-cache" />
				Response.Expires = -1								<meta http-equiv="Expires" content="-1">
			*/
			//if (GetXmlSetting("NoCache") == "True")
			//{
			//	// HTTP Headers  https://en.wikipedia.org/wiki/List_of_HTTP_header_fields
			//	// AddHeader is provided for compatability with earlier versions of ASP
			//	//Response.AddHeader("Cache-Control", "no-cache")
			//	//Response.AddHeader("Pragma", "no-cache")
			//	Response.CacheControl = "no-cache";
			//	Response.Cache.SetCacheability(HttpCacheability.NoCache);
			//	//Response.BufferOutput = true;  // Buffer response so that page is sent after processing is complete
			//	Response.Expires = -1;
			//}
		}

		protected void Page_PreLoad(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				// Set Anti-XSRF token
				ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
				ViewState[AntiXsrfUserNameKey] = Request.ServerVariables["USERNAME"] ?? string.Empty;
			}
			else
			{
				// Validate the Anti-XSRF token
				if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
					|| (string)ViewState[AntiXsrfUserNameKey] != (Request.ServerVariables["USERNAME"] ?? string.Empty))
				{
					throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
				}
			}

			// Homebrew Anti-XSRF validation - ensure request is coming from the same host and port
			//string httpReferer = Request.ServerVariables["HTTP_REFERER"]?.ToString();
			//if (Uri.TryCreate(httpReferer, UriKind.Absolute, out Uri uriReferer))
			//{
			//	string referer = uriReferer.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
			//	string host = Request.Url.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);

			//	if (referer != host) //(!uriReferer.Equals(uriHost))
			//		throw new InvalidOperationException("Form submitted from a foreign host.");
			//}
		}
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
			string pageName = Page.GetType().Name.ToLower().Replace("_aspx", "");

			switch (pageName)
			{
				case "login":
					if (Session != null && Session.SessionID != SessionID.Value)
					{
						// For security reasons, update the Session ID before going to login page
						SessionID.Value = CreateNewSessionID();
					}
					break;
			}
		}

		private string CreateNewSessionID()
		{
			// Update the Session with a new Session ID once user has logged in; returns new Session ID if successful. 
			// Created in response to Portcullis Security Report Item 4.5 "Web Applications Vulnerable To Session Fixation Attacks".
			// See http://weblogs.asp.net/anasghanem/programmatically-changing-the-session-id
			// and http://stackoverflow.com/questions/11987579/how-to-generate-a-new-session-id

			var objSessionIDManager = new SessionIDManager();
			string oldSessionID = Context.Session.SessionID;
			string newSessionID = objSessionIDManager.CreateSessionID(Context);
			objSessionIDManager.SaveSessionID(Context, newSessionID, out bool redirected, out bool cookieAdded);

			Debug.WriteLine($"Old Session ID: {oldSessionID}, New Session ID: {newSessionID}, Redirected: {redirected}, Cookie Added: {cookieAdded}");

			return cookieAdded ? newSessionID : "";
		}
	}
}