using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class MagazineCartridgeController : MonoBehaviour
{
    [Header("弾丸の発射位置"), SerializeField] private Transform muzzle;

    [NonSerialized] public ObjectPool<Bullet> bulletPool;

    private Bullet[] bullets;
    private int currentBulletIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // オブジェクトプール用の弾丸を取得
        bullets = GetComponentsInChildren<Bullet>();

        foreach (Bullet bullet in bullets)
        {
            bullet.gameObject.SetActive(false);
        }

        // オブジェクトプールの生成
        bulletPool = new ObjectPool<Bullet>(
            CreateFunc,
            ActionOnGet,
            ActionOnRelease,
            ActionOnDestroy,
            collectionCheck: true,
            defaultCapacity: bullets.Length,
            maxSize: bullets.Length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Bullet CreateFunc()
    {
        if (currentBulletIndex >= bullets.Length)
        {
            return null;
        }

        return bullets[currentBulletIndex++];
    }

    private void ActionOnGet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
        bullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
    }

    private void ActionOnRelease(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void ActionOnDestroy(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}
