using System.Threading;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using UniRx;
using UnityEngine;
using System.Collections;
using Oculus.Interaction;
using Oculus.Interaction.DistanceReticles;
using System.Collections.Generic;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using Oculus.Platform.Models;
using TMPro;
using Unity.VisualScripting;

[RequireComponent(typeof(StageModel))]
[RequireComponent(typeof(CommonUtility))]
public class GameStateManager : MonoBehaviour
{
    //[SerializeField] private MeshRenderer fadeSphere;

    [SerializeField] private OVRSceneManager sceneManager;

    public static GameStateManager Instance;
    [Header("制限時間、スコアを管理するモデル")] public StageModel model;

    public IReadOnlyReactiveProperty<GameState> State => gameState;
    private ReactiveProperty<GameState> gameState = new(GameState.Loading); // ゲームの進行状態

    public static int CurrentRoomIndex { get; private set; } = 0;   // 現在のルームインデックス

    // ルームリスト
    [SerializeField] private List<Room> roomList;
    [System.Serializable]
    private class Room
    {
        [SerializeField] private string roomName;
        [SerializeField] private PassthroughRoom room;
        [SerializeField] private bool resetRoom;

        public string RoomName { get { return roomName; } }
        public PassthroughRoom Instance { get { return room; } }

        public bool ResetRoom { get { return resetRoom; } }
    }
    private PassthroughRoom currentRoom;

    [SerializeField] private Transform player;
    [SerializeField] private OVRHand leftHand;
    [SerializeField] private OVRHand rightHand;
    [SerializeField] private HandGrabInteractor leftHandGrab;
    [SerializeField] private HandGrabInteractor rightHandGrab;

    [Header("砲台の親オブジェクト"), SerializeField] private Transform cannonParent;
    [Header("砲台のプレファブ"), SerializeField] private GameObject cannonPrefab;
    
    private readonly CompositeDisposable compositeDisposable = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        gameState.AddTo(this);
        gameState.Value = GameState.Loading;
    }

    private void OnEnable()
    {
        cannonParent.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    async void Start()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID
        OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;
#endif
        // ゲームの進行状態を購読する
        gameState.Subscribe(_ => OnChangeState()).AddTo(this);

        // CenterEyeAnchorの初期化が終わるまで待つ
        await UniTask.WaitUntil(() => 
        player.position != Vector3.zero && CommonUtility.Instance != null, cancellationToken: this.GetCancellationTokenOnDestroy());

        //CommonUtility.Instance.ExitExplicitFade();

        // ルームの開始
        currentRoom = roomList[CurrentRoomIndex].Instance;
        currentRoom.gameObject.SetActive(true);
        currentRoom.OnInitializeAsync
            .Subscribe(async _ =>
            {
                await currentRoom.StartRoom(this.GetCancellationTokenOnDestroy());

                // 開始状態に設定
                gameState.Value = GameState.Start;
            }).AddTo(this);
        currentRoom.Initialize(player, leftHand, rightHand, leftHandGrab, rightHandGrab, cannonParent, cannonPrefab);
        sceneManager.SceneModelLoadedSuccessfully += currentRoom.InitializRoom;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private async void OnChangeState()
    {
        if (gameState.Value == GameState.Start)
        {
            compositeDisposable.Clear();

            currentRoom.OnClearAsync
                .Where(x => x)
                .Subscribe(_ =>
                {
                    gameState.Value = GameState.End;
                }).AddTo(compositeDisposable);

            // プレイ中に設定
            gameState.Value = GameState.Playing;
        }
        else if (gameState.Value == GameState.Playing)
        {

        }
        else if (gameState.Value == GameState.End)
        {
            bool next = await currentRoom.EndRoom(this.GetCancellationTokenOnDestroy());

            if (next)
            {
                await NextRoom();
            }
            else
            {
                CommonUtility.Instance.TransitionScene();
            }
        }
    }

    private async UniTask NextRoom()
    {
        if (roomList[++CurrentRoomIndex].ResetRoom)
        {
            await CommonUtility.Instance.FadeOut(this.GetCancellationTokenOnDestroy());
            currentRoom.gameObject.SetActive(false);
        }
        
        // 次の部屋の設定
        currentRoom = roomList[CurrentRoomIndex].Instance;
        currentRoom.gameObject.SetActive(true);
        currentRoom.OnInitializeAsync
            .Subscribe(async _ => 
            {
                await currentRoom.StartRoom(this.GetCancellationTokenOnDestroy());

                // 開始状態に設定
                gameState.Value = GameState.Start;
            }).AddTo(this);
        currentRoom.Initialize(player, leftHand, rightHand, leftHandGrab, rightHandGrab, cannonParent, cannonPrefab);
        currentRoom.InitializRoom();
    }
}
