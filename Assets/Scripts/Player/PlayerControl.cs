using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Camera Values")]
    public Transform cameraAxis;
    public float camRotationSpeed = 45f;

    [Header("Move Values")]
    public float speed = 5f;
    public float roationSpeed = 15f;
    Vector3 velocity = Vector3.zero;
    Vector3 heading = Vector3.zero;
    float currentSpeed = 0f;

    public enum MoveSpace
    {
        global,
        camera,
        player
    }
    public MoveSpace moveSpace = MoveSpace.camera;

    // Start is called before the first frame update
    void Start()
    {
        heading = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        // Initialise velocity. This stops movement if there is no input.
        velocity = Vector3.zero;

        // Find input values
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Add input to velocity
        velocity.x += horizontal;
        velocity.z += vertical;

        velocity.Normalize();

        // Find heading from input values
        if(velocity.magnitude > 0)
        {
            heading = velocity;
        }

        // Calculate velocity. There is no acceleration so velocity is simply scaled to the speed value.
        velocity *= speed;

        currentSpeed = velocity.magnitude;

        // These are temporary camera controls for testing purposes.
        if(Input.GetKey("q"))
        {
            // Rotate cam left
            Vector3 rotation = cameraAxis.eulerAngles;
            rotation.y += camRotationSpeed * Time.deltaTime;
            cameraAxis.eulerAngles = rotation;
        }
        if (Input.GetKey("e"))
        {
            // Rotate cam right
            Vector3 rotation = cameraAxis.eulerAngles;
            rotation.y -= camRotationSpeed * Time.deltaTime;
            cameraAxis.eulerAngles = rotation;
        }
    }

    private void LateUpdate()
    {
        // Update position and rotation based on the indicated transform
        switch(moveSpace)
        {
            case MoveSpace.global:

                // Add position in global space
                transform.position += velocity * Time.deltaTime;

                if (currentSpeed > 0)
                {
                    // Rotate player towards heading with no transform reference
                    Vector3 targetRotation = heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), roationSpeed * Time.deltaTime);
                }
                break;
            case MoveSpace.camera:

                // Add Translation through the cameraAxis
                transform.Translate(velocity * Time.deltaTime, cameraAxis);

                // Rotate player towards heading based on the cameraAxis transform
                if (currentSpeed > 0)
                {
                    Vector3 targetRotation = Quaternion.Euler(cameraAxis.transform.rotation.eulerAngles) * heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), roationSpeed * Time.deltaTime);
                }
                break;
            case MoveSpace.player:

                // Add Translation through this transform
                transform.Translate(velocity * Time.deltaTime, transform);

                // Rotate player towards heading based on this transform
                if (currentSpeed > 0)
                {
                    Vector3 targetRotation = Quaternion.Euler(transform.rotation.eulerAngles) * heading;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), roationSpeed * Time.deltaTime);
                }
                break;
        }
    }
}
