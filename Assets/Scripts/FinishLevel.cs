using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Levels/ FinishLevel")]
public class FinishLevel : MonoBehaviour
{
    [Header("Scene's index")]
    public int sceneIndex;

    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 1f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Вращение объекта
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Левитация объекта
        Vector3 floatPosition = startPosition + new Vector3(0f, Mathf.Sin(Time.time * floatSpeed) * floatHeight, 0f);
        transform.position = floatPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ("Player"))
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
    
