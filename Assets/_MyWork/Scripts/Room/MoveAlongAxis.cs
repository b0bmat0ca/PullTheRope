using DG.Tweening;
using UnityEngine;

public class MoveAlongAxis : MonoBehaviour
{
    private enum MoveAxis
    {
        x, y, z
    }
    [SerializeField] private MoveAxis moveAxis;

    [Header("移動距離"), Range(0, 38), SerializeField] private float moveDistance;
    [Header("逆移動"), SerializeField] private bool reverse;
    [Header("指定した距離を移動するのにかかる時間"), SerializeField] private float moveDuration = 5f;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = Vector3.zero;

        if (reverse)
        {
            moveDistance = -moveDistance;
        }

        switch (moveAxis)
        {
            case MoveAxis.x:
                endPosition = startPosition + new Vector3(moveDistance, 0, 0);
                break;
            case MoveAxis.y:
                endPosition = startPosition + new Vector3(0, moveDistance, 0);
                break;
            case MoveAxis.z:
                endPosition = startPosition + new Vector3(0, 0, moveDistance);
                break;
        }

        transform.DOMove(endPosition, moveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
