using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CannonMove : MonoBehaviour
{
    [SerializeField] private GunController gunController;
    [Header("左手の位置"),SerializeField] private Transform leftHandAnchor;
    [Header("右手の位置"), SerializeField] private Transform rightHandAnchor;
    [Header("回転するスピードの係数"), SerializeField] private float angularSpeed = 1000f;
    [Header("回転角の制限")]
    [SerializeField, Range(-180, 0)] private float minAngle;
    [SerializeField, Range(0, 180)] private float maxAngle;

    private IInputEventProvider inputProvider;

    private float previousX = -99;   // 砲塔を離したという意味で、掴んでいる時にありえない数字を設定

    // Start is called before the first frame update
    void Start()
    {
        inputProvider = GetComponent<IInputEventProvider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inputProvider.IsTurretGrab.Value)
        {
            previousX = -99;
            return;
        }

        switch (inputProvider.TurretGrabbed)
        {
            case TurretGrabbedHand.Left:
                UpdateCannonRotate(leftHandAnchor);
                break;
            case TurretGrabbedHand.Right:
                UpdateCannonRotate(rightHandAnchor);
                break;
        }
    }

    /// <summary>
    /// 砲塔を握っている手のX軸の増減を利用して、砲台を回転させる
    /// </summary>
    /// <param name="handTransform"></param>
    private void UpdateCannonRotate(Transform handTransform)
    {
        if (previousX == -99)
        {
            previousX = handTransform.position.x;
        }
        else if (handTransform.position.x > previousX)
        {
            if (ConstraintMinAngle())
            {
                return;
            }
            transform.Rotate(Vector3.up 
                * ((-1) * angularSpeed * Mathf.Abs(handTransform.position.x - previousX) * Time.deltaTime)
                );
        }
        else if (handTransform.position.x < previousX)
        {
            if (ConstraintMaxAngle())
            {
                return;
            }
            transform.Rotate(Vector3.up
                * (angularSpeed * Mathf.Abs(previousX - handTransform.position.x) * Time.deltaTime)
                );
        }
    }

    /// <summary>
    /// 回転角最小値が -180 ～ 0 の場合に対応した最小値制限
    /// </summary>
    /// <returns></returns>
    private bool ConstraintMinAngle()
    {
        if (minAngle == 0)
        {
            return false;
        }
        else if (transform.localRotation.eulerAngles.y > 180)
        {
            float eulerAngleY = transform.localRotation.eulerAngles.y - 360;

            return eulerAngleY <= minAngle;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 回転角最大値が、0 ～ 180 の場合に対応した最大値制限
    /// </summary>
    /// <returns></returns>
    private bool ConstraintMaxAngle()
    {
        if(maxAngle == 0 || transform.localRotation.eulerAngles.y > 180)
        {
            return false;
        }
        else
        {
            return transform.localEulerAngles.y >= maxAngle;
        }
    }
}
