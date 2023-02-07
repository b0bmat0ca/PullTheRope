using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

public class CheckPinch : MonoBehaviour
{
    public IObservable<bool> OnLeftCheckAsync => onLeftCheckAsyncSubject; // 左手OK通知用
    private readonly AsyncSubject<bool> onLeftCheckAsyncSubject = new();

    public IObservable<bool> OnRightCheckAsync => onRightCheckAsyncSubject;
    private readonly AsyncSubject<bool> onRightCheckAsyncSubject = new();

    [SerializeField] private SphereCollider leftThumbCollider;
    [SerializeField] private SphereCollider leftIndexCollider;
    [SerializeField] private SphereCollider rightThumbCollider;
    [SerializeField] private SphereCollider rightIndexCollider;

    [SerializeField] private GameObject leftGuideHand;
    [SerializeField] private GameObject rightGuideHand;


    private void Awake()
    {
        onLeftCheckAsyncSubject.AddTo(this);
        onRightCheckAsyncSubject.AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // 左手の人差し指と親指の衝突
        leftIndexCollider.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("LeftFinger"))
            .Subscribe(async _ =>
            {
                leftIndexCollider.gameObject.SetActive(false);
                leftThumbCollider.gameObject.SetActive(false);
                leftGuideHand.gameObject.SetActive(false);

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

                onLeftCheckAsyncSubject.OnNext(true);
                onLeftCheckAsyncSubject.OnCompleted();
            }).AddTo(this);

        // 右手の人差し指と親指の衝突
        rightIndexCollider.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("RightFinger"))
            .Subscribe(async _ =>
            {
                rightIndexCollider.gameObject.SetActive(false);
                rightThumbCollider.gameObject.SetActive(false);
                rightGuideHand.gameObject.SetActive(false);

                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

                onRightCheckAsyncSubject.OnNext(true);
                onRightCheckAsyncSubject.OnCompleted();
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
