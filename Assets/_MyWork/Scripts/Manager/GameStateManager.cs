using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CommonUtility))]
public abstract class GameStateManager : MonoBehaviour
{
    public ReactiveProperty<GameState> gameState = new(GameState.Start); // ゲームの進行状態

    protected CancellationTokenSource gameStateTokenSource = new();

    protected virtual void OnDestroy()
    {
        gameStateTokenSource.Cancel();
    }

    protected virtual void Awake()
    {
        gameState.AddTo(this);
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // ゲームの進行状態を購読する
        gameState.Subscribe(_ => OnChangeState()).AddTo(this);
    }

    /// <summary>
    /// GameStateが変化した場合の処理を記載
    /// </summary>
    protected abstract void OnChangeState();
}
