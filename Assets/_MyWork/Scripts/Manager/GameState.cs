/// <summary>
/// ゲーム状態
/// </summary>
public enum GameState
{
    Loading, // 準備中
    Start,  // 開始
    Playing,    // プレイ中
    End, // 次のシーンへ、ゲームクリア
    Restart // やり直し
}
