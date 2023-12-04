using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TierQualityManager : QualitySubManager
{
    public override void Initialize(QualityManager.QualityTier initialQualityTier) 
    {
        if (QualitySettings.GetQualityLevel() != (int)initialQualityTier)
            QualitySettings.SetQualityLevel((int)initialQualityTier, true);
    }

    public override void OnQualityTierChanged(QualityManager.QualityTier newQualityTier) 
    {
        if (QualitySettings.GetQualityLevel() != (int)newQualityTier)
            QualitySettings.SetQualityLevel((int)newQualityTier, default);
    }
}
