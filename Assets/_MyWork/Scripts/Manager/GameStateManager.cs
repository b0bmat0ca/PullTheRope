using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CommonUtility))]
public class GameStateManager : MonoBehaviour
{
    public IReadOnlyReactiveProperty<GameState> State => gameState;
    private ReactiveProperty<GameState> gameState = new(GameState.Loading); // ゲームの進行状態

    private void Awake()
    {
        gameState.AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
