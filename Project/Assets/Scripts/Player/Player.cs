using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] float jumpHeight = 6;
    [SerializeField] float speed = 4;
    [SerializeField] float lookSensitivity;
    [SerializeField] float smoothing;
    float walkingSpeed;

    Vector2 currentLookingPosition;
    Vector2 smoothedVelocity;

    Rigidbody rb;
    Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        walkingSpeed = speed;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Cursor.lockState = CursorLockMode.Locked;
    }

    Vector2 currentVelocity;

    void FixedUpdate()
    {
        var fwd = ((transform.forward * currentVelocity.y) + (transform.right * currentVelocity.x)).normalized * speed;
        rb.velocity = new Vector3(fwd.x, rb.velocity.y, fwd.z);

        if (flying)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(cam.transform.position, cam.transform.position + (cam.transform.forward * 10));
    }

    private void Update()
    {
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

        var inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        currentVelocity.y = speed * inputs.y * Time.fixedDeltaTime;
        currentVelocity.x = speed * inputs.x * Time.fixedDeltaTime;

        if (flying)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                transform.Translate(new Vector3(0, .25f, 0));
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                transform.Translate(new Vector3(0, -.25f, 0));
            }
        }
        else
        {
            speed = walkingSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (rb.velocity.y <= 0 && CanJump())
            {
                rb.AddForce(new Vector3(0, jumpHeight), ForceMode.Impulse);
            }
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

    }

    bool flying = true;

    bool CanJump()
    {
        if (Physics.Raycast(transform.position, -transform.up, 1.01f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

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

