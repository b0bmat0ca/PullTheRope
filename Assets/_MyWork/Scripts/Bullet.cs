using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("弾丸の生存時間"), SerializeField] private double lifeTIme = 3.0;

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

    public async UniTaskVoid Fire(Vector3 force, ObjectPool<Bullet> bulletPool)
    {
        _rigidbody.AddForce(force, ForceMode.Impulse);

        await UniTask.Delay(System.TimeSpan.FromSeconds(lifeTIme), cancellationToken: this.GetCancellationTokenOnDestroy());
        _rigidbody.velocity = Vector3.zero;
        bulletPool.Release(this);
    }
}
