using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using Oculus.Platform.Samples.VrHoops;
using UniRx;
using UnityEngine;
using TMPro;
using System.Threading;

[RequireComponent(typeof(AudioSource))]
public abstract class PassthroughRoom : MonoBehaviour
{
    public IObservable<bool> OnInitializeAsync => onInitializeAsyncSubject;
    protected readonly AsyncSubject<bool> onInitializeAsyncSubject = new(); // ルーム初期化完了通知用

    public IObservable<bool> OnClearAsync => onClearAsyncSubject; // ルームクリア通知用
    protected readonly AsyncSubject<bool> onClearAsyncSubject = new();

    [Header("弾倉"), SerializeField] protected MagazineCartridgeController magazineCartridge;

    // パーティクルリスト
    [SerializeField] protected List<ParticleMap> particleList;
    [System.Serializable]
    protected class ParticleMap
    {
        [SerializeField] private string particleName;
        [SerializeField] private ParticleSystem particle;

        public string ParticleName { get { return particleName; } }
        public ParticleSystem Particle { get { return particle; } }
    }
    protected ParticleSystem GetParticle(string psName)
    {
        foreach (ParticleMap item in particleList)
        {
            if (item.ParticleName == psName)
            {
                return item.Particle;
            }
        }
        return null;
    }

    // 効果音リスト
    [SerializeField] protected List<SEMap> seList;
    [System.Serializable]
    protected class SEMap
    {
        [SerializeField] private string seName;
        [SerializeField] private AudioClip se;

        public string SeName { get { return seName; } }
        public AudioClip Se { get { return se; } }
    }
    protected AudioClip GetSE(string seName)
    {
        foreach (SEMap item in seList)
        {
            if (item.SeName == seName)
            {
                return item.Se;
            }
        }
        return null;
    }

    protected Transform player;
    protected GameObject cannon;
    
    protected const float groundDelta = 0.02f;

    protected Transform envRoot;
    protected AudioSource audioSource;

    protected OVRHand leftHand;
    protected OVRHand rightHand;
    protected HandGrabInteractor leftHandGrab;
    protected HandGrabInteractor rightHandGrab;
    protected Transform cannonParent;
    protected GameObject cannonPrefab;

    protected List<Vector3> cornerPoints = new();
    
    protected static List<SceneAnchorClassification> sceneAnchorClassifications = new();

    /// <summary>
    /// SceneAnchorを保持するクラス
    /// </summary>
    [Serializable]
    protected class SceneAnchorClassification
    {
        public string classification;
        public List<OVRSceneAnchor> anchors;

        public SceneAnchorClassification(string classification, List<OVRSceneAnchor> anchors)
        {
            this.classification = classification;
            this.anchors = anchors;
        }
    }

    /// <summary>
    /// SceneAnchorを種別毎に取得するクラス
    /// </summary>
    /// <param name="classification"></param>
    /// <returns></returns>
    protected SceneAnchorClassification GetSceneAnchorClassification(string classification)
    {
        foreach (SceneAnchorClassification sceneAnchorClassification in sceneAnchorClassifications)
        {
            if (sceneAnchorClassification.classification == classification)
            {
                return sceneAnchorClassification;
            }
        }
        return null;
    }

    public virtual void Initialize(Transform player, OVRHand leftHand, OVRHand rightHand
        , HandGrabInteractor leftHandGrab, HandGrabInteractor rightHandGrab
        ,Transform cannonParent, GameObject cannonPrefab)
    {
        this.player = player;
        this.leftHand = leftHand;
        this.rightHand = rightHand;
        this.leftHandGrab = leftHandGrab;
        this.rightHandGrab = rightHandGrab;
        this.cannonParent = cannonParent;
        this.cannonPrefab = cannonPrefab;
    }

    protected virtual void Awake()
    {
        onInitializeAsyncSubject.AddTo(this);
        onClearAsyncSubject.AddTo(this);
        envRoot = this.transform;
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 砲台の初期化
    /// </summary>
    /// <param name="reset"></param>
    protected void InitializeCannon(bool reset = true)
    {
        Vector3 cannonParentPosition = GetPlayerForwardPosition(0.8f, cannonParent.gameObject.transform.position.y);
        Vector3 cannonParentRotation = new(0, player.eulerAngles.y, 0);
        cannonParent.SetPositionAndRotation(cannonParentPosition, Quaternion.Euler(cannonParentRotation));

        if (reset)
        {
            ResetCannon(cannon.transform.position, Quaternion.Euler(cannonParentRotation));
        }

        ConfigureCannon();
        cannonParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// プレイヤーの前方座標を取得する
    /// </summary>
    /// <param name="forwadOffset"></param>
    /// <param name="yOffset"></param>
    /// <returns></returns>
    protected Vector3 GetPlayerForwardPosition(float forwadOffset ,float yOffset)
    {
        return new Vector3(player.position.x, yOffset, player.position.z) +
            (new Vector3(player.forward.x, 0, player.forward.z).normalized) * forwadOffset;
    }

    protected void ConfigureCannon()
    {
        GunController gunController = cannon.GetComponent<GunController>();
        InputEventProviderGrabbable inputEventProvider = cannon.GetComponent<InputEventProviderGrabbable>();
        CannonMultiMove cannonMultiMove = cannon.GetComponent<CannonMultiMove>();

        gunController.magazineCartridge = magazineCartridge;
        magazineCartridge.muzzle = gunController.muzzle;
        cannonMultiMove.TurretOffset = new(cannonMultiMove.transform.position.x, 0, cannonMultiMove.transform.position.z);
        inputEventProvider.leftHandInteractor = leftHandGrab;
        inputEventProvider.rightHandInteractor = rightHandGrab;
        cannonMultiMove.leftHandAnchor = leftHand.transform;
        cannonMultiMove.rightHandAnchor = rightHand.transform;
    }

    /// <summary>
    /// 砲台のリセット
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    protected void ResetCannon(Vector3 position, Quaternion rotation)
    {
        Destroy(cannon);
        cannon = Instantiate(cannonPrefab, position, rotation, cannonParent.gameObject.transform);
    }

    public void ResetCannon()
    {
        ResetCannon(cannon.transform.position, cannon.transform.rotation);
        ConfigureCannon();
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

    /// <summary>
    /// 部屋の初期化
    /// </summary>
    public abstract void InitializRoom();

    /// <summary>
    /// 部屋の開始
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public abstract UniTask StartRoom(CancellationToken token);

    /// <summary>
    /// 部屋の終了
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public abstract UniTask<bool> EndRoom(CancellationToken token);
}
