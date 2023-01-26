using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBoxTarget : MonoBehaviour
{
    [SerializeField] private CollisionEventSubject subject;

    [SerializeField] private GameObject boxPattern;
    [SerializeField] private Transform itemSpaawnPoint;
    [SerializeField] private GameObject[] items;


    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        subject.OnCollisionEnterAsync
            .Subscribe(_ =>
            {
                boxPattern.SetActive(false);
                Instantiate(items[Random.Range(0, items.Length -1)], itemSpaawnPoint.position, Quaternion.identity);
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
