using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CommonUtility))]
public class GameStateManager : MonoBehaviour
{
    public IReadOnlyReactiveProperty<GameState> State => gameState;
    private ReactiveProperty<GameState> gameState = new(GameState.Loading); // ゲームの進行状態

    [SerializeField] private GunController gunController;

    private void Awake()
    {
        gameState.AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // 最初にトリガーを触ったタイミングでゲームスタート
        gunController.OnFirstTriggerGrabAsync
            .Subscribe(_ => gameState.Value = GameState.Start)
            .AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
