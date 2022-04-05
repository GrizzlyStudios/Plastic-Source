using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float ADSMultiplier;
    private float mouseX;
    private float mouseY;

    [Header("Input System")]
    public PlayerInput playerInput;
    public InputAction lookInput;

    [Header("References")]
    [ReadOnly]
    public PauseMenu PauseMenu;
    [ReadOnly]
    public PlayerController playerController;
    [ReadOnly]
    public BaseGun baseGun;

    private void Awake()
    {
        //input system
        lookInput = playerInput.actions["Look"];

        //references
        PauseMenu = GameObject.Find("Menu").GetComponent<PauseMenu>();
        playerController = GameObject.Find("PlayerBody").GetComponent<PlayerController>();
        baseGun = GameObject.Find("Gun").GetComponent<BaseGun>();
    }

    private void Update()
    {
        if (!PauseMenu.GameIsPaused)
        {
            // get mouse input
            if (baseGun._pressingADS)
            {
                mouseX = Mathf.Clamp(Mathf.Lerp(mouseX, lookInput.ReadValue<Vector2>().x * multiplier * ADSMultiplier * playerController.rotationSensitivity * 0.5f, 15 * Time.deltaTime), -90, 90);
                mouseY = Mathf.Clamp(Mathf.Lerp(mouseY, lookInput.ReadValue<Vector2>().y * multiplier * ADSMultiplier * playerController.rotationSensitivity * 0.5f, 15 * Time.deltaTime), -90, 90);
            }
            else
            {
                mouseX = Mathf.Clamp(Mathf.Lerp(mouseX, lookInput.ReadValue<Vector2>().x * multiplier * playerController.rotationSensitivity * 0.5f, 15 * Time.deltaTime), -90, 90);
                mouseY = Mathf.Clamp(Mathf.Lerp(mouseY, lookInput.ReadValue<Vector2>().y * multiplier * playerController.rotationSensitivity * 0.5f, 15 * Time.deltaTime), -90, 90);
            }


            // calculate target rotation
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            Quaternion targetRotation = rotationX * rotationY;

            // rotate 
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
        }
    }
}
