﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;
using UniRx;
using Cysharp.Threading.Tasks;
using System;


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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private  void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DestroyTarget().Forget();
        }
    }
}