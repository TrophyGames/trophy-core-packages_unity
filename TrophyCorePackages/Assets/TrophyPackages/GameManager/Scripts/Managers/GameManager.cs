using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Trophy.Logging;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    // ---------------------------------------------
    // Initialization.
    // ---------------------------------------------
    private void Start()
    {
        StartCoroutine(InitializeSubManagers());
    }

    private IEnumerator InitializeSubManagers()
    {
        LogUtility.Log("Initialize subManagers called", LogCondition.Full);

        // TODO: Add all submanager initialization methods here.
        yield return null;
    }

    // ---------------------------------------------
    // Application methods.
    // ---------------------------------------------
    private void OnApplicationPause(bool pause)
    {
        LogUtility.Log("Application pause called", LogCondition.Full);

        // TODO: Add pause logic here.
    }

    private void OnApplicationFocus(bool focus)
    {
        LogUtility.Log("Application focus called", LogCondition.Full);

        // TODO: Add focus logic here.
    }

    private void OnApplicationQuit()
    {
        LogUtility.Log("Application quit called", LogCondition.Full);

        // TODO: Add quit logic here.
    }
}