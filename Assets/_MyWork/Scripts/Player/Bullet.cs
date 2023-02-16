using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public ReactiveProperty<bool> onRelease = new(false);   // オブジェクトプールにリリースを許可するフラグ

    [Header("弾丸の生存時間")] public double lifeTIme = 3.0;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
