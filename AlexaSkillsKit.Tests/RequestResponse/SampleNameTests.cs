using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using AlexaSkillsKit.Messages.Validation;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.Tests.Moq;
using AlexaSkillsKit.UI;
using Xunit;

namespace AlexaSkillsKit.Tests.RequestResponse
{
    public class SampleNameTests : AlexaSkillsKitTestBase
    {
        #region Success Messages
        private string MyNameIntentMessage => string.Format("{{" +
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
                                                              "			\"name\": \"{5}\"," +
                                                              "      	\"slots\": {{" +
                                                              "      	      \"Name\": {{" +
                                                              "					\"name\": \"name\"," +
                                                              "					\"value\": \"John\"" +
                                                              "				}}" +
                                                              "      	}}" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId, 
                                                                DateTime.UtcNow, "MyNameIsIntent");

        private string WhatsMyNameIntentMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": false," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}," +
                                                              "     \"attributes\": {{" +
                                                              " 		\"intentSequence\": \"MyNameIsIntent\"," +
                                                              "	    	\"name\": \"John\"" +
                                                              "     }}" +
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
                                                                DateTime.UtcNow, "WhatsMyNameIntent");

        #endregion

        #region Failure Messages
        private string UnknownIntentMessage => string.Format("{{" +
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
                                                              "			\"name\": \"{5}\"," +
                                                              "      	\"slots\": {{" +
                                                              "      	      \"Name\": {{" +
                                                              "					\"name\": \"name\"," +
                                                              "					\"value\": \"John\"" +
                                                              "				}}" +
                                                              "      	}}" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                                DateTime.UtcNow, "Unknown");

        private string MyNameIntentNoSlotMessage => string.Format("{{" +
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
                                                                DateTime.UtcNow, "MyNameIsIntent");

        private string MyNameIntentMissingNameSlotMessage => string.Format("{{" +
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
                                                              "			\"name\": \"{5}\"," +
                                                              "      	\"slots\": {{" +
                                                              "      	      \"RandomSlot\": {{" +
                                                              "					\"name\": \"name\"," +
                                                              "					\"value\": \"John\"" +
                                                              "				}}" +
                                                              "      	}}" +
                                                              "		}}" +
                                                              "	}}" +
                                                              "}}", SessionId, ApplicationId, UserId, RequestId,
                                                                DateTime.UtcNow, "MyNameIsIntent");

        private string WhatsMyNameIntentWithoutAnySessionAttributesMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": false," +
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
                                                                DateTime.UtcNow, "WhatsMyNameIntent");

        private string WhatsMyNameIntentWithoutNameMessage => string.Format("{{" +
                                                              "	\"version\": \"1.0\"," +
                                                              "	\"session\": {{" +
                                                              "		\"new\": false," +
                                                              "		\"sessionId\": \"{0}\"," +
                                                              "		\"application\": {{" +
                                                              "			\"applicationId\": \"{1}\"" +
                                                              "		}}," +
                                                              "		\"user\": {{" +
                                                              "			\"userId\": \"{2}\"" +
                                                              "		}}," +
                                                              "     \"attributes\": {{" +
                                                              " 		\"intentSequence\": \"MyNameIsIntent\"" +
                                                              "     }}" +
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
                                                                DateTime.UtcNow, "WhatsMyNameIntent");

        #endregion

        #region Intent Tests

        [Fact]
        public void MyNameIsIntentIsFound()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
                {
                    Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                    Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                    Content = new StringContent(MyNameIntentMessage)
                };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.GetResponse(request);

            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public void InvalidIntentThrowsException()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(UnknownIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();

            Exception ex = Assert.Throws<SpeechletException>(() => speechlet.GetResponse(request));

            Assert.Equal("Invalid Intent", ex.Message);
        }

        [Fact]
        public void MyNameIsIntentContainsNameSlot()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            var intentRequest = response.AlexaRequest.Request as IntentRequest;

            Assert.True(intentRequest?.Intent.Slots.ContainsKey("Name"));
        }

        [Fact]
        public void MyNameIsIntentNameSlotMatches()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            var intentRequest = response.AlexaRequest.Request as IntentRequest;

            if (intentRequest == null)
                throw new NullReferenceException("intentRequest is null");

            var slots = intentRequest.Intent.Slots;

            if (slots == null)
                throw new NullReferenceException("slots is null");
            
            Assert.Equal("John", slots["Name"]?.Value);
        }

        [Fact]
        public void MyNameIsIntentNoSlotRequestsRetry()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentNoSlotMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });


            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("I'm sorry, I didn't hear your name. You can tell me your name by saying, my name is Sam", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        [Fact]
        public void MyNameIsIntentSlotsExistButNotNameRequestsRetry()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMissingNameSlotMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });


            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("I'm not sure what your name is, please try again", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        [Fact]
        public void MyNameIsIntentRespondsWithName()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });


            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("Hello John, now I can remember your name, you can ask me your name by saying, whats my name?", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        #endregion

        #region Session Tests

        [Fact]
        public void MyNameIsIntentIsNewSession()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            Assert.True(response.AlexaRequest.Session.IsNew);
        }

        [Fact]
        public void MyNameIsIntentLeavesSessionOpen()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(MyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse =  speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.False(speechletResponse.ShouldEndSession);
        }

        [Fact]
        public void WhatsMyNameIntentIsExistingSession()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            Assert.False(response.AlexaRequest.Session.IsNew);
        }

        [Fact]
        public void WhatsMyNameIntentSessionContainsIntentSequence()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });
            
            Assert.True(response.AlexaRequest.Session.Attributes.ContainsKey(Session.INTENT_SEQUENCE));
        }

        [Fact]
        public void WhatsMyNameIntentSessionIntentSequenceMatches()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            var intentSequence = response.AlexaRequest.Session.Attributes[Session.INTENT_SEQUENCE];

            Assert.Equal("MyNameIsIntent", intentSequence);
        }

        [Fact]
        public void WhatsMyNameIntentClosesSessionWithProperSessionValue()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.True(speechletResponse.ShouldEndSession);
        }

        [Fact]
        public void WhatsMyNameIntentLeavesSessionOpenWithoutProperSessionValue()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentWithoutNameMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.False(speechletResponse.ShouldEndSession);
        }

        [Fact]
        public void WhatsMyNameIntentNoSessionAttributesFound()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentWithoutAnySessionAttributesMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("I'm sorry, you seem to be new here. You can tell me your name by saying, my name is Sam", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        [Fact]
        public void WhatsMyNameIntentNoNameSessionAttributeFound()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentWithoutNameMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });

            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("I'm not sure what your name is, you can say, my name is Sam", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        [Fact]
        public void WhatsMyNameIntentRespondsWithName()
        {
            SessionId = new Guid().ToString().ToLower();
            RequestId = new Guid().ToString().ToLower();

            var request = new HttpRequestMessage
            {
                Properties = { { HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration() } },
                Headers =
                        {
                            { Sdk.SIGNATURE_CERT_URL_REQUEST_HEADER, "TestSignatureCertChainUrlValue" },
                            { Sdk.SIGNATURE_REQUEST_HEADER, "TestSignatureValue" }
                        },
                Content = new StringContent(WhatsMyNameIntentMessage)
            };

            var speechlet = new SampleNameSpeechlet();
            var response = speechlet.OnRequestValidation(new ValidationRequest { HttpRequest = request, RequestTime = DateTime.UtcNow });


            speechlet.DoSessionManagement((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);
            var speechletResponse = speechlet.OnIntent((IntentRequest)response.AlexaRequest.Request, response.AlexaRequest.Session);

            Assert.Equal("Your name is John, goodbye", ((PlainTextOutputSpeech)speechletResponse.OutputSpeech).Text);
        }

        #endregion
    }
}
