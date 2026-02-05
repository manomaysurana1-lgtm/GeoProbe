using UnityEngine;

public class CameraRotateAround : MonoBehaviour
{
    [Header("Wander Settings")]
    [Tooltip("How fast the camera changes direction.")]
    public float speed = 0.5f;

    [Tooltip("How far the camera can look on the X axis (Up/Down).")]
    public float xRange = 15f;

    [Tooltip("How far the camera can look on the Y axis (Left/Right).")]
    public float yRange = 45f;

    private float randomSeedX;
    private float randomSeedY;
    private Quaternion startRotation;

    void Start()
    {
        // 1. Remember where the camera was looking when the game started
        startRotation = transform.localRotation;

        // 2. Generate random starting points so it behaves differently every time
        randomSeedX = Random.Range(0f, 1000f);
        randomSeedY = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // 3. Generate smooth values that slowly change over time (Noise)
        // We use Time.time * speed to move through the noise pattern
        float noiseX = Mathf.PerlinNoise(randomSeedX, Time.time * speed);
        float noiseY = Mathf.PerlinNoise(randomSeedY, Time.time * speed);

        // 4. PerlinNoise returns 0 to 1. We map this to -1 to 1.
        float xRotation = (noiseX - 0.5f) * 2 * xRange;
        float yRotation = (noiseY - 0.5f) * 2 * yRange;

        // 5. Apply the rotation relative to the starting rotation
        // -yRotation corresponds to "Yaw" (looking left/right)
        // xRotation corresponds to "Pitch" (looking up/down)
        Quaternion wanderRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        transform.localRotation = startRotation * wanderRotation;
    }
}