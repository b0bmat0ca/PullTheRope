using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

[RequireComponent(typeof(AudioSource))]
public class Target : MonoBehaviour
{
    [SerializeField] protected Collider colider;
    [SerializeField] protected RayfireRigid rayFireRigid;
    [SerializeField] protected float destroyTime = 5.0f;

    protected MeshRenderer[] shatterMesh;   // RayFire Shutter で事前に粉砕されたオブジェクトの配列

    private Camera mainCamera;
    private AudioSource audioSource;

    protected virtual void Awake()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        shatterMesh = rayFireRigid.gameObject.GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        transform.LookAt(mainCamera.transform);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // 粉砕する
            rayFireRigid.Demolish();

            // コライダーを無効化する
            colider.enabled = false;

            // 粉砕後、{destroyTime}秒後にDestroyする
            foreach (MeshRenderer mesh in shatterMesh)
            {
                Destroy(mesh.gameObject, destroyTime);
            }
            Destroy(gameObject, destroyTime);
        }
    }
}
