using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnBoxTarget : MonoBehaviour
{
    [SerializeField] private float minSpawnTime = 5;
    [SerializeField] private float maxSpawnTime = 15;

    // ターゲットリスト
    [SerializeField] protected List<TargetMap> targetList;
    [System.Serializable]
    protected class TargetMap
    {
        [SerializeField] private GameObject target;
        [SerializeField] private float offsetY;
        [SerializeField] private float targetScale;

        public GameObject Target { get { return target; } }
        public float OffsetY { get { return offsetY; } }
        public float TargetScale { get { return targetScale;} }
    }


    // Start is called before the first frame update
    void Start()
    {
        SpawnTarget().Forget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async  UniTaskVoid SpawnTarget()
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(minSpawnTime, maxSpawnTime)), cancellationToken: this.GetCancellationTokenOnDestroy());
            TargetMap target = targetList[Random.Range(0, targetList.Count)];
            GameObject obj = Instantiate(target.Target, transform.position + new Vector3(0, target.OffsetY, 0), Quaternion.identity);
            obj.SetActive(false);
            obj.transform.localScale *= target.TargetScale;
            obj.transform.SetParent(gameObject.transform);
            obj.SetActive(true);
            await UniTask.WaitUntil(() => obj == null, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
