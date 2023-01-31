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

[RequireComponent(typeof(CommonUtility))]
public class GameStateManager : MonoBehaviour
{
    public static int CurrentRoomIndex { get; private set; } = 0;

    [SerializeField] private OVRSceneManager sceneManager;

    public IReadOnlyReactiveProperty<GameState> State => gameState;
    private ReactiveProperty<GameState> gameState = new(GameState.Loading); // ゲームの進行状態

    public MeshRenderer fadeSphere;

    [SerializeField] private OVRHand leftHand;
    [SerializeField] private OVRHand rightHand;
    [SerializeField] private HandGrabInteractor leftHandGrab;
    [SerializeField] private HandGrabInteractor rightHandGrab;

    [Header("砲台の親オブジェクト"), SerializeField] private Transform cannonParent;
    [SerializeField] private GameObject cannonPrefab;

    // ルームリスト
    [SerializeField] private List<Room> roomList;
    [System.Serializable]
    private class Room
    {
        [SerializeField] private string roomName;
        [SerializeField] private PassthroughRoom room;

        public string RoomName { get { return roomName; } }
        public PassthroughRoom Instance { get { return room; } }
    }
    private PassthroughRoom currentRoom;

    private void Awake()
    {
        gameState.AddTo(this);

        fadeSphere.gameObject.SetActive(true);
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
    }

    // Start is called before the first frame update
    async void Start()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID
        OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;
#endif
        // ゲームの進行状態を購読する
        gameState.Subscribe(_ => OnChangeState()).AddTo(this);

        // ルームの開始
        currentRoom = roomList[CurrentRoomIndex].Instance;
        currentRoom.gameObject.SetActive(true);
        currentRoom.Initialize(leftHand, rightHand, leftHandGrab, rightHandGrab, cannonParent, cannonPrefab, 0.25f);
        sceneManager.LoadSceneModel();
        sceneManager.SceneModelLoadedSuccessfully += currentRoom.InitializRoom;
        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());
        fadeSphere.gameObject.SetActive(false);

        // 開始状態に設定
        gameState.Value = GameState.Start;

        currentRoom.OnClearAsync
            .Where(x => x)
            .Subscribe(_ =>
            {
                gameState.Value = GameState.End;
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnChangeState()
    {

        if (gameState.Value == GameState.Start)
        {

        }
        else if (gameState.Value == GameState.Playing)
        {

        }
        else if (gameState.Value == GameState.End)
        {
            fadeSphere.gameObject.SetActive(true);
            fadeSphere.sharedMaterial.SetColor("_Color", Color.black);

            currentRoom.gameObject.SetActive(false);
            CommonUtility.Instance.TransitionScene(++CurrentRoomIndex);
        }
    }

}
