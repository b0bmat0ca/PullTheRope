using UnityEngine;

public class CannonMove : MonoBehaviour
{
    [SerializeField] private GameObject cannonRoot;

    private const float MAX_Y_POSITION = 0.6f;
    private const float MIN_Y_POSITION = -0.6f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPokeUpButton()
    {
        if (cannonRoot.transform.position.y <= MAX_Y_POSITION)
        {
            cannonRoot.transform.Translate(new(0, MAX_Y_POSITION / 60, 0));
        }
    }

    public void OnPokeDownButton() 
    {
        if (cannonRoot.transform.position.y >= MIN_Y_POSITION)
        {
            cannonRoot.transform.Translate(new(0, MIN_Y_POSITION / 60, 0));
        }
    }
}
