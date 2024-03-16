using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Replicate Data class. Includes everything that the server and the player must do at the same time
public struct ReplicateDataPlayerController : IReplicateData
{
    public bool Jump;
    public bool Fire;
    public float Horizontal;
    public float Vertical;
    public float MouseX;
    public float MouseY;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

// Reconcile Data. Includes everything that the server and the player must sync if they desynchronize
public struct ReconcileDataPlayerController : IReconcileData
{
    public Vector3 Position;
    public float VerticalVelocity;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public class PlayerController : NetworkBehaviour
{
    public static Dictionary<int, PlayerController> Players = new();

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpSpeed = 4f;
    public float gravity = 9.81f;
    public bool canMove = true;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookLimitX = 25f;

    [Header("Prefabs")]
    public GameObject cameraPrefab;
    public GameObject hpBarPrefab;

    // Using Events and a separate class to divide the responsabilities of each class
    [Header("Events")]
    public UnityEvent<Vector2> OnMovementChanged;
    public UnityEvent OnJumpPerformed;
    public UnityEvent OnAttackPerformed;
    public UnityEvent OnPlayerRespawn;

    private Transform playerCamera;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;
    private GameObject hpBar;

    // Since this values must be getted by button or button down, I'm caching them for the next tick
    private bool _jumpQueued;
    private bool _fireQueued;
    private float _verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        Players.Remove(OwnerId);

        if (base.TimeManager != null)
            base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        gameObject.name = "Player [" + base.OwnerId + "]";
        Players.Add(OwnerId, this);
        PlayerManager.InitializeNewPlayer(base.OwnerId);
        SetPlayer();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Players.Remove(OwnerId);
        PlayerManager.DeletePlayer(base.OwnerId);
    }


    public static void TogglePlayer(int clientId, bool toggle)
    {
        if (!Players.TryGetValue(clientId, out PlayerController player)) return;

        player.TogglePlayerServer(toggle);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerServer(bool toggle)
    {
        TogglePlayerObserver(toggle);
    }

    [ObserversRpc]
    private void TogglePlayerObserver(bool toggle)
    {
        canMove = toggle;

        if (TryGetComponent(out CapsuleCollider collider)) collider.enabled = toggle;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            renderer.enabled = toggle;
        }

        if (hpBar != null) hpBar.SetActive(toggle);

        characterController.enabled = toggle;
    }

    public static void RespawnPlayer(int clientId, Vector3 position)
    {
        if (!Players.TryGetValue(clientId, out PlayerController player)) return;

        player.OnPlayerRespawn?.Invoke();
        player.SetPlayerPositionServer(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerPositionServer(Vector3 position)
    {
        transform.position = position;
        SetPlayerPositionObserver(position);
    }

    [ObserversRpc]
    private void SetPlayerPositionObserver(Vector3 position)
    {
        transform.position = position;
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            if (base.TimeManager.Tick % 3 == 0) Reconcile(default, false);
            BuildActions(out ReplicateDataPlayerController replicateData);
            PlayerActions(replicateData, false);
        }
        if (base.IsServer)
        {
            PlayerActions(default, true);
            ReconcileDataPlayerController reconcileData = new ReconcileDataPlayerController()
            {
                Position = transform.position,
                VerticalVelocity = _verticalVelocity
            };
            if (base.TimeManager.Tick % 3 == 0) Reconcile(reconcileData, true);
        }
    }

    // The client sets the values to be used by the server to perform actions at the same time
    private void BuildActions(out ReplicateDataPlayerController replicateData)
    {
        replicateData = default;
        replicateData.Jump = _jumpQueued;
        replicateData.Fire = _fireQueued;
        replicateData.Horizontal = Input.GetAxis("Horizontal");
        replicateData.Vertical = Input.GetAxis("Vertical");
        replicateData.MouseX = Input.GetAxis("Mouse X");
        replicateData.MouseY = Input.GetAxis("Mouse Y");

        _jumpQueued = false;
        _fireQueued = false;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        _jumpQueued = Input.GetButton("Jump");
        _fireQueued = Input.GetButton("Fire");
    }

    private void SetPlayer()
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
        }
        else
        {
            hpBar = Instantiate(hpBarPrefab, transform);
        }
    }

    [Replicate]
    private void PlayerActions(ReplicateDataPlayerController replicateData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        // The functions are separated to improve code legibility

        Movement(replicateData, asServer);
        Rotation(replicateData, asServer);
        Attack(replicateData, asServer);
    }

    private void Movement(ReplicateDataPlayerController replicateData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        float delta = (float)base.TimeManager.TickDelta;

        // Get the local forward and right to world coordinates
        Vector3 playerForward = transform.TransformDirection(Vector3.forward);
        Vector3 playerRight = transform.TransformDirection(Vector3.right);

        float verticalAxis = canMove ? replicateData.Vertical : 0;
        float horizontalAxis = canMove ? replicateData.Horizontal : 0;

        try
        {
            OnMovementChanged?.Invoke(new(horizontalAxis, verticalAxis));
        }
        catch (System.Exception) { }

        float moveSpeedVertical = moveSpeed * verticalAxis;
        float moveSpeedHorizontal = moveSpeed * horizontalAxis;

        float currentDirectionY = _verticalVelocity;
        moveDirection = playerForward * moveSpeedVertical + playerRight * moveSpeedHorizontal;

        if (replicateData.Jump && canMove && characterController.isGrounded)
        {
            // Applying the jump as move direction instead of force so it can interacts with custom gravity of the CharacterController
            _verticalVelocity = jumpSpeed;
            OnJumpPerformed?.Invoke();
        }
        else
        {
            _verticalVelocity = currentDirectionY;
        }

        // The gravity is multiplied by delta time so it increases gradually
        if (!characterController.isGrounded) _verticalVelocity -= gravity * delta;

        moveDirection.y = _verticalVelocity;

        if (!canMove) return;
        characterController.Move(moveDirection * delta);
    }

    private void Rotation(ReplicateDataPlayerController replicateData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (!canMove || playerCamera == null) return;

        // The rotation X and Y are handled separately to make the player unable to rotate on it's X angle

        rotationX -= replicateData.MouseY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookLimitX, lookLimitX);

        // By rotating the Target of the camera, Cinemachine is going to follow that rotation and apply damping
        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, replicateData.MouseX * lookSpeed, 0);
    }

    private void Attack(ReplicateDataPlayerController replicateData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        // Using GetButton so players can hold to keep attacking
        if (replicateData.Fire)
        {
            OnAttackPerformed?.Invoke();
        }
    }

    [Reconcile]
    private void Reconcile(ReconcileDataPlayerController reconcileData, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = reconcileData.Position;
        _verticalVelocity = reconcileData.VerticalVelocity;
    }
}
