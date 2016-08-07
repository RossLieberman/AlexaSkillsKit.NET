using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Hosting;
using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Json;
using AlexaSkillsKit.Messages.Validation;
using AlexaSkillsKit.Tests.Moq;
using Xunit;

namespace AlexaSkillsKit.Tests.Authentication
{
    public class SpeechletValidationTests : AlexaSkillsKitTestBase
    {
        #region Success Messages
        private string GenericSuccessMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": true," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}" +
                                                              "	}}," +
                                                              "	\"request\": {{" +
                                                              "		\"type\": \"IntentRequest\"," +
                                                              "		\"requestId\": \"{3}\"," +
                                                              "		\"timestamp\": \"{4}\"," +
                                                              "		\"intent\": {{" +
                                                              "			\"name\": \"{5}\"" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                            DateTime.UtcNow, "MyIntent");

        #endregion

        #region Failure Messages
        private string InvalidJsonMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\" " +
                                                              "		\"new\": true," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}" +
                                                              "	}}," +
                                                              "	\"request\": {{" +
                                                              "		\"type\": \"IntentRequest\"," +
                                                              "		\"requestId\": \"{3}\"," +
                                                              "		\"timestamp\": \"{4}\"," +
                                                              "		\"intent\": {{" +
                                                              "			\"name\": \"{5}\"" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                            DateTime.UtcNow, "MyIntent");

        private string InvalidVersionMessage => string.Format("{{" +
                                                              "	\"version\": \"0.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": true," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}" +
                                                              "	}}," +
                                                              "	\"request\": {{" +
                                                              "		\"type\": \"IntentRequest\"," +
                                                              "		\"requestId\": \"{3}\"," +
                                                              "		\"timestamp\": \"{4}\"," +
                                                              "		\"intent\": {{" +
                                                              "			\"name\": \"{5}\"" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                            DateTime.UtcNow, "MyIntent");

        private string UnkonwnRequestTypeMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": true," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}" +
                                                              "	}}," +
                                                              "	\"request\": {{" +
                                                              "		\"type\": \"UnknownRequest\"," +
                                                              "		\"requestId\": \"{3}\"," +
                                                              "		\"timestamp\": \"{4}\"," +
                                                              "		\"intent\": {{" +
                                                              "			\"name\": \"{5}\"" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                            DateTime.UtcNow, "MyIntent");

        private string OutdatedMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": true," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}" +
                                                              "	}}," +
                                                              "	\"request\": {{" +
                                                              "		\"type\": \"IntentRequest\"," +
                                                              "		\"requestId\": \"{3}\"," +
                                                              "		\"timestamp\": \"{4}\"," +
                                                              "		\"intent\": {{" +
                                                              "			\"name\": \"{5}\"" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                            DateTime.UtcNow.AddSeconds(-1 * (Sdk.TIMESTAMP_TOLERANCE_SEC + 1)), "MyIntent");

        #endregion

        #region Certificate Signature Header Tests

        [Fact]
        public void HeaderContainsValidSignatureCert()
        {
            var request = new ValidationRequest
                {
                    HttpRequest = new HttpRequestMessage
                    {
                        Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                        Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                        Content = new StringContent(GenericSuccessMessage)
                    }
                };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.False((response.ValidationResult & SpeechletRequestValidationResult.NoCertHeader) == 
                            SpeechletRequestValidationResult.NoCertHeader);
        }

        [Fact]
        public void HeaderDoesNotContainsValidSignatureCert()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.NoCertHeader) ==
                            SpeechletRequestValidationResult.NoCertHeader);
        }

        [Fact]
        public void HeaderContainsBlankSignatureCertFails()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.NoCertHeader) ==
                            SpeechletRequestValidationResult.NoCertHeader);
        }

        #endregion

        #region Signature Request Header Tests

        [Fact]
        public void HeaderContainsValidSignature()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.False((response.ValidationResult & SpeechletRequestValidationResult.NoSignatureHeader) ==
                            SpeechletRequestValidationResult.NoSignatureHeader);
        }

        [Fact]
        public void HeaderDoesNotContainsValidSignature()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.NoSignatureHeader) ==
                            SpeechletRequestValidationResult.NoSignatureHeader);
        }

        [Fact]
        public void HeaderContainsBlankSignatureFails()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.NoSignatureHeader) ==
                            SpeechletRequestValidationResult.NoSignatureHeader);
        }

        #endregion

        #region Timestamp Tests

        [Fact]
        public void TimestampIsWithinTolerance()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                },
                RequestTime = DateTime.UtcNow
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());
            var alexaRequest = SpeechletRequestEnvelope.FromJson(Encoding.UTF8.GetString(alexaBytes));

            var validTimestamp = SpeechletRequestTimestampVerifier.VerifyRequestTimestamp(alexaRequest, request.RequestTime);

            Assert.True(validTimestamp);
        }

        [Fact]
        public void TimestampIsNotWithinTolerance()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(OutdatedMessage)
                },
                RequestTime = DateTime.UtcNow
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());
            var alexaRequest = SpeechletRequestEnvelope.FromJson(Encoding.UTF8.GetString(alexaBytes));

            var validTimestamp = SpeechletRequestTimestampVerifier.VerifyRequestTimestamp(alexaRequest, request.RequestTime);

            Assert.False(validTimestamp);
        }

        [Fact]
        public void TimestampRequestIsWithinTolerance()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                },
                RequestTime = DateTime.UtcNow
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.False((response.ValidationResult & SpeechletRequestValidationResult.InvalidTimestamp) ==
                            SpeechletRequestValidationResult.InvalidTimestamp);
        }

        [Fact]
        public void TimestampRequestIsNotWithinTolerance()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(OutdatedMessage)
                },
                RequestTime = DateTime.UtcNow
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.InvalidTimestamp) ==
                            SpeechletRequestValidationResult.InvalidTimestamp);
        }

        #endregion

        #region Get Requests Tests 

        [Fact]
        public void RequestIsValidAlexaMessage()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                }
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            var speechlet = new EmptySpeechlet();
            var response = new ValidationResponse
                    {
                        ValidationResult = SpeechletRequestValidationResult.OK
                    };

            speechlet.GetRequest(alexaBytes, ref response);

            Assert.True(response.ValidationResult == SpeechletRequestValidationResult.OK);
        }

        [Fact]
        public void RequestIsInvalidJsonAlexaMessage()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(InvalidJsonMessage)
                }
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            var speechlet = new EmptySpeechlet();
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK
            };

            speechlet.GetRequest(alexaBytes, ref response);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.InvalidJson) ==
                            SpeechletRequestValidationResult.InvalidJson);
        }

        [Fact]
        public void RequestIsInvalidVersionAlexaMessage()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(InvalidVersionMessage)
                }
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            var speechlet = new EmptySpeechlet();
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK
            };

            speechlet.GetRequest(alexaBytes, ref response);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.InvalidSpeechlet) == SpeechletRequestValidationResult.InvalidSpeechlet);
        }

        [Fact]
        public void RequestIsUnknownRequestTypeAlexaMessage()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(UnkonwnRequestTypeMessage)
                }
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            var speechlet = new EmptySpeechlet();
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK
            };

            speechlet.GetRequest(alexaBytes, ref response);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.InvalidSpeechlet) == SpeechletRequestValidationResult.InvalidSpeechlet);
        }

        [Fact]
        public void RequestIsEmptyAlexaMessage()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(string.Empty)
                }
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            var speechlet = new EmptySpeechlet();
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK
            };

            speechlet.GetRequest(alexaBytes, ref response);

            Assert.True((response.ValidationResult & SpeechletRequestValidationResult.InvalidSpeechlet) == SpeechletRequestValidationResult.InvalidSpeechlet);
        }

        [Fact]
        public void RequestIsValid()
        {
            var request = new ValidationRequest
            {
                HttpRequest = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(GenericSuccessMessage)
                },
                RequestTime = DateTime.UtcNow
            };

            var speechlet = new EmptySpeechlet();
            var response = speechlet.OnRequestValidation(request);

            //Need to validate no errors except for InvalidSignature since SSL Cert not supplied <see cref="SignatureVerifierTests">
            Assert.True(
                    (response.ValidationResult & SpeechletRequestValidationResult.NoCertHeader) != SpeechletRequestValidationResult.NoCertHeader
                    &&
                    (response.ValidationResult & SpeechletRequestValidationResult.NoSignatureHeader) != SpeechletRequestValidationResult.NoSignatureHeader
                    &&
                    (response.ValidationResult & SpeechletRequestValidationResult.InvalidJson) != SpeechletRequestValidationResult.InvalidJson
                    &&
                    (response.ValidationResult & SpeechletRequestValidationResult.InvalidSpeechlet) != SpeechletRequestValidationResult.InvalidSpeechlet
                    &&
                    (response.ValidationResult & SpeechletRequestValidationResult.InvalidTimestamp) != SpeechletRequestValidationResult.InvalidTimestamp
                    &&
                    (response.ValidationResult & SpeechletRequestValidationResult.Error) != SpeechletRequestValidationResult.Error
                    );
        }

        #endregion
    }
}
