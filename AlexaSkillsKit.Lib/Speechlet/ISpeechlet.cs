//  Copyright 2015 Stefan Negritoiu (FreeBusy). See LICENSE file for more information.

using System;
using System.Net.Http;
using AlexaSkillsKit.Authentication;
using AlexaSkillsKit.Json;
using AlexaSkillsKit.Messages.Validation;

namespace AlexaSkillsKit.Speechlet
{
    public interface ISpeechlet
    {
        void GetRequest(byte[] alexaBytes, ref ValidationResponse response);
        ValidationResponse OnRequestValidation(ValidationRequest request);

        SpeechletResponse OnIntent(IntentRequest intentRequest, Session session);
        SpeechletResponse OnLaunch(LaunchRequest launchRequest, Session session);
        void OnSessionStarted(SessionStartedRequest sessionStartedRequest, Session session);
        void OnSessionEnded(SessionEndedRequest sessionEndedRequest, Session session);
    }
}