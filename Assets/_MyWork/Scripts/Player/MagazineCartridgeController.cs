using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(AudioSource))]
public class MagazineCartridgeController : MonoBehaviour
{
    [Header("弾丸の発射位置")] public Transform muzzle;
    
    [NonSerialized] public ObjectPool<Bullet> bulletPool;

    [Header("弾丸のPrafab"), SerializeField] private GameObject bulletPrefab;
    private Bullet[] bullets = new Bullet[20];
    private int currentBulletIndex = 0;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource= GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetBullet(bulletPrefab);
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
        audioSource.PlayOneShot(audioSource.clip);
    }

    private void ActionOnRelease(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void ActionOnDestroy(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    public void ResetBullet(GameObject prefab)
    {
        // オブジェクトプール用の弾丸を取得して、Bulletコンポーネントを付与
        for (int i = 0; i < bullets.Length; i++)
        {
            Bullet bullet = Instantiate(prefab, this.transform).AddComponent<Bullet>();
            bullets[i] = bullet;
            bullet.onRelease
                .Where(x => x)
                .Subscribe(_ =>
                {
                    bullet.onRelease.Value = false;
                    bulletPool.Release(bullet);
                }).AddTo(this);
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
}
