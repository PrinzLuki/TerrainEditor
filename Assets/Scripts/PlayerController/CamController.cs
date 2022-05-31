using UnityEngine;

/// <summary>
/// Please do not rate this
/// </summary>
public class CamController : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float sensitivity = 10;
    readonly string xMouse = "Mouse X";
    readonly string yMouse = "Mouse Y";

    float xRot;
    float yRot;

    [SerializeField] Transform playerT;

    private void Update()
    {
        float mouseX = Input.GetAxisRaw(xMouse) * Time.deltaTime * sensitivity;
        float mouseY = Input.GetAxisRaw(yMouse) * Time.deltaTime * sensitivity;

        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
        playerT.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
