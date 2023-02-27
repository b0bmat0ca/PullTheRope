using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DamageNumbersPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CheckPinch : MonoBehaviour
{
    public IObservable<bool> OnLeftCheckAsync => onLeftCheckAsyncSubject; // 左手OK通知用
    private readonly AsyncSubject<bool> onLeftCheckAsyncSubject = new();

    public IObservable<bool> OnRightCheckAsync => onRightCheckAsyncSubject;
    private readonly AsyncSubject<bool> onRightCheckAsyncSubject = new();

    [SerializeField] private OVRHand leftOVRHand;
    [SerializeField] private OVRHand rightOVRHand;

    [SerializeField] private SphereCollider leftThumbCollider;
    [SerializeField] private SphereCollider leftIndexCollider;
    [SerializeField] private SphereCollider rightThumbCollider;
    [SerializeField] private SphereCollider rightIndexCollider;

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
    private AudioSource audioSource;

    private bool visibleLeftFinger = false;
    private bool visibleRightFinger = false;

    private bool checkLeftPinch = false;
    private bool checkRightPinch = false;

    private void Awake()
    {
        onLeftCheckAsyncSubject.AddTo(this);
        onRightCheckAsyncSubject.AddTo(this);
        audioSource = GetComponent<AudioSource>();
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

        // 左手の人差し指と親指の衝突を購読
        leftIndexCollider.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("LeftFinger"))
            .Subscribe(async _ =>
            {
                if (checkLeftPinch)
                {
                    leftIndexCollider.gameObject.SetActive(false);
                    leftThumbCollider.gameObject.SetActive(false);
                    leftGuideHand.gameObject.SetActive(false);

                    AudioClip OKSE = GetSE("OKSE");
                    audioSource.PlayOneShot(OKSE);
                    okTextPrefab.Spawn(leftGuideHand.transform.position, "OK");

                    await UniTask.Delay(TimeSpan.FromSeconds(OKSE.length), cancellationToken: this.GetCancellationTokenOnDestroy());

                    onLeftCheckAsyncSubject.OnNext(true);
                    onLeftCheckAsyncSubject.OnCompleted();
                }
            }).AddTo(this);

        // 右手の人差し指と親指の衝突を購読
        rightIndexCollider.OnTriggerEnterAsObservable()
            .Where(other => other.CompareTag("RightFinger"))
            .Subscribe(async _ =>
            {
                if (checkRightPinch)
                {
                    rightIndexCollider.gameObject.SetActive(false);
                    rightThumbCollider.gameObject.SetActive(false);
                    rightGuideHand.gameObject.SetActive(false);

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
                checkLeftPinch = true;
            }
            else
            {
                checkLeftPinch = false;
            }    
        }
        else
        {
            checkLeftPinch = false;
        }

        if (rightOVRHand.HandConfidence == OVRHand.TrackingConfidence.High && visibleRightFinger)
        {
            if (rightOVRHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                checkRightPinch = true;
            }
            else
            {
                checkRightPinch = false;
            }
        }
        else
        {
            checkRightPinch = false;
        }
        
    }
}
