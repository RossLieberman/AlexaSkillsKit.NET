using System.Linq;
using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Messages.Validation;
using AlexaSkillsKit.Slu;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;

namespace AlexaSkillsKit.Tests.Moq
{
    public class SampleNameSpeechlet : Speechlet.Speechlet
    {
        // Note: NAME_KEY being a JSON property key gets camelCased during serialization
        private const string NameKey = "name";
        private const string NameSlot = "Name";

        public override SpeechletResponse OnIntent(IntentRequest request, Session session)
        {
            // Get intent from the request object.
            var intent = request.Intent;
            var intentName = intent?.Name;

            // Note: If the session is started with an intent, no welcome message will be rendered;
            // rather, the intent specific response will be returned.
            if ("MyNameIsIntent".Equals(intentName))
                return SetNameInSessionAndSayHello(intent, session);

            if ("WhatsMyNameIntent".Equals(intentName))
                return GetNameFromSessionAndSayHello(intent, session);
            
            throw new SpeechletException("Invalid Intent");
        }

        public override SpeechletResponse OnLaunch(LaunchRequest request, Session session)
        {
            return GetWelcomeResponse();
        }

        public override void OnSessionStarted(SessionStartedRequest request, Session session) { }

        public override void OnSessionEnded(SessionEndedRequest request, Session session) { }



        /**
         * Creates and returns a {@code SpeechletResponse} with a welcome message.
         * 
         * @return SpeechletResponse spoken and visual welcome message
         */
        private SpeechletResponse GetWelcomeResponse()
        {
            // Create the welcome message.
            var speechOutput = "Welcome to the Alexa AppKit session sample app, please tell me your name by saying, my name is Sam";

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse("Welcome", speechOutput, false);
        }

        /**
         * Creates a {@code SpeechletResponse} for the intent and stores the extracted name in the
         * Session.
         * 
         * @param intent
         *            intent for the request
         * @return SpeechletResponse spoken and visual response the given intent
         */
        private SpeechletResponse SetNameInSessionAndSayHello(Intent intent, Session session)
        {
            // Get the slots from the intent.
            var slots = intent.Slots;
            string speechOutput;

            if (!slots.Any())
            {
                speechOutput = "I'm sorry, I didn't hear your name. You can tell me your name by saying, my name is Sam";
                return BuildSpeechletResponse(intent.Name, speechOutput, false);
            }

            // Get the name slot from the list slots.
            var nameSlot = slots.Keys.Contains(NameSlot) ? slots[NameSlot] : null;

            // Check for name and create output to user.
            if (nameSlot != null)
            {
                // Store the user's name in the Session and create response.
                var name = nameSlot.Value;
                session.Attributes[NameKey] = name;
                speechOutput = $"Hello {name}, now I can remember your name, you can ask me your name by saying, whats my name?";
            }
            else
            {
                // Render an error since we don't know what the users name is.
                speechOutput = "I'm not sure what your name is, please try again";
            }

            // Here we are setting shouldEndSession to false to not end the session and
            // prompt the user for input
            return BuildSpeechletResponse(intent.Name, speechOutput, false);
        }


        /**
         * Creates a {@code SpeechletResponse} for the intent and get the user's name from the Session.
         * 
         * @param intent
         *            intent for the request
         * @return SpeechletResponse spoken and visual response for the intent
         */
        private SpeechletResponse GetNameFromSessionAndSayHello(Intent intent, Session session)
        {
            string speechOutput;
            var shouldEndSession = false;

            if (!session.Attributes[Session.INTENT_SEQUENCE].Contains("MyNameIsIntent"))
            {
                speechOutput = "I'm sorry, you seem to be new here. You can tell me your name by saying, my name is Sam";
                return BuildSpeechletResponse(intent.Name, speechOutput, false);
            }

            // Get the user's name from the session.
            var name = session.Attributes.ContainsKey(NameKey) ? session.Attributes[NameKey] : null;

            // Check to make sure user's name is set in the session.
            if (!string.IsNullOrEmpty(name))
            {
                speechOutput = $"Your name is {name}, goodbye";
                shouldEndSession = true;
            }
            else
            {
                // Since the user's name is not set render an error message.
                speechOutput = "I'm not sure what your name is, you can say, my name is Sam";
            }

            return BuildSpeechletResponse(intent.Name, speechOutput, shouldEndSession);
        }

        /// <summary>
        /// For Testing Purposes Override Request Validation to just convert Request to Alexa Response Object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override ValidationResponse OnRequestValidation(ValidationRequest request)
        {
            var response = new ValidationResponse
            {
                ValidationResult = SpeechletRequestValidationResult.OK,
                Success = true
            };

            var alexaBytes = AsyncHelpers.RunSync(() => request.HttpRequest.Content.ReadAsByteArrayAsync());

            GetRequest(alexaBytes, ref response);

            return response;
        }

        /**
         * Creates and returns the visual and spoken response with shouldEndSession flag
         * 
         * @param title
         *            title for the companion application home card
         * @param output
         *            output content for speech and companion application home card
         * @param shouldEndSession
         *            should the session be closed
         * @return SpeechletResponse spoken and visual response for the given input
         */
        private SpeechletResponse BuildSpeechletResponse(string title, string output, bool shouldEndSession)
        {
            // Create the Simple card content.
            var card = new SimpleCard
                {
                    Title = $"SessionSpeechlet - {title}",
                    Content = $"SessionSpeechlet - {output}"
                };

            // Create the plain text output.
            var speech = new PlainTextOutputSpeech
                {
                    Text = output
                };

            // Create the speechlet response.
            var response = new SpeechletResponse
                {
                    ShouldEndSession = shouldEndSession,
                    OutputSpeech = speech,
                    Card = card
                };
            return response;
        }
    }
}
