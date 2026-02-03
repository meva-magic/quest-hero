using UnityEngine;

public class BobItem : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    
    [Header("Randomize Settings")]
    [SerializeField] private bool randomizeOffset = true;
    [SerializeField] private bool randomizeSpeed = false; 
    
    private Vector3 startPosition;
    private float timeOffset;
    private float currentSpeed;
    
    void Start()
    {
        startPosition = transform.position;
        
        timeOffset = randomizeOffset ? Random.Range(0f, Mathf.PI * 2) : 0f;
        
        currentSpeed = randomizeSpeed ? bobSpeed * Random.Range(0.8f, 1.2f) : bobSpeed;
    }
    
    void Update()
    {
        float verticalBob = Mathf.Sin((Time.time * currentSpeed) + timeOffset) * bobHeight;

        float verticalComponent = verticalBob * 0.7f;   
        float forwardComponent = verticalBob * 0.3f;
        Vector3 bobVector = new Vector3(0, verticalComponent, forwardComponent);
        
        transform.position = startPosition + bobVector;
    }
    
    public void ResetPosition()
    {
        transform.position = startPosition;
    }
}
