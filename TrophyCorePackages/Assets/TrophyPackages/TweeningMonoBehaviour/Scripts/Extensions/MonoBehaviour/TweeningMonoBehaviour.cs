using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TweenScaleType
{
    Vector3,
    LocalPercentage
}

[Serializable]
public class TweeningMonoBehaviourSettingsData
{
    [Header("Tween Types")]
    public TweenScaleType tweenScaleType = TweenScaleType.Vector3;

    [Header("Cancel")]
    public bool cancelTweensOnDisable = false;

    [HideInInspector] public List<int> tweenIDs = new List<int>();
    [HideInInspector] public List<int> specialTweenIDs = new List<int>();
}

public class TweeningMonoBehaviour : MonoBehaviour
{
    [Header("Tween Behaviour Settings")]
    [SerializeField]
    public TweeningMonoBehaviourSettingsData behaviourSettings;

    //-------------
    // Delayed call.
    //-------------
    public void TweenDelayedCall(GameObject gameObject, float delay, Action action, bool ignoreTimeScale = false)
    {
        AddTweenID(false, LeanTween.delayedCall(gameObject, delay, action).setIgnoreTimeScale(ignoreTimeScale));
    }

    //-------------
    // Alpha Canvas.
    //-------------
    public void TweenAlphaCanvas(CanvasGroup group, float fadeTo, float time, float delay = 0, Action completeAction = null, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        AddTweenID(isSpecialTween, LeanTween.alphaCanvas(group, fadeTo, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }));
    }

    //-------------
    // Scale.
    //-------------
    public void TweenScaleLocalPercentage(GameObject gameObject, float delay, float time, Vector3 percentages, RectTransform transform, Action completeAction, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        TweenScale(gameObject, delay, time,
            new Vector3(transform.localScale.x * (percentages.x / 100f),
                transform.localScale.y * (percentages.y / 100f),
                transform.localScale.z * (percentages.z / 100f)),
            completeAction, isSpecialTween, easeType, ignoreTimeScale);
    }
    public void TweenScale(GameObject gameObject, float delay, float time, Vector3 toScale, Action completeAction, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        AddTweenID(isSpecialTween, LeanTween.scale(gameObject, toScale, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }));
    }

    //-------------
    // Move.
    //-------------
    public void TweenMove(GameObject gameObject, float delay, float time, Vector3 toPosition, Action completeAction, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        AddTweenID(isSpecialTween, LeanTween.move(gameObject, toPosition, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }));
    }
    public void TweenMoveLocal(GameObject gameObject, float delay, float time, Vector3 toLocalPosition, Action completeAction, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        AddTweenID(isSpecialTween, LeanTween.moveLocal(gameObject, toLocalPosition, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }));
    }

    //-------------
    // Value.
    //-------------
    public void TweenValue(GameObject gameObject, float fromValue, float toValue, float time, float delay = 0, Action<float> updateAction = null, Action completeAction = null, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    {
        AddTweenID(isSpecialTween, LeanTween.value(gameObject, fromValue, toValue, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnUpdate((float val) =>
        {
            updateAction?.Invoke(val);
        })
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }).setIgnoreTimeScale(ignoreTimeScale));
    }

    public void TweenValue(GameObject gameObject, Color fromValue, Color toValue, float time, float delay = 0, Action<Color> updateAction = null, Action completeAction = null, bool isSpecialTween = false, LeanTweenType easeType = LeanTweenType.notUsed, bool ignoreTimeScale = false)
    { 
        AddTweenID(isSpecialTween, LeanTween.value(gameObject, fromValue, toValue, time).setDelay(delay).setEase(easeType).setIgnoreTimeScale(ignoreTimeScale)
        .setOnUpdate((Color val) =>
        {
            updateAction?.Invoke(val);
        })
        .setOnComplete(() =>
        {
            completeAction?.Invoke();
        }));
    }


    #region Add Tween id
    public void AddTweenID(bool isSpecialTween, LTDescr tween)
    {
        if (isSpecialTween)
            behaviourSettings.specialTweenIDs.Add(tween.id);
        else
            behaviourSettings.tweenIDs.Add(tween.id);
    }

    #endregion

    #region Cleanup
    public void CancelAllTweens()
    {
        CancelStandardTweens();
        CancelSpecialTweens();
    }
    public void CancelStandardTweens()
    {
        foreach (var id in behaviourSettings.tweenIDs)
        {
            if (LeanTween.descr(id) != null)
                LeanTween.cancel(id);
        }
    }

    public void CancelSpecialTweens()
    {
        foreach (var id in behaviourSettings.specialTweenIDs)
        {
            if (LeanTween.descr(id) != null)
                LeanTween.cancel(id);
        }
    }

    public virtual void OnDisable()
    {
        if (behaviourSettings.cancelTweensOnDisable)
            CancelAllTweens();
    }

    public virtual void OnDestroy()
    {
        CancelAllTweens();
    }

    #endregion
}