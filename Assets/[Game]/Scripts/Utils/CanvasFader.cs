using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    public delegate void FadeEvent();
    public event FadeEvent OnShow;
    public event FadeEvent OnHide;

    [SerializeField] float duration;
    [SerializeField] float delay;

    CanvasGroup group;
    Tweener tween;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    public void SetDelay(float value) => delay = value;
    public void SetDuration(float value) => duration = value;

    public void SetGroup(bool value)
    {
        group.alpha = (value) ? 1f : 0f;
        group.interactable = value;
        group.blocksRaycasts = value;

        tween?.Kill();
    }

    public void Show()
    {
        OnShow?.Invoke();
        tween?.Kill();

        tween = group.DOFade(1f, duration).SetDelay(delay).OnComplete(() =>
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        });
    }

    public void Hide()
    {
        OnHide?.Invoke();
        tween?.Kill();

        tween = group.DOFade(0f, duration).SetDelay(delay).OnComplete(() =>
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        });
    }

    public void Show(Action completed = null)
    {
        OnShow?.Invoke();
        tween?.Kill();

        tween = group.DOFade(1f, duration).SetDelay(delay).OnComplete(() =>
        {
            group.interactable = true;
            group.blocksRaycasts = true;

            completed?.Invoke();
        });
    }

    public void Hide(Action completed = null)
    {
        OnHide?.Invoke();
        tween?.Kill();

        tween = group.DOFade(0f, duration).SetDelay(delay).OnComplete(() =>
        {
            group.interactable = false;
            group.blocksRaycasts = false;

            completed?.Invoke();
        });
    }
}
