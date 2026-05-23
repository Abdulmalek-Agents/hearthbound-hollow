using UnityEngine;

public class ParticleCannon : MonoBehaviour
{

    private float timer;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private GameObject shotPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = timeBetweenShots;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            Instantiate(shotPrefab, transform.position, transform.rotation);
            timer = timeBetweenShots;
        }
    }
}
