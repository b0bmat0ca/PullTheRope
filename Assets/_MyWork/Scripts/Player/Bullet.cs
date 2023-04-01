using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Bullet : MonoBehaviour
{
    public ReactiveProperty<bool> onRelease = new(false);   // オブジェクトプールにリリースを許可するフラグ

    [NonSerialized] public double lifeTIme = 3.0;

    private AudioSource audioSource;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Ground"))
            .Subscribe(_ =>
            {
                audioSource.PlayOneShot(audioSource.clip);

            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Vector3 force)
    {
        _rigidbody.AddForce(force, ForceMode.Impulse);
        Release().Forget();
    }

    private async UniTaskVoid Release()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTIme), cancellationToken: this.GetCancellationTokenOnDestroy());

        _rigidbody.velocity = Vector3.zero;
        onRelease.Value = true;
    }
}
