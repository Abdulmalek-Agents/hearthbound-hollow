using DistantLands.Zephyr;
using UnityEngine;

public class WindmillBladesExample : MonoBehaviour
{

    public float rotationSpeed;
    public float windIntensity;
    public Vector3 rotationDirection;
    ZephyrWind wind;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wind = ZephyrWind.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        float dotProduct = Mathf.Abs(Vector3.Dot(transform.forward, wind.Direction));

        transform.eulerAngles += rotationDirection * (rotationSpeed * windIntensity * wind.WindStrength * dotProduct * Time.deltaTime);
    }
}
