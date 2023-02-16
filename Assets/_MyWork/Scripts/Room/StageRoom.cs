﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UniRx;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Cysharp.Threading.Tasks.CompilerServices;
using System.Threading;
using MText;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.AddressableAssets;

public class StageRoom : PassthroughRoom
{
    [Header("HUDキャンバス"), SerializeField] private GameObject hudCanvas;
    [Header("HUDテキスト"), SerializeField] private TextMeshProUGUI hudText;
    [Header("制限時間"), SerializeField] private int time = 60;
    [Header("制限時間、スコアを表示するUI"), SerializeField] private GameObject scoreDialog;
    [Header("ターゲット生成位置"), SerializeField] private GameObject spawnPoint;

    private int stageIndex = 1;

    private float currentTime = 0;
    private StageModel model;

    private bool roomStart = false;

    [Header("ステージ用アセット")]
    [Header("SkyBox"), SerializeField] private GameObject skyBox;

    #region PassthroughRoom
    public override void  InitializRoom()
    {
        OVRSceneAnchor floorAnchor = null;

        foreach (SceneAnchorClassification sceneAnchorClassification in sceneAnchorClassifications)
        {
            if (sceneAnchorClassification.classification == OVRSceneManager.Classification.Couch ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Desk ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Other)
            {
                foreach (OVRSceneAnchor sceneAnchor in sceneAnchorClassification.anchors)
                {
                    sceneAnchor.gameObject.SetActive(true); // 可視化する
                }
            }
            else if (sceneAnchorClassification.classification == OVRSceneManager.Classification.Floor)
            {
                OVRSceneAnchor sceneAnchor = sceneAnchorClassification.anchors[0];
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
            }
        }
        CullForegroundObjects();

        // 砲塔の取得と非表示化
        cannon = cannonParent.gameObject.transform.GetComponentInChildren<CannonMultiMove>().gameObject;
        cannonParent.gameObject.SetActive(false);

        // 初期化完了通知
        onInitializeAsyncSubject.OnNext(true);
        onInitializeAsyncSubject.OnCompleted();
    }

    public override async UniTask StartRoom(CancellationToken token)
    {
        // 砲塔の初期化
        InitializeCannon();

        await CommonUtility.Instance.FadeIn(token);

        // ステージタイトル表示
        hudText.text = $"Stage {stageIndex}";
        hudCanvas.SetActive(true);
        

        // 制限時間を設定
        model.Time.Value = this.time;
        AudioClip countDown = GetSE("StartCountDown");
        SEPlay(countDown);
        await UniTask.Delay(TimeSpan.FromSeconds(countDown.length), cancellationToken: token);

        hudCanvas.SetActive(false);

        BGMPlay();

        roomStart = true;
    }

    public override async UniTask<bool> EndRoom(CancellationToken token)
    {
        ParticleSystem toReal = GetParticle("ToReal");

        // ステージ終了の演出

        // ステージエンドタイトル表示
        hudText.text = $"Stage {stageIndex} End";
        hudCanvas.SetActive(true);

        spawnPoint.SetActive(false);

        AudioClip end = GetSE("End");
        SEPlay(end);
        await UniTask.Delay(TimeSpan.FromSeconds(end.length), cancellationToken: token);

        hudCanvas.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
        cannonParent.gameObject.SetActive(false);

        toReal.gameObject.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);

        return true;
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        model = GameStateManager.Instance.model;
        hudCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        LoadAsset(1, this.GetCancellationTokenOnDestroy()).Forget();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 制限時間を購読
        model.Time
            .Skip(1)
            .Where(x => x == 0)
            .Subscribe(async  _=>
            {
                BGMPlay(true);

                if (GameStateManager.Instance.maxStageCount == stageIndex)
                {
                    onClearAsyncSubject.OnNext(true);
                    onClearAsyncSubject.OnCompleted();
                }
                else
                {
                    CancellationToken token = this.GetCancellationTokenOnDestroy();
                    await EndStage(token);
                    await CommonUtility.Instance.FadeOut(token);
                    await NextStage(token);
                }

            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (roomStart)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1)
            {
                if (model.Time.Value > 0)
                {
                    model.Time.Value -= 1;
                }
                currentTime -= 1;
            }
        }
    }

    private async UniTask NextStage(CancellationToken token)
    {
        roomStart = false;

        await LoadAsset(++stageIndex, token);
        await StartRoom(token);
    }

    private async UniTask EndStage(CancellationToken token)
    {
        // ステージエンドタイトル表示
        hudText.text = $"Stage {stageIndex} End";
        hudCanvas.SetActive(true);

        spawnPoint.SetActive(false);

        AudioClip end = GetSE("End");
        SEPlay(end);
        await UniTask.Delay(TimeSpan.FromSeconds(end.length), cancellationToken: token);

        hudCanvas.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
        cannonParent.gameObject.SetActive(false);
    }

    private async UniTask LoadAsset(int index, CancellationToken token)
    {
        // BGMの処理
        audioSources[0].clip = await Addressables.LoadAssetAsync<AudioClip>($"Stage{index}_BGM").Task;

        // SkyBoxの処理
        Material skyMaterial = await Addressables.LoadAssetAsync<Material>($"Stage{index}_SkyBox").Task;
        skyBox.GetComponent<MeshRenderer>().material = skyMaterial;

        // 弾倉の処理
        GameObject bulletPrefab = await Addressables.LoadAssetAsync<GameObject>($"Stage{index}_Bullet").Task;
        AudioClip bulletSE = await Addressables.LoadAssetAsync<AudioClip>($"Stage{index}_Bullet_SE").Task;
        magazineCartridge.ResetBullet(bulletPrefab);
        magazineCartridge.GetComponent<AudioSource>().clip = bulletSE;

        /*
        MeshFilter[] groundMeshFilter = groundPrefab.GetComponentsInChildren<MeshFilter>();
        Mesh[] groundMesh = new Mesh[3];
        for (int i = 0; i < groundMesh.Length; i++)
        {
            groundMesh[i] = await Addressables.LoadAssetAsync<Mesh>($"Stage{index}_Ground_LOD{i}").Task;
        }
        for (int i = 0; i < groundMeshFilter.Length; i++)
        {
            groundMeshFilter[i].mesh = groundMesh[i];
        }
        */
    }
}