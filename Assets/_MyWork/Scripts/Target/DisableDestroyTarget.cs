using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public class DisableDestroyTarget : Target
{
    #region Target
    protected override async UniTaskVoid DestroyTarget()
    {
        // コライダーを無効化する
        colider.enabled = false;

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.enabled = false;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: token);

        Destroy(gameObject);
    }
    #endregion

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // 弾丸と衝突したかを購読
        subject.OnCollisionEnterAsync
            .Subscribe(_ =>
            {
                DestroyTarget().Forget();
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
