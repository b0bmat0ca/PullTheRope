using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

public class RankingInfoPresenter : MonoBehaviour
{
    public IObservable<bool> OnLodedRankingAsync => onLoadedAsyncSubject;
    private readonly AsyncSubject<bool> onLoadedAsyncSubject = new();

    [SerializeField] private TextMeshProUGUI yourScore;
    [SerializeField] private TextMeshProUGUI yourRank;
    [SerializeField] private RectTransform rankingUI;
    [Header("Top10入りした場合のフォント"), SerializeField] private TMP_FontAsset top10font;

    private StageModel model;

    private TextMeshProUGUI[] rankingText;

    private int playNum;
    private List<int> scoreList = new();

    private void OnDestroy()
    {
        onLoadedAsyncSubject.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        model = GameStateManager.Instance.model;
        rankingText = rankingUI.GetComponentsInChildren<TextMeshProUGUI>();
        yourRank.text = "0";

        // プレイした人の数を取得
        playNum = PlayerPrefs.GetInt("PlayNum", 0);

        // スコアを取得
        for (int i = 1; i <= playNum; i++)
        {
            scoreList.Add(PlayerPrefs.GetInt($"Player{i}", 0));
        }

        // 今回のプレイ情報を追加
        scoreList.Add(model.Score.Value);
        PlayerPrefs.SetInt("PlayNum", ++playNum);
        PlayerPrefs.SetInt($"Player{playNum}", model.Score.Value);

        // 重複スコアを弾く
        //scoreList = scoreList.Distinct().ToList();

        // スコアリストを降順にソートする
        IOrderedEnumerable<int> sortScoreList = scoreList.OrderByDescending(x => x);

        // UI表示、トップ10のランキング
        yourScore.text = model.Score.Value.ToString();
        int beforeScore = 0;
        int elementIdx = 0;
        int rank = 1;

        // トップ10表示処理
        foreach (int score in sortScoreList.Take(10))
        {
            if (yourRank.text.Equals("0") && model.Score.Value == score)
            {
                rankingText[elementIdx].font = top10font;
                rankingText[elementIdx].UpdateFontAsset();
                rankingText[elementIdx].ForceMeshUpdate();
                rankingText[elementIdx + 1].font = top10font;
                rankingText[elementIdx + 1].UpdateFontAsset();
                rankingText[elementIdx + 1].ForceMeshUpdate();
                yourRank.text = rank.ToString();
            }

            if (elementIdx != 0 && beforeScore == score)
            {
                rankingText[elementIdx].text = string.Empty;
            }
            rankingText[++elementIdx].text = score.ToString();
            beforeScore = score;
            elementIdx++;
            rank++;
        }

        // トップ10に入っていない場合の処理
        if (yourRank.text.Equals("0"))
        {
            yourRank.text = scoreList.FindIndex(x => x == model.Score.Value).ToString();
        }

        onLoadedAsyncSubject.OnNext(true);
        onLoadedAsyncSubject.OnCompleted();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
