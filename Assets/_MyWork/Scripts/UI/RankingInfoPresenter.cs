using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using TMPro;
using System;
using UniRx;
using System.Runtime.CompilerServices;

public class RankingInfoPresenter : MonoBehaviour
{
    public IObservable<bool> OnLodedRankingAsync => onLoadedAsyncSubject;
    private readonly AsyncSubject<bool> onLoadedAsyncSubject = new();

    [SerializeField] private TextMeshProUGUI yourScore;
    [SerializeField] private RectTransform rankingUI;

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
        foreach (int score in sortScoreList.Take(10))
        {
            if (elementIdx != 0 && beforeScore == score)
            {
                rankingText[elementIdx].text = string.Empty;
            }
            rankingText[++elementIdx].text = score.ToString();
            beforeScore = score;
            elementIdx++;
        }

        onLoadedAsyncSubject.OnNext(true);
        onLoadedAsyncSubject.OnCompleted();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
