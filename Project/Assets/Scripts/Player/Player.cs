using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] Camera cam;

    [SerializeField] float jumpHeight = 6;
    [SerializeField] float speed = 4;
    [SerializeField] float lookSensitivity;
    [SerializeField] float smoothing;
    [SerializeField] float flySpeed;
    float walkingSpeed;
    float prevYPosition;

    
    Vector2 currentLookingPosition;
    Vector2 smoothedVelocity;
    Vector3 velocity;


    void Start()
    {
        walkingSpeed = speed;
        prevYPosition = transform.position.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    Vector2 currentVelocity;

    void FixedUpdate()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + (cam.transform.forward * 10));
    }

    private void Update()
    {
        var fwd = ((transform.forward * currentVelocity.y) + (transform.right * currentVelocity.x)).normalized * speed;
        velocity = new Vector3(fwd.x, velocity.y, fwd.z);

        if (flying)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
            transform.position = new Vector3(transform.position.x, prevYPosition, transform.position.z);
        }

        transform.Translate(velocity);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0))
                Cursor.lockState = CursorLockMode.Locked;
            return;
        }

        if (Input.GetKeyDown(KeyCode.I))
            transform.Translate(new Vector3(0, 0, 32), Space.World);

        if (Input.GetKeyDown(KeyCode.K))
            transform.Translate(new Vector3(0, 0, -32), Space.World);

        if (Input.GetKeyDown(KeyCode.L))
            transform.Translate(new Vector3(32, 0, 0), Space.World);

        if (Input.GetKeyDown(KeyCode.J))
            transform.Translate(new Vector3(-32, 0, 0), Space.World);

        var inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        currentVelocity.y = speed * inputs.y * Time.fixedDeltaTime;
        currentVelocity.x = speed * inputs.x * Time.fixedDeltaTime;

        if (flying)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                transform.Translate(new Vector3(0, .25f * flySpeed, 0));
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(0, -.25f * flySpeed, 0));
            }
        }
        else
        {
            speed = walkingSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
        }

        RotateCamera();

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Game");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            flying = !flying;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            speed /= 1.5f;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            speed *= 1.5f;
        }

        prevYPosition = transform.position.y;
    }

    bool flying = true;

    void RotateCamera()
    {
        Vector2 inputValues = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        inputValues = Vector2.Scale(inputValues, new Vector2(smoothing * lookSensitivity, smoothing * lookSensitivity));

        smoothedVelocity.x = Mathf.Lerp(smoothedVelocity.x, inputValues.x, 1 / smoothing);
        smoothedVelocity.y = Mathf.Lerp(smoothedVelocity.y, inputValues.y, 1 / smoothing);

        currentLookingPosition += smoothedVelocity;

        cam.transform.localRotation = Quaternion.AngleAxis(-currentLookingPosition.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(currentLookingPosition.x, transform.up);
    }

}

