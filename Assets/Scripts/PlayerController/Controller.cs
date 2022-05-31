using UnityEngine;

/// <summary>
/// Please do not rate this
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Controller : MonoBehaviour
{
    Rigidbody rb;

    [Header("Controller Settings")]
    public float speed = 4;
    public float groundDrag = 10f;
    public float airDrag = 9.5f;
    public float gravity;
    public float jumpPower = 2;

    float gravityWorker;

    readonly string hAxis = "Horizontal";
    readonly string vAxis = "Vertical";
    readonly string jAxis = "Jump";
    float horizontalInput;
    float verticalInput;
    float jumpInput;

    bool isGrounded;
    public LayerMask groundLayer;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        var direction = transform.forward * verticalInput + transform.right * horizontalInput;
        rb.AddForce(direction.normalized * speed * 10, ForceMode.Force);

        var jumpheight = Vector3.up * jumpInput;
        if (isGrounded)
        {
            rb.AddForce(jumpheight * jumpPower, ForceMode.Impulse);
            gravityWorker = 0;
        }
        else rb.AddForce(Vector3.down * gravityWorker, ForceMode.Force); gravityWorker += gravity;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw(hAxis);
        verticalInput = Input.GetAxisRaw(vAxis);
        jumpInput = Input.GetAxis(jAxis);

        //Gravity
        isGrounded = Physics.CheckSphere(this.transform.position + Vector3.down * 0.8f, 0.3f, groundLayer);
        if (isGrounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(this.transform.position + Vector3.down * 0.8f, 0.3f);
    }
}
