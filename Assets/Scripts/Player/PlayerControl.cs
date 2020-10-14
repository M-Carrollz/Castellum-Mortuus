using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Camera Values")]
    public Transform cameraAxis;
    public float camRotationSpeed = 45f;
    public int rotateMouseButton = 1;
    public int panMouseButton = 0;

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

        // Camera Controls

        // Find mouse Input
        Vector3 mouseInput = Vector3.zero;
        mouseInput.x = Input.GetAxis("Mouse X");
        mouseInput.z = Input.GetAxis("Mouse Y");

        // Rotation
        if (Input.GetMouseButton(rotateMouseButton))
        {
            Vector3 rotation = cameraAxis.eulerAngles;
            rotation.y += mouseInput.x * camRotationSpeed * Time.deltaTime;
            cameraAxis.eulerAngles = rotation;
        }
        // Panning
        if(Input.GetMouseButton(panMouseButton))
        {

            cameraAxis.Translate(-mouseInput, cameraAxis);

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
