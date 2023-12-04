using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioQualityManager : QualitySubManager
{
    private AudioConfiguration config;

    public override void Initialize(QualityManager.QualityTier initialQualityTier)
    {
        config = AudioSettings.GetConfiguration();
        SetAudioQualityLevel(initialQualityTier);
    }

    public override void OnQualityTierChanged(QualityManager.QualityTier newQualityTier)
    {
        SetAudioQualityLevel(newQualityTier);
    }

    void SetAudioQualityLevel(QualityManager.QualityTier newQualityTier)
    {
        if (QualityManager.Instance.CurrentConfig.forceMonoAudioTiers[(int)newQualityTier])
            config.speakerMode = AudioSpeakerMode.Mono;
        else
            config.speakerMode = AudioSpeakerMode.Stereo;
    }
}
