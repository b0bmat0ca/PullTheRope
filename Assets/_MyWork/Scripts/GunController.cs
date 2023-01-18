using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UniRx;
using Oculus.Interaction;

public class GunController : MonoBehaviour
{
    [Header("紐"), SerializeField] private ObiRope rope;
    [Header("トリガー"), SerializeField] private Grabbable trigger;
    [Header("発射口の位置"), SerializeField] private Transform muzzle;
    [Header("弾倉"), SerializeField] private MagazineCartridgeController magazineCartridge;

    [Header("鉄砲のパワー"), SerializeField] private float gunPower = 20f;
    [Header("発射する紐の長さ"), SerializeField] private float fireRopeLength = 0.4f;

    private float defaultRopeLength;    // 初期の紐の長さ
    private float resetRopeLength;  // 発射計測を開始する紐の長さ
    private float previousRopeLength;   // 発射計測フラグ用の紐の長さ

    private float pullStartTime;    // 発射判定を計測するための開始時間

    private bool isFire = false;    // 発射処理フラグ

    private ReactiveProperty<float> ropeLength = new(); // 紐の長さ監視

    private void Awake()
    {
        ropeLength.AddTo(this);
        previousRopeLength = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        defaultRopeLength = rope.CalculateLength(); // 0.3
        ropeLength.Value = defaultRopeLength;

        resetRopeLength = (defaultRopeLength + fireRopeLength) / 2; // 0.35

        ropeLength
            .Subscribe(x => 
            {
                if (x > fireRopeLength && !isFire)
                {
                    isFire = true;
                    Fire(CalculateGunSpeed());
                }
                else if ( x <= resetRopeLength)
                {
                    isFire = false;
                    previousRopeLength = 0;
                }
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger.GrabPoints.Count > 0)
        {
            ropeLength.Value = rope.CalculateLength();

            if (previousRopeLength == 0)
            {
                previousRopeLength = ropeLength.Value;
            }

            if (previousRopeLength >= ropeLength.Value && ropeLength.Value <= fireRopeLength)
            {
                previousRopeLength = ropeLength.Value;

                pullStartTime = Time.time;
            }
        }

#if UNITY_EDITOR
        // テスト用コード
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire(gunPower);
        }
#endif
    }

    /// <summary>
    /// 発射する弾丸のスピードを計算する
    /// </summary>
    /// <returns></returns>
    private float CalculateGunSpeed()
    {
        // 引っ張り方によって、スピードを変更する
        return gunPower / (Time.time - pullStartTime + 1);
    }

    /// <summary>
    /// 弾丸を発射する
    /// </summary>
    /// <param name="bulletSpeed"></param>
    private void Fire(float bulletSpeed)
    {
#if !UNITY_EDITOR
        // 握られていない場合は、発射しない
        if (trigger.GrabPoints.Count == 0)
        {
            return;
        }
#endif

        magazineCartridge.bulletPool.Get().Fire(muzzle.forward * bulletSpeed, magazineCartridge.bulletPool).Forget();
    }
}
