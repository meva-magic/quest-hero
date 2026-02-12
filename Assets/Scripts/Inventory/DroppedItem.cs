using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DroppedItem : MonoBehaviour
{
    public bool autoStart;
    [SerializeField]
    float enabledPickupDelay = 3.0f;
    public ItemObject item;
    public bool pickedUp = false;

    void Start()
    {
        if (autoStart && item != null)
        {
            Initialize(item);
        }
    }

    public void Initialize(ItemObject item)
    {
        this.item = item;
        var droppedItem = Instantiate(item.prefab, transform);
        droppedItem.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        StartCoroutine(EnablePickup(enabledPickupDelay));
    }

    IEnumerator EnablePickup(float dealy)
    {
        yield return new WaitForSeconds(dealy);
        GetComponent<Collider>().enabled = true;
    }
}
