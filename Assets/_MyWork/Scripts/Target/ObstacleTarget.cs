using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObstacleTarget : MonoBehaviour
{
    [SerializeField] private Collider colider;

    // パーティクルリスト
    [SerializeField] private List<ParticleMap> particleList;
    [System.Serializable]
    private class ParticleMap
    {
        [SerializeField] private string particleName;
        [SerializeField] private ParticleSystem particle;

        public string ParticleName { get { return particleName; } }
        public ParticleSystem Particle { get { return particle; } }
    }
    private ParticleSystem GetParticle(string psName)
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
    [SerializeField] private List<SEMap> seList;
    [System.Serializable]
    private class SEMap
    {
        [SerializeField] private string seName;
        [SerializeField] private AudioClip se;

        public string SeName { get { return seName; } }
        public AudioClip Se { get { return se; } }
    }
    private AudioClip GetSE(string seName)
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

    private AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        colider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Bullet"))
            .Subscribe(_ =>
            {
                audioSource.PlayOneShot(GetSE("BulletHit"));

            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
