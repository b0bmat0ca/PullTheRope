using UnityEngine;

public class CannonMultiMove : MonoBehaviour
{
    public Vector3 TurretOffset{get; set;}  // 砲塔の配置位置によるベクトルのオフセット

    [SerializeField] private GunController gunController;

    [Header("左手の位置")] public Transform leftHandAnchor;
    [Header("右手の位置")] public Transform rightHandAnchor;
    [Header("回転するスピードの係数"), SerializeField] private float angularSpeed = 10f;

    private IInputEventProvider inputProvider;
    private Vector3 handReferenceVector = Vector3.back; // 回転開始の基準ベクトル
    private Transform handTransform;
    private float handOffset;
    private const float HAND_OFFSET = 19;   // 掴む位置による角度の誤差

    private void Awake()
    {
        TurretOffset = new(transform.position.x, 0, transform.position.z);
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
            Vector3 currentHandVector = new Vector3(handTransform.position.x, 0, handTransform.position.z) - TurretOffset;
            float handAngle = Vector3.SignedAngle(handReferenceVector, currentHandVector, Vector3.up);

            transform.Rotate(new Vector3(0, (handAngle - handOffset) * (angularSpeed * Time.deltaTime), 0));
            handReferenceVector = -transform.forward;
        }
    }
}
