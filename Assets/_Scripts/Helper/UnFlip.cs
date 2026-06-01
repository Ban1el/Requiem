using UnityEngine;

public class UnFlip : MonoBehaviour
{
    private Vector3 originalLocalScale;

    void Start()
    {
        originalLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // Counteract parent's X flip
        float parentScaleX = transform.parent.localScale.x;
        transform.localScale = new Vector3(
            originalLocalScale.x / parentScaleX,  // cancels out -1
            originalLocalScale.y,
            originalLocalScale.z
        );
    }
}
