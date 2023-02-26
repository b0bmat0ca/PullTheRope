using System;
using Cysharp.Threading.Tasks;
using RayFire;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class EnableDestroyTarget : Target
{
    [SerializeField] protected RayfireRigid rayFireRigid;

    #region Target
    protected override async UniTaskVoid DestroyTarget()
    {
        if (!colider.enabled)
        {
            return;
        }

        // コライダーを無効化する
        colider.enabled = false;

        // 得点追加
        if (model.Time.Value > 0)
        {
            pointTextPrefab.Spawn(transform.position, point);
            model.Score.Value += point;
        }

        // 粉砕する
        rayFireRigid.Demolish();

        // 粉砕後、{destroyTime}秒後にDestroyする
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Destroy(meshRenderer.gameObject, destroyTime);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: token);

        // 購読者に破壊を通知
        onDestroyAsyncSubject.OnNext(true);
        onDestroyAsyncSubject.OnCompleted();

        Destroy(gameObject);
    }
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 弾丸と衝突したかを購読
        if (enableRigidBody)
        {
            this.OnCollisionEnterAsObservable()
                .Where(collision => collision.gameObject.CompareTag("Bullet"))
                .Subscribe(_ =>
                {
                    audioSource.PlayOneShot(GetSE("BulletHit"));
                    DestroyTarget().Forget();

                }).AddTo(this);
        }
        else
        {
            colider.OnCollisionEnterAsObservable()
                .Where(collision => collision.gameObject.CompareTag("Bullet"))
                .Subscribe(_ =>
                {
                    audioSource.PlayOneShot(GetSE("BulletHit"));
                    DestroyTarget().Forget();

                }).AddTo(this);
        }
    }

    public void PunchTarget()
    {
        DestroyTarget().Forget();
    }
}
