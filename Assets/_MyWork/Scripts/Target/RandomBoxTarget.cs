using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RayFire;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class RandomBoxTarget : Target
{
    [SerializeField] private GameObject boxPattern;
    [SerializeField] private Transform itemSpaawnPoint;
    [SerializeField] private GameObject[] items;

    private bool onDestroy = false;

    #region Target
    protected override async UniTaskVoid DestroyTarget()
    {
        onDestroy = true;
        await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: token);

        Destroy(gameObject);
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        token = tokenSource.Token;

        // 弾丸と衝突したかを購読
        subject.OnCollisionEnterAsync
            .Subscribe(_ =>
            {
                audioSource.PlayOneShot(GetSE("BulletHit"));
                if (!onDestroy)
                {
                    InstantiateRandomObject();
                }
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateRandomObject()
    {
        boxPattern.SetActive(false);
        GameObject obj = Instantiate(items[Random.Range(0, items.Length - 1)], itemSpaawnPoint.position, Quaternion.identity);
        obj.SetActive(false);
        obj.transform.localScale = transform.localScale;
        obj.SetActive(true);

        // ランダムボックスの破壊
        DestroyTarget().Forget();
    }
}
