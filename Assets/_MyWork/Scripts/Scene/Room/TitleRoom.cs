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

                if (classification.Contains(OVRSceneManager.Classification.Ceiling))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.Ceiling, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.DoorFrame))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.DoorFrame, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.WallFace))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.WallFace, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.WindowFrame))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.WindowFrame, sceneAnchor);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Desk))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.Desk, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Couch))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.Couch, sceneAnchor, false);
                }
                else if (classification.Contains(OVRSceneManager.Classification.Other))
                {
                    SetSceneAnchor(OVRSceneManager.Classification.Other, sceneAnchor, false);
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
                    SetSceneAnchor(OVRSceneManager.Classification.Floor, sceneAnchor);
                }
            }
        }
        CullForegroundObjects();

        cannonBase.SetActive(false);
    }

    public override async UniTask StartRoom(float fadeTime)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    public override async UniTask EndRoom()
    {
        targetHand.SetActive(false);
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());
    }
    #endregion

    /// <summary>
    /// OVRSceneAnchorオブジェクトを保持しておく
    /// </summary>
    /// <param name="classificationName"></param>
    /// <param name="sceneAnchor"></param>
    /// <param name="activeself"></param>
    private void SetSceneAnchor(string classificationName, OVRSceneAnchor sceneAnchor, bool activeself = true)
    {
        SceneAnchormap map = new(classificationName, sceneAnchor);
        sceneAnchormap.Add(map);
        sceneAnchor.gameObject.SetActive(activeself);
    }

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
                onClearAsyncSubject.OnNext(true);
                onClearAsyncSubject.OnCompleted();
            });

        Vector3 randomBoxPosition = CannonPosition(randomBox.transform.position.y);
        Vector3 randomBoxRotation = new(0, player.eulerAngles.y, 0);

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
        DisablePassthrough().Forget();
    }

    /// <summary>
    /// ターゲットのガイドハンドを表示
    /// </summary>
    private void DisplayTargetHand()
    {
        targetHand.SetActive(true);
    }

    /// <summary>
    /// パススルー表示を終了
    /// </summary>
    /// <returns></returns>
    private async UniTask DisablePassthrough()
    {
        randomBox.GetComponent<RandomBoxTarget>().DestroyBox();

        foreach (SceneAnchormap map in sceneAnchormap)
        {
            if (map.name == OVRSceneManager.Classification.Couch ||
                map.name == OVRSceneManager.Classification.Desk || 
                map.name == OVRSceneManager.Classification.Other)
            {
                // 現実世界のカウチ、机、その他に設定されたものは、そのまま表示しない
                continue;
            }
            map.anchor.gameObject.SetActive(false); // 現実世界を見えなくする
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        Vector3 titleTextPosition = new Vector3(player.forward.x, 0, player.forward.z).normalized
            * titleDistance + new Vector3(0, titleText.transform.position.y, 0);
        titleText.transform.position = titleTextPosition;
        titleText.transform.LookAt(new Vector3(player.position.x, titleText.transform.position.y, player.position.z));

        InitializeCannon(false);
        titleText.SetActive(true);
    }
}
