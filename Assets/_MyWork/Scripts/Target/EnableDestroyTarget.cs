using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;


public class EnableDestroyTarget : Target
{
    [SerializeField] protected RayfireRigid rayFireRigid;

    #region Target
    protected override async UniTaskVoid DestroyTarget()
    {
        // 粉砕する
        rayFireRigid.Demolish();

        // コライダーを無効化する
        colider.enabled = false;

        // 粉砕後、{destroyTime}秒後にDestroyする
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Destroy(meshRenderer.gameObject, destroyTime);
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
