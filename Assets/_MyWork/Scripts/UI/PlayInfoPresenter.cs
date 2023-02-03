using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UniRx;
using UnityEngine.SocialPlatforms.Impl;

public class PlayInfoPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI score;

    private StageModel model;

    // Start is called before the first frame update
    void Start()
    {
        model = GameStateManager.Instance.model;

        model.Time
            .Subscribe(x => time.text = x.ToString())
            .AddTo(this);

        model.Score
            .Subscribe(x => score.text = x.ToString())
            .AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
