using System;
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
using UnityEditor;
using UniRx.Triggers;
using DG.Tweening;

public class StageRoom : PassthroughRoom
{
    [Header("HUDキャンバス"), SerializeField] private GameObject hudCanvas;
    [Header("HUDテキスト"), SerializeField] private TextMeshProUGUI hudText;
    [Header("制限時間"), SerializeField] private int time = 60;
    [Header("制限時間、スコアを表示するUI"), SerializeField] private GameObject scoreDialog;
    [Header("ターゲット生成位置"), SerializeField] private Transform spawnParent;

    [Header("パススルー表示用マテリアル"), SerializeField] private Material passthroughMaterial;

    private float currentTime = 0;
    private StageModel model;

    private bool roomStart = false;

    // Stage情報
    private class StageInfo
    {
        public int Stage;
    }
    private int maxStageCount = 0;
    private int stageIndex = 1;

    [Header("ステージ用アセット")]
    [Header("SkyBox"), SerializeField] private GameObject skyBox;

    private Material skyMaterial;
    private GameObject bulletPrefab;
    private AudioClip bulletSE;

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

    public override async UniTask StartRoom()
    {
        CancellationToken token = tokenSource.Token;
        // 部屋開始の演出

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

    public override async UniTask<bool> EndRoom()
    {
        // 部屋終了の演出
        ParticleSystem toReal = GetParticle("ToReal");

        toReal.gameObject.transform.position = new Vector3(player.position.x, 0, player.position.z);
        toReal.gameObject.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: tokenSource.Token);

        return true;
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        model = GameStateManager.Instance.model;
        hudCanvas.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //LoadAsset(1, this.GetCancellationTokenOnDestroy()).Forget();
        ReleaseAsset(stageIndex);
    }

    // Start is called before the first frame update
   async void Start()
    {
        // ステージ数の読み込み
        TextAsset stageInfo = await Addressables.LoadAssetAsync<TextAsset>("StageInfo").Task;
        StageInfo info = JsonUtility.FromJson<StageInfo>(stageInfo.ToString());
        maxStageCount = info.Stage;
        Addressables.Release(stageInfo);

        // 制限時間を購読
        model.Time
            .Skip(1)
            .Where(x => x == 0)
            .Subscribe(async  _=>
            {
                CancellationToken token = tokenSource.Token;

                BGMPlay(true);

                if (maxStageCount == stageIndex)
                {
                    // ステージエンドタイトル表示
                    hudText.text = $"Stage {stageIndex} End";
                    hudCanvas.SetActive(true);

                    AudioClip end = GetSE("End");
                    SEPlay(end);
                    await UniTask.Delay(TimeSpan.FromSeconds(end.length), cancellationToken: token);

                    hudCanvas.SetActive(false);

                    // ターゲットの削除
                    foreach (Transform target in spawnParent)
                    {
                        Destroy(target.gameObject);
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token);

                    // ターゲットの削除(念のため)
                    foreach (Transform target in spawnParent)
                    {
                        Destroy(target.gameObject);
                    }

                    // 砲台を非表示
                    cannonParent.gameObject.SetActive(false);

                    await EnableDoorFrame();
                }
                else
                {
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

    /// <summary>
    /// 現実世界へのドアを表示
    /// </summary>
    private async UniTask EnableDoorFrame()
    {
        AudioClip endStage = GetSE("EndStage");
        List<OVRSceneAnchor> doorFrames = GetSceneAnchorClassification(OVRSceneManager.Classification.DoorFrame).anchors;
        
        foreach (OVRSceneAnchor doorFrame in doorFrames)
        {
            doorFrame.gameObject.SetActive(true);

            VirtualDoorMove doorMove = doorFrame.GetComponent<VirtualDoorMove>();
            doorMove.depthOccluder.material = passthroughMaterial;  // 現実世界表示用マテリアル
            doorMove.OnDoorOpen
                .Where(x => x)
                .Subscribe(async _ =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: tokenSource.Token);

                    onClearAsyncSubject.OnNext(true);
                    onClearAsyncSubject.OnCompleted();
                }).AddTo(this);
        }

        // 案内音声再生
        SEPlay(endStage);
        await UniTask.Delay(TimeSpan.FromSeconds(endStage.length), cancellationToken: tokenSource.Token);
        SEPlay(GetSE("GotoReal"));
    }

    /// <summary>
    /// 次のステージの呼び出し
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask NextStage(CancellationToken token)
    {
        roomStart = false;

        ReleaseAsset(stageIndex);
        await LoadAsset(++stageIndex, token);
        await StartRoom();
    }

    /// <summary>
    /// ステージの終了処理
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask EndStage(CancellationToken token)
    {
        // ステージエンドタイトル表示
        hudText.text = $"Stage {stageIndex} End";
        hudCanvas.SetActive(true);

        AudioClip end = GetSE("End");
        SEPlay(end);
        await UniTask.Delay(TimeSpan.FromSeconds(end.length), cancellationToken: token);

        hudCanvas.SetActive(false);

        // ターゲットの削除
        foreach (Transform target in spawnParent)
        {
            Destroy(target.gameObject);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);

        // ターゲットの削除(念のため)
        foreach (Transform target in spawnParent)
        {
            Destroy(target.gameObject);
        }

        cannonParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Addressablesでアセットをロード
    /// </summary>
    /// <param name="index"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async UniTask LoadAsset(int index, CancellationToken token)
    {
        // BGMの処理
        audioSources[0].clip = await Addressables.LoadAssetAsync<AudioClip>($"Stage{index}_BGM").Task;

        // SkyBoxの処理
        skyMaterial = await Addressables.LoadAssetAsync<Material>($"Stage{index}_SkyBox").Task;
        skyBox.GetComponent<MeshRenderer>().material = skyMaterial;

        // 弾倉の処理
        bulletPrefab = await Addressables.LoadAssetAsync<GameObject>($"Stage{index}_Bullet").Task;
        bulletSE = await Addressables.LoadAssetAsync<AudioClip>($"Stage{index}_Bullet_SE").Task;
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

    /// <summary>
    /// Addressablesでロードしたアセットをリリース
    /// </summary>
    /// <param name="index"></param>
    private void ReleaseAsset(int index)
    {
        // BGMの処理
        Addressables.Release(audioSources[0].clip);

        // SkyBoxの処理
        Addressables.Release(skyMaterial);

        // 弾倉の処理
        Addressables.Release(bulletPrefab);
        Addressables.Release(bulletSE);
    }
}
