using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx.Triggers;
using UniRx;
using UnityEngine;
using System;

[RequireComponent(typeof(AudioSource))]
public abstract class Target : MonoBehaviour
{
    public IObservable<bool> OnDestroyAsync => onDestroyAsyncSubject; // ターゲット破壊通知用
    protected readonly AsyncSubject<bool> onDestroyAsyncSubject = new();

    [SerializeField] protected TargetData targetData;
    [SerializeField] protected Collider colider;
    public Collider TargetColider { get { return colider; } }
    [SerializeField] protected float destroyTime = 5.0f;

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

    protected string _name;
    protected int point;
    protected bool enableRigidBody;

    protected AudioSource audioSource;
    protected MeshRenderer[] meshRenderers;
    protected CancellationTokenSource tokenSource = new();
    protected CancellationToken token;

    protected void OnDestroy()
    {
        tokenSource.Cancel();
    }

    protected virtual void Awake()
    {
        _name = targetData.Name;
        point = targetData.Point;
        enableRigidBody = targetData.EnableRigidBody;
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        token = tokenSource.Token;
    }

    protected abstract UniTaskVoid DestroyTarget();
}
