using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UniRx;
using UnityEngine;

public class CannonMultiMove : MonoBehaviour
{
    [SerializeField] private GunController gunController;
    [Header("左手の位置"),SerializeField] private Transform leftHandAnchor;
    [Header("右手の位置"), SerializeField] private Transform rightHandAnchor;
    [Header("回転するスピードの係数"), SerializeField] private float angularSpeed = 10f;

    private IInputEventProvider inputProvider;
    private Vector3 turretOffset;   // 砲塔の配置位置によるベクトルのオフセット
    private Vector3 handReferenceVector = Vector3.back; // 回転開始の基準ベクトル
    private Transform handTransform;
    private float handOffset;
    private const float HAND_OFFSET = 19;   // 掴む位置による角度の誤差

    private void Awake()
    {
        turretOffset = new(transform.position.x, 0, transform.position.z);
    }
    // Start is called before the first frame update
    void Start()
    {
        inputProvider = GetComponent<IInputEventProvider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputProvider.IsTurretGrab.Value)
        {
            if (inputProvider.TurretGrabbed == TurretGrabbedHand.Left)
            {
                handTransform = leftHandAnchor;
                handOffset = HAND_OFFSET;
            }
            else
            {
                handTransform = rightHandAnchor;
                handOffset = -HAND_OFFSET;
            }
            Vector3 currentHandVector = new Vector3(handTransform.position.x, 0, handTransform.position.z) - turretOffset;
                
            Debug.Log("currentHandVector :" +currentHandVector);

            float handAngle = Vector3.SignedAngle(handReferenceVector, currentHandVector, Vector3.up);
            Debug.Log("handAngle" + handAngle);

            transform.Rotate(new Vector3(0, (handAngle - handOffset) * (angularSpeed * Time.deltaTime), 0));
            handReferenceVector = -transform.forward;
        }
        else
        {

        }
    }
}
