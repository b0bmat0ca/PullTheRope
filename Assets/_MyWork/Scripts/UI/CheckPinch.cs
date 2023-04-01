using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DamageNumbersPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CheckPinch : MonoBehaviour
{
    public IObservable<bool> OnLeftCheckAsync => onLeftCheckAsyncSubject; // 左手OK通知用
    private readonly AsyncSubject<bool> onLeftCheckAsyncSubject = new();

    public IObservable<bool> OnRightCheckAsync => onRightCheckAsyncSubject;
    private readonly AsyncSubject<bool> onRightCheckAsyncSubject = new();

    private BoolReactiveProperty checkLeftPinch = new(false);   // 左手でピンチをしたか
    private BoolReactiveProperty checkRightPinch = new(false);  // 右手でピンチをしたか

    [Header("ピンチポーズ練習回数"), SerializeField] private int startPinchCount = 5;
    private int leftPinchCount = 0;
    private int rightPinchCount = 0;

    [SerializeField] private OVRHand leftOVRHand;
    [SerializeField] private OVRHand rightOVRHand;

    [SerializeField] private GameObject leftGuideHand;
    [SerializeField] private GameObject rightGuideHand;

    [SerializeField] private DamageNumber okTextPrefab;

    // パーティクルリスト
    [SerializeField] protected List<ParticleMap> particleList;
    [System.Serializable]
    protected class ParticleMap
    {
        [SerializeField] private string particleName;
        [SerializeField] private ParticleSystem particle;

        public string ParticleName { get { return particleName; } }
        public ParticleSystem Particle { get { return particle; } }
    }
    protected ParticleSystem GetParticle(string psName)
    {
        foreach (ParticleMap item in particleList)
        {
            if (item.ParticleName == psName)
            {
                return item.Particle;
            }
        }
        return null;
    }

    // 効果音リスト
    [SerializeField] protected List<SEMap> seList;
    [System.Serializable]
    protected class SEMap
    {
        [SerializeField] private string seName;
        [SerializeField] private AudioClip se;

        public string SeName { get { return seName; } }
        public AudioClip Se { get { return se; } }
    }
    protected AudioClip GetSE(string seName)
    {
        foreach (SEMap item in seList)
        {
            if (item.SeName == seName)
            {
                return item.Se;
            }
        }
        return null;
    }

    private Camera mainCamera;
    [SerializeField] private AudioSource audioSource;

    private bool visibleLeftFinger = false;
    private bool visibleRightFinger = false;



    private void Awake()
    {
        onLeftCheckAsyncSubject.AddTo(this);
        onRightCheckAsyncSubject.AddTo(this);
    }


    // Start is called before the first frame update
    void Start()
    {
        // 左手がカメラに写っているか購読
        leftOVRHand.gameObject.GetComponent<VisibleCamera>()
            .OnVisibleCamera
            .Subscribe(x =>
            {
                visibleLeftFinger = x;
            }).AddTo(this);

        // 右手がカメラに写っているか購読
        rightOVRHand.gameObject.GetComponent<VisibleCamera>()
            .OnVisibleCamera
            .Subscribe(x =>
            {
                visibleRightFinger = x;
            }).AddTo(this);

        // 左手の人差し指と親指のピンチポーズを購読
        checkLeftPinch
            .Where(x => x)
            .Subscribe(async _ =>
            {
                leftPinchCount++;
                if (leftGuideHand.activeSelf &&leftPinchCount >=  startPinchCount)
                {
                    leftGuideHand.SetActive(false);

                    AudioClip OKSE = GetSE("OKSE");
                    audioSource.PlayOneShot(OKSE);
                    okTextPrefab.Spawn(leftGuideHand.transform.position, "OK");

                    await UniTask.Delay(TimeSpan.FromSeconds(OKSE.length), cancellationToken: this.GetCancellationTokenOnDestroy());

                    onLeftCheckAsyncSubject.OnNext(true);
                    onLeftCheckAsyncSubject.OnCompleted();
                }
            }).AddTo(this);

        // 右手の人差し指と親指のピンチポーズを購読
        checkRightPinch
            .Where(x => x)
            .Subscribe(async _ =>
            {
                rightPinchCount++;
                if(rightGuideHand.activeSelf &&rightPinchCount >= startPinchCount)
                { 
                    rightGuideHand.SetActive(false);

                    AudioClip OKSE = GetSE("OKSE");
                    audioSource.PlayOneShot(OKSE);
                    okTextPrefab.Spawn(rightGuideHand.transform.position, "OK");

                    await UniTask.Delay(TimeSpan.FromSeconds(OKSE.length), cancellationToken: this.GetCancellationTokenOnDestroy());

                    onRightCheckAsyncSubject.OnNext(true);
                    onRightCheckAsyncSubject.OnCompleted();
                }
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (leftOVRHand.HandConfidence == OVRHand.TrackingConfidence.High && visibleLeftFinger)
        {
            if (leftOVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                checkLeftPinch.Value = true;
            }
            else
            {
                checkLeftPinch.Value = false;
            }    
        }
        else
        {
            checkLeftPinch.Value = false;
        }

        if (rightOVRHand.HandConfidence == OVRHand.TrackingConfidence.High && visibleRightFinger)
        {
            if (rightOVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                checkRightPinch.Value = true;
            }
            else
            {
                checkRightPinch.Value = false;
            }
        }
        else
        {
            checkRightPinch.Value = false;
        }
        
    }
}
