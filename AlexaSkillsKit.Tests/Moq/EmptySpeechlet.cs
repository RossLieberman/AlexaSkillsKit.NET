using AlexaSkillsKit.Speechlet;

namespace AlexaSkillsKit.Tests.Moq
{
    public class EmptySpeechlet : Speechlet.Speechlet
    {
        public override SpeechletResponse OnIntent(IntentRequest request, Session session)
        {
            var response = new SpeechletResponse();

            return response;
        }

        public override SpeechletResponse OnLaunch(LaunchRequest request, Session session)
        {
            var response = new SpeechletResponse();

            return response;
        }

        public override void OnSessionStarted(SessionStartedRequest request, Session session) { }

        public override void OnSessionEnded(SessionEndedRequest request, Session session) {  }
    }
}
