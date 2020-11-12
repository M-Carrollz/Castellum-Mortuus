using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCam;
    Transform mainCamTransform;
    PlayerControl player;
    Transform playerTransform;

    Vector3 mainCamLocalOrigin = Vector3.zero;
    Vector3 desiredLocation = Vector3.zero;
    Vector3 desiredDirection = Vector3.zero;

    public float distance = 5f;
    public float speed = 10f;

    public bool interp = false;

    [Header("Shake")]

    public bool shake = false;

    [Range(0, 100)]
    public float amplitude = 1;
    [Range(0.00001f, 0.99999f)]
    public float frequency = 0.98f;

    [Range(1, 4)]
    public int octaves = 2;

    [Range(0.00001f, 5)]
    public float persistance = 0.2f;
    [Range(0.00001f, 100)]
    public float lacunarity = 20;

    [Range(0.00001f, 0.99999f)]
    public float burstFrequency = 0.5f;

    [Range(0, 5)]
    public int burstContrast = 2;

    private void Awake()
    {
        mainCam = GetComponentInChildren<Camera>();
        mainCamTransform = mainCam.gameObject.transform;
        mainCamLocalOrigin = mainCamTransform.localPosition;
        player = FindObjectOfType<PlayerControl>();
        playerTransform = player.gameObject.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = playerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(shake)
        {
            ShakeCamera();
        }
    }

    private void LateUpdate()
    {
        desiredLocation = playerTransform.position + (player.heading * distance);
        desiredDirection = desiredLocation - transform.position;

        Debug.DrawLine(transform.position, transform.position + desiredDirection, Color.blue);

        if(player.currentSpeed == 0)
        {
            return;
        }

        if (interp)
        {
            transform.position = Vector3.MoveTowards(transform.position, desiredLocation, speed * Time.deltaTime);
            //transform.position = transform.position + desiredDirection;
        }
        else
        {
            transform.position = playerTransform.position;
        }
    }

    Vector2 FindShake(float amplitude, float frequency, int octaves, float persistance, float lacunarity, float burstFrequency, int burstContrast, float time)
    {
        float valX = 0;
        float valY = 0;

        float iAmplitude = 1;
        float iFrequency = frequency;
        float maxAmplitude = 0;

        // Burst Frequency
        float burstCoord = time / (1 - burstFrequency);

        // Sample diagonally through perlin noise
        float burstMultiplier = Mathf.PerlinNoise(burstCoord, burstCoord);

        // Apply contrast to the burst multiplier using power, it will make values stay close to zero and less often peak to 1
        burstMultiplier = Mathf.Pow(burstMultiplier, burstContrast);

        for(int i = 0; i < octaves; i++) // Iterate through octaves
        {
            float noiseFrequency = time / (1 - iFrequency) / 10;

            float perlinValueX = Mathf.PerlinNoise(noiseFrequency, 0.5f);
            float perlinValueY = Mathf.PerlinNoise(0.5f, noiseFrequency);

            // Adding small value To keep the average at 0 and   *2 - 1 to keep values between -1 and 1.
            perlinValueX = (perlinValueX + 0.0352f) * 2 - 1;
            perlinValueY = (perlinValueY + 0.0345f) * 2 - 1;

            valX += perlinValueX * iAmplitude;
            valY += perlinValueY * iAmplitude;

            // Keeping track of maximum amplitude for normalizing later
            maxAmplitude += iAmplitude;

            iAmplitude *= persistance;
            iFrequency *= lacunarity;
        }

        valX *= burstMultiplier;
        valY *= burstMultiplier;

        // normalize
        valX /= maxAmplitude;
        valY /= maxAmplitude;

        valX *= amplitude;
        valY *= amplitude;

        return new Vector2(valX, valY);
    }

    void ShakeCamera()
    {
        Vector3 shakeOffset = Vector3.zero;
        Vector2 noise = FindShake(amplitude, frequency, octaves, persistance, lacunarity, burstFrequency, burstContrast, Time.time);
        shakeOffset.x = noise.x;
        shakeOffset.y = noise.y;
        mainCamTransform.localPosition = mainCamLocalOrigin + shakeOffset;
    }
}
