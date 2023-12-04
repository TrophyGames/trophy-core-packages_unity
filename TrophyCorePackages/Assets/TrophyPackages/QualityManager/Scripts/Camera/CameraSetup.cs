using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    public Camera cameraReference;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    public IEnumerator Setup()
    {
        while (!QualityManager.Instance.IsInitialized)
        {
            yield return new WaitForEndOfFrame();
        }

        CameraManager.Instance.AddCamera(cameraReference, true);
    }
}
