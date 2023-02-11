using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using Cysharp.Threading.Tasks;
using UniRx;
using DG.Tweening;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Threading;
using DamageNumbersPro;

public class EntranceRoom : PassthroughRoom
{
    [SerializeField] private GameObject randomBox;
    [SerializeField] private GameObject titleText;
    [SerializeField] private float titleDistance = 10f;
    [SerializeField] private EnableDestroyTarget target;

    [SerializeField] private InputEventProviderGrabbable inputEventProvider;

    [SerializeField] private GameObject guideDialog;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool leftPinch = false;
    private bool rightPinch = false;

    [SerializeField] private DamageNumber okTextPrefab;
    private bool triggerGrabbed = false;
    private bool turretGrabbed = false;

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
                    SetSceneAnchorClassification(OVRSceneManager.Classification.DoorFrame, sceneAnchor);
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

        // 砲塔の取得と非表示化
        cannon = cannonRoot.transform.GetComponentInChildren<CannonMultiMove>().gameObject;
        cannonRoot.SetActive(false);

        // 初期化完了通知
        onInitializeAsyncSubject.OnNext(true);
        onInitializeAsyncSubject.OnCompleted();
    }

    public override async UniTask StartRoom(CancellationToken token)
    {
        // 動画の再生開始を待つ
        await UniTask.WaitUntil(() => videoPlayer.isPlaying, cancellationToken: token);

        // Center Eye Anchorが準備できるのを待つ
        await UniTask.WaitUntil(() => player.position != Vector3.zero, cancellationToken: token);
        guideDialog.transform.SetPositionAndRotation(GetPlayerForwardPosition(0.8f, 1f),
            Quaternion.Euler(new(guideDialog.transform.rotation.eulerAngles.x, player.eulerAngles.y, 0)));        
    }


    public override async UniTask<bool> EndRoom(CancellationToken token)
    {
        // ターゲットオブジェクトが削除されたタイミング
        await UniTask.WaitUntil(() => target == null, cancellationToken: token);

        return true;
    }
    #endregion

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

    protected override void Awake()
    {
        base.Awake();
        randomBox.SetActive(false);
        titleText.SetActive(false);

        // 初めてトリガーを握ったイベントを取得
        inputEventProvider.IsTriggerGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                audioSource.PlayOneShot(GetSE("OKSE"));
                okTextPrefab.Spawn(cannon.transform.position - new Vector3(0, 0.5f, 0), "OK");
                triggerGrabbed = true;
                if (turretGrabbed)
                {
                    MoveTarget();
                }
            }).AddTo(this);

        // 初めて砲塔を握ったイベントを取得
        inputEventProvider.IsTurretGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                audioSource.PlayOneShot(GetSE("OKSE"));
                okTextPrefab.Spawn(cannon.transform.position - new Vector3(0, 0.5f, 0), "OK");
                turretGrabbed = true;
                if (triggerGrabbed)
                {
                    MoveTarget();
                }
            }).AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckPinch checkPinch = guideDialog.GetComponent<CheckPinch>();

        checkPinch.OnLeftCheckAsync
            .Subscribe(_ =>
            {
                leftPinch = true;
                if (rightPinch)
                {
                    EnableRandomBox(GetPlayerForwardPosition(0.8f, 1.6f), Quaternion.identity).Forget();
                }
            }).AddTo(this);

        checkPinch.OnRightCheckAsync
            .Subscribe(_ =>
            {
                rightPinch = true;
                if (leftPinch)
                {
                    EnableRandomBox(GetPlayerForwardPosition(0.8f, 1.6f), Quaternion.identity).Forget();
                }
            }).AddTo(this);


        // ターゲットが破壊されたかを購読
        target.OnDestroyAsync
            .Where(x => x)
            .Subscribe(_ =>
            {
                onClearAsyncSubject.OnNext(true);
                onClearAsyncSubject.OnCompleted();
            });
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        //デバック用コード
        if (Input.GetKeyDown(KeyCode.P))
        {
            PunchRandomBox();
        }
#endif
    }

    private async UniTaskVoid EnableRandomBox(Vector3 position, Quaternion rotation)
    {
        guideDialog.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

        randomBox.transform.SetPositionAndRotation(position, rotation);
        randomBox.SetActive(true);
    }

    /// <summary>
    /// ランダムボックスをパンチした際の処理
    /// </summary>
    public void PunchRandomBox()
    {
        DisablePassthrough().Forget();
    }

    /// <summary>
    /// ターゲットを砲台の近くに移動
    /// </summary>
    private void MoveTarget()
    {
        audioSource.PlayOneShot(GetSE("MoveTarget"));
        target.transform.DOMove(cannon.transform.position + cannon.transform.forward, 1f).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// パススルー表示を終了
    /// </summary>
    /// <returns></returns>
    private async UniTask DisablePassthrough()
    {
        randomBox.GetComponent<RandomBoxTarget>().DestroyBox();

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
            await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        Vector3 titleTextPosition = new Vector3(player.forward.x, 0, player.forward.z).normalized
            * titleDistance + new Vector3(0, titleText.transform.position.y, 0);
        titleText.transform.position = titleTextPosition;
        titleText.transform.LookAt(new Vector3(player.position.x, titleText.transform.position.y, player.position.z));
        titleText.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

        // 砲塔の初期化
        InitializeCannon(false);
    }
}
