//  Copyright 2015 Stefan Negritoiu (FreeBusy). See LICENSE file for more information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Json;
using AlexaSkillsKit.Messages.Validation;

namespace AlexaSkillsKit.Speechlet
{
    public abstract class Speechlet : ISpeechlet
    {

        /// <summary>
        /// Processes Alexa request AND validates request signature
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public virtual HttpResponseMessage GetResponse(HttpRequestMessage httpRequest)
        {

            var request = new ValidationRequest { HttpRequest = httpRequest, RequestTime = DateTime.UtcNow };
            var response = OnRequestValidation(request);

            if (!response.Success) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) {
                    ReasonPhrase = response.ValidationResult.ToString()
                };
            }

            string alexaResponse = DoProcessRequest(response.AlexaRequest);

            HttpResponseMessage httpResponse;
            if (alexaResponse == null) {
                httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
            else {
                httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
                httpResponse.Content = new StringContent(alexaResponse, Encoding.UTF8, "application/json");
                Debug.WriteLine(httpResponse.ToLogString());
            }

            return httpResponse;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestContent"></param>
        /// <returns></returns>
        public virtual string ProcessRequest(string requestContent) {
            var requestEnvelope = SpeechletRequestEnvelope.FromJson(requestContent);
            return DoProcessRequest(requestEnvelope);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestJson"></param>
        /// <returns></returns>
        public virtual string ProcessRequest(JObject requestJson) {
            var requestEnvelope = SpeechletRequestEnvelope.FromJson(requestJson);
            return DoProcessRequest(requestEnvelope);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestEnvelope"></param>
        /// <returns></returns>
        private string DoProcessRequest(SpeechletRequestEnvelope requestEnvelope) {
            Session session = requestEnvelope.Session;
            SpeechletResponse response = null;

            // process launch request
            if (requestEnvelope.Request is LaunchRequest) {
                var request = requestEnvelope.Request as LaunchRequest;
                if (requestEnvelope.Session.IsNew) {
                    OnSessionStarted(
                        new SessionStartedRequest(request.RequestId, request.Timestamp), session);
                }
                response = OnLaunch(request, session);
            }

            // process intent request
            else if (requestEnvelope.Request is IntentRequest) {
                var request = requestEnvelope.Request as IntentRequest;

                // Do session management prior to calling OnSessionStarted and OnIntentAsync 
                // to allow dev to change session values if behavior is not desired
                DoSessionManagement(request, session);

                if (requestEnvelope.Session.IsNew) {
                    OnSessionStarted(
                        new SessionStartedRequest(request.RequestId, request.Timestamp), session);
                }
                response = OnIntent(request, session);
            }

            // process session ended request
            else if (requestEnvelope.Request is SessionEndedRequest) {
                var request = requestEnvelope.Request as SessionEndedRequest;
                OnSessionEnded(request, session);
            }

            var responseEnvelope = new SpeechletResponseEnvelope {
                Version = requestEnvelope.Version,
                Response = response,
                SessionAttributes = requestEnvelope.Session.Attributes
            };
            return responseEnvelope.ToJson();
        }


        /// <summary>
        /// 
        /// </summary>
        private void DoSessionManagement(IntentRequest request, Session session) {
            if (session.IsNew) {
                session.Attributes[Session.INTENT_SEQUENCE] = request.Intent.Name;
            }
            else {
                // if the session was started as a result of a launch request 
                // a first intent isn't yet set, so set it to the current intent
                if (!session.Attributes.ContainsKey(Session.INTENT_SEQUENCE)) {
                    session.Attributes[Session.INTENT_SEQUENCE] = request.Intent.Name;
                }
                else {
                    session.Attributes[Session.INTENT_SEQUENCE] += Session.SEPARATOR + request.Intent.Name;
                }
            }

            // Auto-session management: copy all slot values from current intent into session
            foreach (var slot in request.Intent.Slots.Values) {
                session.Attributes[slot.Name] = slot.Value;
            }
        }

        /// <summary>
        /// Takes HttpRequest.Context.Bytes and converts to an Alexa Request.
        /// The response is passed in as a reference object since updating multiple values.
        /// </summary>
        public virtual void GetRequest(byte[] alexaBytes, ref ValidationResponse response)
        {
            try
            {
                var alexaContent = UTF8Encoding.UTF8.GetString(alexaBytes);
                response.AlexaRequest = SpeechletRequestEnvelope.FromJson(alexaContent);
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.InvalidJson;
            }
            catch (InvalidCastException)
            {
                response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.InvalidJson;
            }
            catch (Exception ex)
            {
                //TODO: Log Error
                response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.Error;
            }
        }
        

        /// <summary>
        /// Opportunity to set policy for handling requests with invalid signatures and/or timestamps
        /// </summary>
        /// <returns>Validation Response Object. Success will be true if tests pass</returns>
        public virtual ValidationResponse OnRequestValidation(ValidationRequest request)
        {
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK
            };

            string chainUrl = null;
            if (!request.HttpRequest.Headers.Contains(Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER) ||
                String.IsNullOrEmpty(chainUrl = request.HttpRequest.Headers.GetValues(Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER).First()))
            {
                response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.NoCertHeader;
            }

            string signature = null;
            if (!request.HttpRequest.Headers.Contains(Sdk.SIGNATURE_REQUEST_HEADER) ||
                String.IsNullOrEmpty(signature = request.HttpRequest.Headers.GetValues(Sdk.SIGNATURE_REQUEST_HEADER).First()))
            {
                response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.NoSignatureHeader;
            }

            var alexaBytes = AsyncHelpers.RunSync<byte[]>(() => request.HttpRequest.Content.ReadAsByteArrayAsync());
            Debug.WriteLine(request.HttpRequest.ToLogString());

            // attempt to verify signature only if we were able to locate certificate and signature headers
            if (response.ValidationResult == SpeechletRequestValidationResult.OK)
            {
                if (!SpeechletRequestSignatureVerifier.VerifyRequestSignature(alexaBytes, signature, chainUrl))
                {
                    response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.InvalidSignature;
                }
            }

            GetRequest(alexaBytes, ref response);

            // attempt to verify timestamp only if we were able to parse request body
            if (response.AlexaRequest != null)
            {
                if (!SpeechletRequestTimestampVerifier.VerifyRequestTimestamp(response.AlexaRequest, request.RequestTime))
                {
                    response.ValidationResult = response.ValidationResult | SpeechletRequestValidationResult.InvalidTimestamp;
                }
            }

            response.Success = (response.ValidationResult == SpeechletRequestValidationResult.OK);

            return response;
        }


        public abstract SpeechletResponse OnIntent(IntentRequest intentRequest, Session session);
        public abstract SpeechletResponse OnLaunch(LaunchRequest launchRequest, Session session);
        public abstract void OnSessionStarted(SessionStartedRequest sessionStartedRequest, Session session);
        public abstract void OnSessionEnded(SessionEndedRequest sessionEndedRequest, Session session);
    }
}
