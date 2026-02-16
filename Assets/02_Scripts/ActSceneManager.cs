using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ActId
{
    Scene1,
    Scene2,
    Scene3,
}

[Serializable]
public class ActScene
{
    public ActId id;

    [Header("Enable these when entering this act")]
    public List<GameObject> enable = new();

    [Header("Optional: force-disable these when entering this act")]
    public List<GameObject> disable = new();

    [Header("Events")]
    public UnityEvent onEnter;
    public UnityEvent onExit;
}

public class ActSceneManager : MonoBehaviour
{
    [Header("Acts (Theatrical Scenes)")]
    public List<ActScene> acts = new();

    [Header("Start")]
    public ActId startAct = ActId.Scene1;

    [Header("Transition")]
    [Tooltip("If true, acts are processed sequentially (queue). If false, SwitchAct is immediate.")]
    public bool useQueue = true;

    [Tooltip("Optional transition delay seconds (for blackout, sound cue, etc.)")]
    public float transitionDelay = 0f;

    private readonly Queue<ActId> _queue = new();
    private bool _transitioning = false;
    private ActId _currentAct;

    void Awake()
    {
        // 초기 상태 정리: 모든 enable 리스트에 있는 루트는 일단 끈다(중복 포함 가능)
        foreach (var act in acts)
        {
            foreach (var go in act.enable)
                if (go) go.SetActive(false);
        }

        // 시작 act 진입
        SwitchActImmediate(startAct);
    }

    public void Enqueue(ActId next)
    {
        if (!useQueue)
        {
            SwitchAct(next);
            return;
        }

        _queue.Enqueue(next);
        TryDequeue();
    }

    public void SwitchAct(ActId next)
    {
        if (useQueue)
        {
            Enqueue(next);
            return;
        }

        StartCoroutine(DoSwitch(next));
    }

    public void SwitchActImmediate(ActId next)
    {
        // exit
        var cur = FindAct(_currentAct);
        if (cur != null) cur.onExit?.Invoke();

        // enter
        ApplyAct(next);

        _currentAct = next;

        var nxt = FindAct(next);
        if (nxt != null) nxt.onEnter?.Invoke();
    }

    private void TryDequeue()
    {
        if (_transitioning) return;
        if (_queue.Count == 0) return;

        StartCoroutine(DoSwitch(_queue.Dequeue()));
    }

    private IEnumerator DoSwitch(ActId next)
    {
        _transitioning = true;

        var cur = FindAct(_currentAct);
        if (cur != null) cur.onExit?.Invoke();

        if (transitionDelay > 0f)
            yield return new WaitForSeconds(transitionDelay);

        ApplyAct(next);
        _currentAct = next;

        var nxt = FindAct(next);
        if (nxt != null) nxt.onEnter?.Invoke();

        _transitioning = false;
        TryDequeue();
    }

    private void ApplyAct(ActId next)
    {
        // 기본 정책: "다른 act의 enable 루트는 모두 끄고", next의 enable만 켠다
        // (이게 제일 깔끔하게 'Scene1 벽/창문' vs 'Scene2 벽/창문' 만들기 좋음)
        foreach (var act in acts)
        {
            foreach (var go in act.enable)
                if (go) go.SetActive(false);
        }

        var actNext = FindAct(next);
        if (actNext == null) return;

        foreach (var go in actNext.enable)
            if (go) go.SetActive(true);

        foreach (var go in actNext.disable)
            if (go) go.SetActive(false);
    }

    private ActScene FindAct(ActId id)
    {
        return acts.Find(a => a.id == id);
    }

    // 편의: 다음 act로 (Scene1->Scene2->Scene3 순서)
    public void Next()
    {
        int idx = (int)_currentAct + 1;
        if (idx >= Enum.GetValues(typeof(ActId)).Length) return;
        SwitchAct((ActId)idx);
    }

    public ActId Current => _currentAct;
}
