using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFramerateQualityManager : QualitySubManager
{
    public void Initialize(int targetFramerate)
    {
        Application.targetFrameRate = targetFramerate;
    }
}
