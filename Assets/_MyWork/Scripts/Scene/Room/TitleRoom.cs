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

public class TitleRoom : PassthroughRoom
{
    [SerializeField] private GameObject randomBox;
    [SerializeField] private GameObject titleText;
    [SerializeField] private float titleDistance = 10f;
    [SerializeField] private EnableDestroyTarget target;
    [SerializeField] private GameObject targetHand;
    [SerializeField] private InputEventProviderGrabbable inputEventProvider;

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

                if (classification.Contains(OVRSceneManager.Classification.Ceiling) ||
                    classification.Contains(OVRSceneManager.Classification.DoorFrame) ||
                    classification.Contains(OVRSceneManager.Classification.WallFace) ||
                    classification.Contains(OVRSceneManager.Classification.WindowFrame))
                {
                    sceneAnchor.gameObject.SetActive(false); // Passthrough で現実世界が「見えなくなる」

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
                    sceneAnchor.gameObject.SetActive(false);    // Passthrough で現実世界が「見えなくなる」
                }
                else if (classification.Contains(OVRSceneManager.Classification.Desk) ||
                         classification.Contains(OVRSceneManager.Classification.Other) ||
                         classification.Contains(OVRSceneManager.Classification.Couch))
                {
                    sceneAnchor.gameObject.SetActive(false);
                }
            }
        }
        CullForegroundObjects();

        cannonBase.SetActive(false);
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        randomBox.SetActive(false);
        targetHand.SetActive(false);
        titleText.SetActive(false);

        // 初めてトリガーを握ったイベントを取得
        inputEventProvider.IsTriggerGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                triggerGrabbed = true;
                if (turretGrabbed)
                {
                    DisplayTargetHand();
                }
            }).AddTo(this);

        // 初めて砲塔を握ったイベントを取得
        inputEventProvider.IsTurretGrab
            .Where(x => x)
            .First()
            .Subscribe(_ =>
            {
                turretGrabbed = true;
                if (triggerGrabbed)
                {
                    DisplayTargetHand();
                }
            }).AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // ターゲットが破壊されたかを購読
        target.OnDestroyAsync
            .Where(x => x)
            .Subscribe(_ =>
            {
                targetHand.SetActive(false);
                onClearAsyncSubject.OnNext(true);
                onClearAsyncSubject.OnCompleted();
            });

        Vector3 randomBoxPosition = CannonPosition(randomBox.transform.position.y);
        Vector3 randomBoxRotation = new(0, mainCamera.transform.eulerAngles.y, 0);

        randomBox.transform.SetPositionAndRotation(randomBoxPosition, Quaternion.Euler(randomBoxRotation));
        randomBox.SetActive(true);
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

    /// <summary>
    /// ランダムボックスをパンチした際の処理
    /// </summary>
    public void PunchRandomBox()
    {
        DisplayTitleAndCannon().Forget();
    }

    /// <summary>
    /// ターゲットのガイドハンドを表示
    /// </summary>
    private void DisplayTargetHand()
    {
        targetHand.SetActive(true);
    }

    /// <summary>
    /// タイトルと砲台を表示
    /// </summary>
    /// <returns></returns>
    private async UniTask DisplayTitleAndCannon()
    {
        randomBox.GetComponent<RandomBoxTarget>().DestroyBox();
        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());

        Vector3 titleTextPosition = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z).normalized
            * titleDistance + new Vector3(0, titleText.transform.position.y, 0);
        titleText.transform.position = titleTextPosition;
        titleText.transform.LookAt(new Vector3(mainCamera.transform.position.x, titleText.transform.position.y, mainCamera.transform.position.z));

        InitializeCannon(false);
        titleText.SetActive(true);
    }
}
