using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CreditTitle : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject subTitlePrefab;
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private List<CreditContent> contentList;
    [SerializeField] private double fadeInTime = 0.1;

    [Serializable]
    private class CreditContent
    {
        public string subTitle;
        public List<string> content;
    }

    // Start is called before the first frame update
    async void Start()
    {
        await DisplayCredit(this.GetCancellationTokenOnDestroy());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async UniTask<bool> DisplayCredit(CancellationToken token)
    {
        bool developSection = false;

        _ = title.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime * 2), cancellationToken: token);

        foreach (CreditContent contentItem in contentList)
        {
            GameObject subTitle = Instantiate(contentPrefab, this.transform);
            subTitle.GetComponent<TextMeshProUGUI>().text = contentItem.subTitle;

            if (contentItem != contentList.Last())
            {
                _ = subTitle.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear);
            }
            else
            {
                developSection = true;
                _ = subTitle.GetComponent<RectTransform>().DOAnchorPos(new(0, -400), 5).SetEase(Ease.Linear);
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);

            foreach (string content in contentItem.content)
            {
                GameObject obj = Instantiate(contentPrefab, this.transform);
                obj.GetComponent<TextMeshProUGUI>().text = content;

                if (!developSection)
                {
                    _ = obj.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear);

                    await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);
                }
                else
                {
                    _ = obj.GetComponent<RectTransform>().DOAnchorPos(new(0, -500), 4).SetEase(Ease.Linear);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);
        }

        return true;
    }
}
