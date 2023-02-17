using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using UniRx.Triggers;
using Oculus.Interaction;
using System.Linq;

public class DisableDestroyTarget : Target
{
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

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
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
                .Subscribe(collision =>
                {
                    if (collision.gameObject.CompareTag("Bullet"))
                    {
                        audioSource.PlayOneShot(GetSE("BulletHit"));
                        DestroyTarget().Forget();
                    }
                    else if (collision.gameObject.name.StartsWith("Hand"))
                    {
                        if (!audioSource.isPlaying)
                        {
                            audioSource.PlayOneShot(GetSE("HandHit"));
                        }
                        if (enableHandDestroy)
                        {
                            DestroyTarget().Forget();
                        }
                    }
                }).AddTo(this);
        }
        else
        {
            colider.OnCollisionEnterAsObservable()
                .Subscribe(collision =>
                {
                    if (collision.gameObject.CompareTag("Bullet"))
                    {
                        audioSource.PlayOneShot(GetSE("BulletHit"));
                        DestroyTarget().Forget();
                    }
                    else if (collision.gameObject.name.StartsWith("Hand"))
                    {
                        if (!audioSource.isPlaying)
                        {
                            audioSource.PlayOneShot(GetSE("HandHit"));
                        }
                        if (enableHandDestroy)
                        {
                            DestroyTarget().Forget();
                        }
                    }

                }).AddTo(this);
        }

        audioSource.PlayOneShot(GetSE("Instantiate"));
    }
}
