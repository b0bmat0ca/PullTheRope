using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using Oculus.Platform.Samples.VrHoops;
using UniRx;
using UnityEngine;
using TMPro;

public abstract class PassthroughRoom : MonoBehaviour
{
    //public OVRSceneManager sceneManager;
    public TextMeshProUGUI text;    // デバッグ用

    public IObservable<bool> OnClearAsync => onClearAsyncSubject; // ルームクリア通知用
    protected readonly AsyncSubject<bool> onClearAsyncSubject = new();

    protected GameObject cannon;
    [SerializeField] protected  GameObject cannonBase;

    protected Camera mainCamera;
    protected const float groundDelta = 0.02f;
    protected const float cannonYOffset = 1.342f;

    protected Transform envRoot;
    protected OVRHand leftHand;
    protected OVRHand rightHand;
    protected HandGrabInteractor leftHandGrab;
    protected HandGrabInteractor rightHandGrab;
    protected Transform cannonParent;
    protected GameObject cannonPrefab;
    protected float cannonOffset;

    protected List<Vector3> cornerPoints = new();
    protected static List<SceneAnchormap> sceneAnchormap = new();

    [Serializable]
    protected class SceneAnchormap
    {
        public string name;
        public OVRSceneAnchor anchor;

        public SceneAnchormap(string name, OVRSceneAnchor anchor)
        {
            this.name = name;
            this.anchor = anchor;
        }
    }

    public virtual void Initialize(OVRHand leftHand, OVRHand rightHand
        , HandGrabInteractor leftHandGrab, HandGrabInteractor rightHandGrab
        ,Transform cannonParent, GameObject cannonPrefab, float cannonOffset)
    {
        this.leftHand= leftHand;
        this.rightHand= rightHand;
        this.leftHandGrab= leftHandGrab;
        this.rightHandGrab= rightHandGrab;
        this.cannonParent= cannonParent;
        this.cannonPrefab= cannonPrefab;
        this.cannonOffset= cannonOffset;

        cannon = cannonParent.GetComponentInChildren<CannonMultiMove>().gameObject;
        cannon.SetActive(false);
    }

    protected virtual void Awake()
    {
        onClearAsyncSubject.AddTo(this);
        mainCamera = Camera.main;
        envRoot = this.transform;
    }

    protected void InitializeCannon(bool reset = true)
    {
        Vector3 cannonPosition = CannonPosition(cannonYOffset);
        Vector3 cannonRotation = new(0, mainCamera.transform.eulerAngles.y, 0);
        Vector3 cannonBasePosition = new(cannonPosition.x, cannonBase.transform.position.y, cannonPosition.z);
        cannonBase.transform.SetPositionAndRotation(cannonBasePosition, Quaternion.identity);
        cannonBase.SetActive(true);
        if (reset)
        {
            CannonReset(cannonPosition, Quaternion.Euler(cannonRotation));
        }
        else
        {
            cannon.transform.SetPositionAndRotation(cannonPosition, Quaternion.Euler(cannonRotation));
        }

        InputEventProviderGrabbable inputEventProvider = cannon.GetComponent<InputEventProviderGrabbable>();
        CannonMultiMove cannonMultiMove = cannon.GetComponent<CannonMultiMove>();
        cannonMultiMove.TurretOffset = new(cannonMultiMove.transform.position.x, 0, cannonMultiMove.transform.position.z);
        inputEventProvider.leftHandInteractor = leftHandGrab;
        inputEventProvider.rightHandInteractor = rightHandGrab;
        cannonMultiMove.leftHandAnchor = leftHand.transform;
        cannonMultiMove.rightHandAnchor = rightHand.transform;
        cannon.SetActive(true);
    }

    protected GameObject CannonInstantiate(Vector3 position, Quaternion rotation)
    {
        cannon = Instantiate(cannonPrefab, position, rotation, cannonParent);
        cannon.SetActive(false);

        return cannon;
    }

    protected Vector3 CannonPosition(float yOffset)
    {
        Vector3 playerForward = mainCamera.transform.forward;
        return new Vector3(mainCamera.transform.position.x, yOffset, mainCamera.transform.position.z)
            + new Vector3(playerForward.x, 0, playerForward.z) * cannonOffset;
    }

    public void CannonReset(Vector3 position, Quaternion rotation)
    {
        Destroy(cannon);
        CannonInstantiate(position, rotation);
    }

    /// <summary>
    /// If an object contains the ForegroundObject component and is inside the room, destroy it.
    /// </summary>
    protected void CullForegroundObjects()
    {
        ForegroundObject[] foregroundObjects = envRoot.GetComponentsInChildren<ForegroundObject>();
        foreach (ForegroundObject obj in foregroundObjects)
        {
            if (cornerPoints != null && IsPositionInRoom(obj.transform.position))
            {
                Destroy(obj.gameObject);
            }
        }
    }

    /// <summary>
    /// Given a world position, test if it is within the floor outline (along horizontal dimensions)
    /// </summary>
    public bool IsPositionInRoom(Vector3 pos)
    {
        Vector3 floorPos = new Vector3(pos.x, cornerPoints[0].y, pos.z);
        // Shooting a ray from point to the right (X+), count how many walls it intersects.
        // If the count is odd, the point is in the room
        // Unfortunately we can't use Physics.RaycastAll, because the collision may not match the mesh, resulting in wrong counts
        int lineCrosses = 0;
        for (int i = 0; i < cornerPoints.Count; i++)
        {
            Vector3 startPos = cornerPoints[i];
            Vector3 endPos = (i == cornerPoints.Count - 1) ? cornerPoints[0] : cornerPoints[i + 1];

            // get bounding box of line segment
            float xMin = startPos.x < endPos.x ? startPos.x : endPos.x;
            float xMax = startPos.x > endPos.x ? startPos.x : endPos.x;
            float zMin = startPos.z < endPos.z ? startPos.z : endPos.z;
            float zMax = startPos.z > endPos.z ? startPos.z : endPos.z;
            Vector3 lowestPoint = startPos.z < endPos.z ? startPos : endPos;
            Vector3 highestPoint = startPos.z > endPos.z ? startPos : endPos;

            // it's vertically within the bounds, so it might cross
            if (floorPos.z <= zMax &&
                floorPos.z >= zMin)
            {
                if (floorPos.x <= xMin)
                {
                    // it's completely to the left of this line segment's bounds, so must intersect
                    lineCrosses++;
                }
                else if (floorPos.x < xMax)
                {
                    // it's within the bounds, so further calculation is needed
                    Vector3 lineVec = (highestPoint - lowestPoint).normalized;
                    Vector3 camVec = (floorPos - lowestPoint).normalized;
                    // polarity of cross product defines which side the point is on
                    if (Vector3.Cross(lineVec, camVec).y < 0)
                    {
                        lineCrosses++;
                    }
                }
                // else it's completely to the right of the bounds, so it definitely doesn't cross
            }
        }
        return (lineCrosses % 2) == 1;
    }

    public abstract void InitializRoom();
}
