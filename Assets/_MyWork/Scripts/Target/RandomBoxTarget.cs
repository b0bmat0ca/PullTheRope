using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RayFire;
using UniRx;
using UniRx.Triggers;
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
        colider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Bullet"))
            .Subscribe(collision =>
            {
                audioSource.PlayOneShot(GetSE("BulletHit"));
                InstantiateRandomObject();

            }).AddTo(this);
    }

    public void InstantiateRandomObject()
    {
        if (onDestroy)
        {
            return;
        }

        boxPattern.SetActive(false);
        GameObject obj = Instantiate(items[Random.Range(0, items.Length)], itemSpaawnPoint.position, Quaternion.identity);
        obj.SetActive(false);
        obj.transform.localScale = transform.localScale;
        obj.transform.SetParent(gameObject.transform.parent);
        obj.SetActive(true);

        // ランダムボックスの破壊
        DestroyBox();
    }

    public void DestroyBox()
    {
        if (onDestroy)
        {
            return;
        }

        DestroyTarget().Forget();
    }
}
