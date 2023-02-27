using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DamageNumbersPro;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

public class EntranceRoom : PassthroughRoom
{
    [SerializeField] private InputEventProviderGrabbable inputEventProvider;

    [Header("地面"), SerializeField] private GameObject stageGround;
    [Header("スカイボックス"), SerializeField] private GameObject skySphere;

    [Header("タイトル"), SerializeField] private GameObject titleText;
    [SerializeField] private float titleDistance = 10f;
    [SerializeField] private EnableDestroyTarget target;

    [Header("案内柱"), SerializeField] private GameObject information;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CheckPinch checkPinch;
    private bool leftPinch = false;
    private bool rightPinch = false;

    [Header("はてなボックス"), SerializeField] private GameObject randomBox;

    [Header("操作説明タブレット"), SerializeField] private GameObject howToTablet;

    [Header("OK演出"), SerializeField] private DamageNumber okTextPrefab;
    private bool triggerGrabbed = false;
    private bool turretGrabbed = false;

    private readonly AsyncSubject<bool> onPunchRandomBoxAsyncSubject = new();

    #region PassthroughRoom
    public override void InitializRoom()
    {
        OVRSceneAnchor[] sceneAnchors = FindObjectsOfType<OVRSceneAnchor>();
        OVRSceneAnchor floorAnchor = null;

        if (sceneAnchors != null)
        {
            foreach (OVRSceneAnchor sceneAnchor in sceneAnchors)
            {
                OVRSemanticClassification classification = sceneAnchor.GetComponent<OVRSemanticClassification>();

                if (classification.Contains(OVRSceneManager.Classification.Ceiling))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.Ceiling, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.DoorFrame))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.DoorFrame, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.WallFace))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.WallFace, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.WindowFrame))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.WindowFrame, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Desk))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.Desk, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Couch))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.Couch, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Other))
                {
                    SetSceneAnchorClassification(OVRSceneManager.Classification.Other, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Floor))
                {
                    floorAnchor = sceneAnchor;

                    if (envRoot)
                    {
                        Vector3 envPos = envRoot.position;
                        float groundHeight = sceneAnchor.transform.position.y - groundDelta;
                        envRoot.position = new Vector3(envPos.x, groundHeight, envPos.z);

                        if (OVRPlugin.GetSpaceBoundary2D(sceneAnchor.Space, out Vector2[] boundary))
                        {
                            cornerPoints = boundary.ToList()
                                .ConvertAll<Vector3>(corner => new Vector3(-corner.x, corner.y, 0.0f));

                            cornerPoints.Reverse();
                            for (int i = 0; i < cornerPoints.Count; i++)
                            {
                                cornerPoints[i] = sceneAnchor.transform.TransformPoint(cornerPoints[i]);
                            }
                        }
                    }
                    SetSceneAnchorClassification(OVRSceneManager.Classification.Floor, sceneAnchor);
                }
            }
        }
        CullForegroundObjects();

        // Levelの非表示
        stageGround.SetActive(false);
        skySphere.SetActive(false);

        // 砲塔の取得
        cannon = cannonParent.GetComponentInChildren<CannonMultiMove>(true).gameObject;

        // 初期化完了通知
        onInitializeAsyncSubject.OnNext(true);
        onInitializeAsyncSubject.OnCompleted();
    }

    public override async UniTask StartRoom()
    {
        CancellationToken token = tokenSource.Token;

        // 動画の再生開始を待つ
        await UniTask.WaitUntil(() => videoPlayer.isPlaying, cancellationToken: token);

        initialPlayerPosition = new Vector3(player.position.x, 0, player.position.z);
        initialPlayerDirection = new Vector3(player.forward.x, 0, player.forward.z).normalized;

        information.transform.SetPositionAndRotation(GetPlayerForwardPosition(1.5f, 0),
            Quaternion.Euler(new(0, player.eulerAngles.y, 0)));

        // 初期化状況の調整時間
        await UniTask.Delay(TimeSpan.FromSeconds(2),cancellationToken: token);

        // 初期フェードを切る
        CommonUtility.Instance.ExitExplicitFade();
    }

    public override async UniTask<bool> EndRoom()
    {
        // ターゲットオブジェクトが削除されたタイミング
        await UniTask.WaitUntil(() => target == null, cancellationToken: tokenSource.Token);

        return true;
    }
    #endregion


    protected override void Awake()
    {
        base.Awake();
        onPunchRandomBoxAsyncSubject.AddTo(this);
    }

    private void OnEnable()
    {
        randomBox.SetActive(false);
        titleText.SetActive(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // 操作説明タブレットを非表示
        howToTablet.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        // 左手の掴むポーズイベントを購読
        checkPinch.OnLeftCheckAsync
            .Subscribe(async _ =>
            {
                leftPinch = true;
                if (rightPinch)
                {
                    await EnableDoorFrame();
                }
            }).AddTo(this);

        // 右手の掴むポーズイベントを購読
        checkPinch.OnRightCheckAsync
            .Subscribe(async _ =>
            {
                rightPinch = true;
                if (leftPinch)
                {
                    await EnableDoorFrame();
                }
            }).AddTo(this);

        // ランダムボックスをパンチしたイベントを購読
        onPunchRandomBoxAsyncSubject
            .Subscribe(_ =>
            {
                // 砲塔の初期化
                InitializeCannon(false);

                SEPlay(GetSE("HowTo"));

                // ランダムボックスの破壊
                randomBox.GetComponent<RandomBoxTarget>().DestroyBox();
            }).AddTo(this);

        // 初めてトリガーを握ったイベントを購読
        inputEventProvider.IsTriggerGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                SEPlay(GetSE("OKSE"));
                okTextPrefab.Spawn(cannon.transform.position - new Vector3(0, 0.5f, 0), "OK");
                triggerGrabbed = true;
                if (turretGrabbed)
                {
                    MoveTarget();
                }
            }).AddTo(this);

        // 初めて砲塔を握ったイベントを購読
        inputEventProvider.IsTurretGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                SEPlay(GetSE("OKSE"));
                okTextPrefab.Spawn(cannon.transform.position - new Vector3(0, 0.5f, 0), "OK");
                turretGrabbed = true;
                if (triggerGrabbed)
                {
                    MoveTarget();
                }
            }).AddTo(this);

        // ターゲットが破壊されたかを購読
        target.OnDestroyAsync
            .Where(x => x)
            .Subscribe(_ =>
            {
                BGMPlay(true);

                onClearAsyncSubject.OnNext(true);
                onClearAsyncSubject.OnCompleted();
            });
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// OVRSceneAnchorオブジェクトを保持し、初期表示設定を行う
    /// </summary>
    /// <param name="classificationName"></param>
    /// <param name="sceneAnchor"></param>
    /// <param name="activeself"></param>
    private void SetSceneAnchorClassification(string classificationName, OVRSceneAnchor sceneAnchor, bool activeself = true)
    {
        SceneAnchorClassification sceneAnchorClassification;

        sceneAnchorClassification = GetSceneAnchorClassification(classificationName);
        if (sceneAnchorClassification == null)
        {
            sceneAnchorClassification = new(classificationName, new());
            sceneAnchorClassifications.Add(sceneAnchorClassification);
        }
        sceneAnchorClassification.anchors.Add(sceneAnchor);

        sceneAnchor.gameObject.SetActive(activeself);   // 初期表示設定
    }

    private void EnableRandomBox(Vector3 position, Quaternion rotation)
    {
        randomBox.transform.SetPositionAndRotation(position, rotation);
        randomBox.SetActive(true);
    }

    /// <summary>
    /// はてなボックスをパンチした際の処理
    /// </summary>
    public void PunchRandomBox()
    {
        onPunchRandomBoxAsyncSubject.OnNext(true);
        onPunchRandomBoxAsyncSubject.OnCompleted();
    }

    /// <summary>
    /// ターゲットを砲台の近くに移動
    /// </summary>
    private void MoveTarget()
    {
        SEPlay(GetSE("MoveTarget"));
        target.transform.DOMove(cannon.transform.position + cannon.transform.forward * 3, 1f).SetEase(Ease.InOutSine);
    }


    /// <summary>
    /// Virtual世界へのドアを表示
    /// </summary>
    private async UniTask EnableDoorFrame()
    {
        AudioClip orderComming = GetSE("OrderComming");
        List<OVRSceneAnchor> doorFrames = GetSceneAnchorClassification(OVRSceneManager.Classification.DoorFrame).anchors;

        information.SetActive(false);

        foreach (OVRSceneAnchor doorFrame in doorFrames)
        {
            doorFrame.gameObject.SetActive(true);
            doorFrame.GetComponent<VirtualDoorMove>().OnDoorOpen
                .Where(x => x)
                .Subscribe(async _ =>
                {
                    CancellationToken token = tokenSource.Token;
                    // Levelの表示
                    stageGround.SetActive(true);
                    skySphere.SetActive(true);

                    await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);
                    await DisablePassthrough(token);
                }).AddTo(this);
        }

        // 案内音声再生
        SEPlay(orderComming);
        await UniTask.Delay(TimeSpan.FromSeconds(orderComming.length), cancellationToken: tokenSource.Token);
        SEPlay(GetSE("GotoStage"));
    }

    /// <summary>
    /// パススルー表示を終了
    /// </summary>
    /// <returns></returns>
    private async UniTask DisablePassthrough(CancellationToken token)
    {
        ParticleSystem toVirtual = GetParticle("ToVirtual");

        CommonUtility.Instance.FadeOut();   // 現実世界で徐々にフェードアウトさせるとVR世界が写り込むため、即時フェードアウトさせる

        foreach (SceneAnchorClassification sceneAnchorClassification in sceneAnchorClassifications)
        {
            if (sceneAnchorClassification.classification == OVRSceneManager.Classification.Couch ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Desk ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Other)
            {
                // 現実世界のカウチ、机、その他に設定されたものは、そのまま表示しない
                continue;
            }

            foreach (OVRSceneAnchor sceneAnchor in sceneAnchorClassification.anchors)
            {
                sceneAnchor.gameObject.SetActive(false);    // 現実世界を見えなくする
            }
        }

        // タイトルをプレイヤーの後ろ方向に表示する
        Vector3 titleTextPosition = new Vector3(-player.forward.x, 0, -player.forward.z).normalized
            * titleDistance + new Vector3(0, titleText.transform.position.y, 0);
        titleText.transform.position = titleTextPosition;
        titleText.transform.LookAt(new Vector3(player.position.x, titleText.transform.position.y, player.position.z));
        titleText.SetActive(true);

        toVirtual.gameObject.transform.position = new Vector3(player.position.x, 0, player.position.z);
        toVirtual.gameObject.SetActive(true);

        // はてなボックスをプレイヤーの初期位置前方に表示する
        EnableRandomBox(GetPlayerInitialForwardPosition(1f, 1.8f), Quaternion.identity);
        await CommonUtility.Instance.FadeIn(this.GetCancellationTokenOnDestroy());
        await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
        BGMPlay();
    }
}
