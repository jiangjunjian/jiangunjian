using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Extensions;
using RestSharp.Validation;

namespace RestSharp_Test
{
    /// <summary>
    /// The rest api. 
    /// rest api 通用类
    /// </summary>
    public class RestApi
    {
        /// <summary>
        /// The base url.
        /// </summary>
        private const string BaseUrl = "https://api.twilio.com/2008-08-01";

        /// <summary>
        /// The message.
        /// </summary>
        private const string Message = "Error retrieving response.  Check inner details for more info.";


        /// <summary>
        /// The _account sid.
        /// </summary>
        private readonly string accountSid;

        /// <summary>
        /// The _secret key.
        /// </summary>
        private readonly string secretKey;

        public RestApi(string accountSid, string secretKey)
        {
            this.accountSid = accountSid;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// </exception>
        public T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(accountSid, secretKey);
            request.AddParameter("AccountSid", accountSid, ParameterType.UrlSegment); // used on every request
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {

                var twilioException = new ApplicationException(Message, response.ErrorException);
                throw twilioException;
            }

            return response.Data;
        }

        // TwilioApi.cs, method of TwilioApi class
        public Call GetCall(string callSid)
        {
            var request = new RestRequest();
            request.Resource = "Accounts/{AccountSid}/Calls/{CallSid}";
            request.RootElement = "Call";

            request.AddParameter("CallSid", callSid, ParameterType.UrlSegment);

            return this.Execute<Call>(request);
        }

        // TwilioApi.cs, method of TwilioApi class
        public Call InitiateOutboundCall(CallOptions options)
        {
            ////Require.Argument("Caller", options.Caller);
            ////Require.Argument("Called", options.Called);
            Require.Argument("Url", options.Url);

            var request = new RestRequest(Method.POST);
            request.Resource = "Accounts/{AccountSid}/Calls";
            request.RootElement = "Calls";

            ////request.AddParameter("Caller", options.Caller);
            ////request.AddParameter("Called", options.Called);
            request.AddParameter("Url", options.Url);

            if (!string.IsNullOrEmpty(options.Method))
            {
                request.AddParameter("Method", options.Method);
            }

            if (!string.IsNullOrEmpty(options.SendDigits))
            {
                request.AddParameter("SendDigits", options.SendDigits);
            }

            if (!string.IsNullOrEmpty(options.IfMachine))
            {
                request.AddParameter("IfMachine", options.IfMachine);
            }

            if (options.Timeout.HasValue)
            {
                request.AddParameter("Timeout", options.Timeout.Value);
            }

            return this.Execute<Call>(request);
        }
    }

    /// <summary>
    /// The call.
    /// </summary>
    public class Call
    {
        public string Sid { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CallSegmentSid { get; set; }
        public string AccountSid { get; set; }
        public string Called { get; set; }
        public string Caller { get; set; }
        public string PhoneNumberSid { get; set; }
        public int Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int Flags { get; set; }
    }

    /// <summary>
    /// Available options to include when initiating a phone call
    /// </summary>
    public class CallOptions
    {
        /// <summary>
        /// The phone number to use as the caller id. Format with a '+' and country code e.g., +16175551212 (E.164 format). Must be a Twilio number or a valid outgoing caller id for your account.
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// The number to call formatted with a '+' and country code e.g., +16175551212 (E.164 format). Twilio will also accept unformatted US numbers e.g., (415) 555-1212, 415-555-1212.
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// The fully qualified URL that should be consulted when the call connects. Just like when you set a URL for your inbound calls.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// The 34 character sid of the application Twilio should use to handle this phone call. If this parameter is present, Twilio will ignore all of the voice URLs passed and use the URLs set on the application.
        /// </summary>
        public string ApplicationSid { get; set; }
        /// <summary>
        /// A URL that Twilio will request when the call ends to notify your app.
        /// </summary>
        public string StatusCallback { get; set; }
        /// <summary>
        /// The HTTP method Twilio should use when requesting the above URL. Defaults to POST.
        /// </summary>
        public string StatusCallbackMethod { get; set; }
        /// <summary>
        /// The call lifecycle events Twilio should send a StatusCallback request for.
        /// Available event types:
        /// - initiated
        /// - ringing
        /// - answered
        /// - completed
        ///
        /// "completed" events are free; see twilio.com for pricing on the other event types.
        /// If not set, defaults to ["completed"].
        /// </summary>
        public string[] StatusCallbackEvents { get; set; }
        /// <summary>
        /// The HTTP method Twilio should use when requesting the required Url parameter's value above. Defaults to POST.
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// A string of keys to dial after connecting to the number. Valid digits in the string include: any digit (0-9), '#' and '*'. For example, if you connected to a company phone number, and wanted to dial extension 1234 and then the pound key, use SendDigits=1234#. Remember to URL-encode this string, since the '#' character has special meaning in a URL.
        /// </summary>
        public string SendDigits { get; set; }
        /// <summary>
        /// Tell Twilio to try and determine if a machine (like voicemail) or a human has answered the call. Possible values are Continue and Hangup.
        /// </summary>
        public string IfMachine { get; set; }
        /// <summary>
        /// The integer number of seconds that Twilio should allow the phone to ring before assuming there is no answer. Default is 60 seconds, the maximum is 999 seconds. Note, you could set this to a low value, such as 15, to hangup before reaching an answering machine or voicemail.
        /// </summary>
        public int? Timeout { get; set; }
        /// <summary>
        /// A URL that Twilio will request if an error occurs requesting or executing the TwiML at Url.
        /// </summary>
        public string FallbackUrl { get; set; }
        /// <summary>
        /// The HTTP method that Twilio should use to request the FallbackUrl. Must be either GET or POST. Defaults to POST.
        /// </summary>
        public string FallbackMethod { get; set; }
        /// <summary>
        /// Set this parameter to 'true' to record the entirety of a phone call. The RecordingUrl will be sent to the StatusCallback URL. Defaults to 'false'.
        /// </summary>
        public bool Record { get; set; }
        /// <summary>
        /// If this is a Sip call, set the authorization username for your Sip
        /// endpoint
        /// </summary>
        public string SipAuthUsername { get; set; }
        /// <summary>
        /// If this is a Sip call, set the authorization password for your Sip
        /// endpoint
        /// </summary>
        public string SipAuthPassword { get; set; }
        /// <summary>
        /// Set this parameter to specify the number of channels in the final .wav recording. Defaults to 'mono'.
        /// </summary>
        public string RecordingChannels { get; set; }
        /// <summary>
        /// A URL that Twilio will request when the recording is ready to notify your app.
        /// </summary>
        public string RecordingStatusCallback { get; set; }
        /// <summary>
        /// The HTTP method Twilio should use when requesting the above URL. Defaults to POST.
        /// </summary>
        public string RecordingStatusCallbackMethod { get; set; }
    }
}
