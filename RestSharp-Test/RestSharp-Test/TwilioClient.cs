using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions;

namespace RestSharp_Test
{
    /// <summary>
    /// The twilio client.
    /// </summary>
    public abstract partial class TwilioClient
    {
        /// <summary>
        /// Gets or sets the account sid.
        /// </summary>
        private string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the auth token.
        /// </summary>
        private string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the account resource sid.
        /// </summary>
        private string AccountResourceSid { get; set; }

        /// <summary>
        /// Gets or sets the date format.
        /// </summary>
        private string DateFormat { get; set; }

        /// <summary>
        /// Gets or sets client.
        /// </summary>
        private RestClient client;

        /// <summary>
        /// Twilio API version to use when making requests
        /// </summary>
        public string ApiVersion { get; private set; }

        /// <summary>
        /// Base URL of API
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Proxy
        /// </summary>
        public IWebProxy Proxy
        {
            get { return this.client.Proxy; }
            set { this.client.Proxy = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioClient"/> class.
        /// </summary>
        /// <param name="accountSid">
        /// The account sid.
        /// </param>
        /// <param name="authToken">
        /// The auth token.
        /// </param>
        /// <param name="accountResourceSid">
        /// The account resource sid.
        /// </param>
        /// <param name="apiVersion">
        /// The api version.
        /// </param>
        /// <param name="baseUrl">
        /// The base url.
        /// </param>
        public TwilioClient(string accountSid, string authToken, string accountResourceSid, string apiVersion, string baseUrl)
        {
            this.ApiVersion = apiVersion;
            this.BaseUrl = baseUrl;
            this.AccountSid = accountSid;
            this.AuthToken = authToken;
            this.AccountResourceSid = accountResourceSid;
            this.DateFormat = "ddd, dd MMM yyyy HH:mm:ss '+0000'";

            ////silverlight friendly way to get current version
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(assembly.FullName);
            var version = assemblyName.Version;

            this.client = new RestClient();
            this.client.UserAgent = "twilio-csharp/" + version + " (.NET " + Environment.Version.ToString() + ")";
            this.client.Authenticator = new HttpBasicAuthenticator(this.AccountSid, this.AuthToken);
            this.client.AddDefaultHeader("Accept-charset", "utf-8");


            this.client.BaseUrl = new Uri(string.Format("{0}{1}", this.BaseUrl, this.ApiVersion));
            this.client.Timeout = 30500;

            //// if acting on a subaccount, use request.AddUrlSegment("AccountSid", "value")
            //// to override for that request.
            this.client.AddDefaultUrlSegment("AccountSid", this.AccountResourceSid);
        }


        /// <summary>
        /// Execute a manual REST request
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to create and populate with the returned data.
        /// </typeparam>
        /// <param name="request">
        /// The RestRequest to execute (will use client credentials)
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public virtual T Execute<T>(IRestRequest request) where T : new()
        {
            request.OnBeforeDeserialization = (resp) =>
            {
                // for individual resources when there's an error to make
                // sure that RestException props are populated
                if (((int)resp.StatusCode) >= 400)
                {
                    // have to read the bytes so .Content doesn't get populated
                    const string restException = "{{ \"RestException\" : {0} }}";
                    var content = resp.RawBytes.AsString(); ////get the response content
                    var newJson = string.Format(restException, content);

                    resp.Content = null;
                    resp.RawBytes = Encoding.UTF8.GetBytes(newJson.ToString());
                }
            };

            request.DateFormat = this.DateFormat;

            var response = this.client.Execute<T>(request);
            return response.Data;
        }

        /// <summary>
        /// Execute a manual REST request
        /// </summary>
        /// <param name="request">
        /// The RestRequest to execute (will use client credentials)
        /// </param>
        /// <returns>
        /// The <see cref="IRestResponse"/>.
        /// </returns>
        public virtual IRestResponse Execute(IRestRequest request)
        {
            return this.client.Execute(request);
        }
    }
}
