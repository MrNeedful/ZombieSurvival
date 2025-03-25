using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;
    public int quantity = 1;

    [Header("Pickup Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;
    public float rotationSpeed = 50f;
    public bool showPickupPrompt = true;

    private Vector3 startPosition;
    private float timeOffset;

    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void Update()
    {
        // Bob up and down
        float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotate
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        // Draw pickup range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    private void OnGUI()
    {
        if (!showPickupPrompt) return;

        // Show pickup prompt when player is looking at the item
        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 screenPoint = camera.WorldToScreenPoint(transform.position);
        if (screenPoint.z > 0) // Only show if item is in front of camera
        {
            float distance = Vector3.Distance(camera.transform.position, transform.position);
            if (distance <= 2f) // Only show within pickup range
            {
                string prompt = $"Press E to pick up {itemData.itemName} x{quantity}";
                GUI.Label(new Rect(screenPoint.x - 100, Screen.height - screenPoint.y - 20, 200, 20), prompt);
            }
        }
    }
} 