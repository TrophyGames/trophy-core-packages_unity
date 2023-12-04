using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityManagerDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (QualityManager.Instance.DynamicQualityManagementActive)
                QualityManager.Instance.SetDynamicQualityManagementInactive();
            else
                QualityManager.Instance.SetDynamicQualityManagementActive();

        }

        if (!QualityManager.Instance.DynamicQualityManagementActive)
        {
            if (Input.GetKeyDown(KeyCode.W) && QualityManager.Instance.CurrentQualityTier != QualityManager.Instance.CurrentConfig.maxQualityTier)
                QualityManager.Instance.ManuallyChangeQualityTier(QualityManager.Instance.CurrentQualityTier + 1);
            else if (Input.GetKeyDown(KeyCode.S) && QualityManager.Instance.CurrentQualityTier != QualityManager.Instance.CurrentConfig.minQualityTier)
                QualityManager.Instance.ManuallyChangeQualityTier(QualityManager.Instance.CurrentQualityTier - 1);
        }
    }
}
