using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QualitySubManager : MonoBehaviour
{
    public virtual void Initialize(QualityManager.QualityTier initialQualityTier) { }

    public virtual void OnQualityTierChanged(QualityManager.QualityTier newQualityTier) { }

    public virtual void OnQualityManagementBegin() { }

    public virtual void OnQualityManagementEnd() { }
}
