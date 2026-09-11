using UnityEngine;

public class Test2 : MonoBehaviour
{
    public GameObject text;

    private float startTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name);
        if (other.CompareTag("Player"))
        {
            text.SetActive(true);
            startTime = Time.time;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) text.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (text.active && other.CompareTag("Player") && Time.time - startTime > 2f) text.SetActive(false);
    }
}