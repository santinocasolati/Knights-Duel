using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpSpeed = 4f;
    public float gravity = 9.81f;
    public bool canMove = true;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookLimitX = 25f;
    public GameObject cameraPrefab;

    private Transform playerCamera;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnStartClient()
    {
        gameObject.name = "Player [" + base.OwnerId + "]";
        SetCamera();
    }

    private void SetCamera()
    {
        // If the player is not the owner we don't need neither the Camera nor the PlayerController
        // The Camera is instantiated to not cause problems with two virtual cameras for a few frames
        // Using Cinemachine to improve camera effects

        if (base.IsOwner)
        {
            GameObject camera = Instantiate(cameraPrefab, transform);
            playerCamera = camera.transform.Find("Target");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        } else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        // The functions are separated to improve code legibility

        Movement();
        Rotation();
    }

    private void Movement()
    {
        // Get the local forward and right to world coordinates
        Vector3 playerForward = transform.TransformDirection(Vector3.forward);
        Vector3 playerRight = transform.TransformDirection(Vector3.right);

        float moveSpeedVertical = canMove ? moveSpeed * Input.GetAxis("Vertical") : 0;
        float moveSpeedHorizontal = canMove ? moveSpeed * Input.GetAxis("Horizontal") : 0;

        float currentDirectionY = moveDirection.y;
        moveDirection = playerForward * moveSpeedVertical + playerRight * moveSpeedHorizontal;

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            // Applying the jump as move direction instead of force so it can interacts with custom gravity of the CharacterController
            moveDirection.y = jumpSpeed;
        } else
        {
            moveDirection.y = currentDirectionY;
        }

        // The gravity is multiplied by delta time so it increases gradually
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void Rotation()
    {
        if (!canMove || playerCamera == null) return;

        // The rotation X and Y are handled separately to make the player unable to rotate on it's X angle

        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookLimitX, lookLimitX);

        // By rotating the Target of the camera, Cinemachine is going to follow that rotation and apply damping
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
        
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
