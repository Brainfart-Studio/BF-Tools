using UnityEngine;
using BFTools.Core.EventBus;
using BFTools.Feedback.Hitstop;
using BFTools.Feedback.ScreenShake;
using BFTools.Feedback.ScreenFlash;
using BFTools.Feedback.Haptics;

namespace BFTools.EditorTools.ProjectVerification.Editor
{
    public class BFProjectVerificationTrigger : MonoBehaviour
    {
        public const string EventName = "Default";

        public void TriggerHitstop()
        {
            EventBus<BFHitstopEvent>.Fire(new BFHitstopEvent { eventName = EventName });
        }

        public void TriggerScreenShake()
        {
            EventBus<BFScreenShakeEvent>.Fire(new BFScreenShakeEvent { eventName = EventName });
        }

        public void TriggerScreenFlash()
        {
            EventBus<BFScreenFlashEvent>.Fire(new BFScreenFlashEvent { eventName = EventName });
        }

        public void TriggerHaptics()
        {
            EventBus<BFHapticsEvent>.Fire(new BFHapticsEvent { eventName = EventName });
        }
    }
}