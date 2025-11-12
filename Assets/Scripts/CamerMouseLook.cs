using UnityEngine;

public class CamerMouseLook : MonoBehaviour
{
    // Rotacio
    private const float sensibilitatCamera = 3f;
    private const float alturaCamera = 0.5f;
    private const float rotacioXMin = -30f;
    private const float rotatacioXMax = 30f;
    private float rotacioY = 0f;
    private float rotacioX = 0f;

    // Moviment
    private const float velocitatCamera = 3f;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Rotacio de la camera
        rotacioY += Input.GetAxisRaw("Mouse X") * sensibilitatCamera;
        rotacioX += Input.GetAxisRaw("Mouse Y") * -1 * sensibilitatCamera;
        rotacioX = Mathf.Clamp(rotacioX, rotacioXMin, rotatacioXMax);

        transform.localEulerAngles = new Vector3(rotacioX, rotacioY, 0);

        // Moviment de la camera
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * velocitatCamera * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * velocitatCamera * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * velocitatCamera * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * velocitatCamera * Time.deltaTime;
        }

        transform.position = new Vector3(transform.position.x, alturaCamera, transform.position.z);
    }
}
