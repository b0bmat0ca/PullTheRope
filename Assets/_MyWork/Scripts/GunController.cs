using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using UniRx;

public class GunController : MonoBehaviour
{
    [Header("紐"), SerializeField] private ObiRope rope;
    [Header("発射口の位置"), SerializeField] private Transform muzzle;
    [Header("弾倉"), SerializeField] private MagazineCartridgeController magazineCartridge;
    [Header("弾丸のスピード"), SerializeField] private float bulletSpeed = 10f;
    [Header("発射する紐の長さ"), SerializeField] private float fireRopeLength = 0.4f;

    private float defaultRopeLength;
    private float resetRopeLength;

    private bool isFire = false;

    private ReactiveProperty<float> ropeLength = new();

    private void Awake()
    {
        ropeLength.AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        defaultRopeLength = rope.CalculateLength();
        ropeLength.Value = defaultRopeLength;

        resetRopeLength = (defaultRopeLength + fireRopeLength) / 2;


        // ロープの長さを監視
        ropeLength
            .Subscribe(x => 
            {
                if (x > fireRopeLength && !isFire)
                {
                    Fire();
                    isFire= true;
                }
                else if ( x <= resetRopeLength)
                {
                    isFire = false;
                }
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        ropeLength.Value = rope.CalculateLength();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    private void Fire()
    {
        // @todo 引っ張り方によって、加える力を変更する
        magazineCartridge.bulletPool.Get().Fire(muzzle.forward * bulletSpeed, magazineCartridge.bulletPool).Forget();
    }
}
