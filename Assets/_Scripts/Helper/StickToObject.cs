using UnityEngine;

public class StickToObject : MonoBehaviour
{
    [SerializeField]
    private GameObject parentObject;
    private Canvas canvas;

    private void Update()
    {
        this.transform.position = parentObject.transform.position;
    }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.gameObject.SetActive(true);
    }
}
