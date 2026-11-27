using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;
    public Rigidbody targetRigidbody; // Boşsa otomatik bulur

    [Header("GTA Yakın Takip Mesafe ve Açı")]
    public float distance = 4.6f;
    public float height = 1.65f;
    public float yawOffset = 0f; // Modelin +Z ekseni arkaya bakıyorsa 180 yap
    public Vector3 lookOffset = new Vector3(0f, 1.0f, 0f); // Arabanın ortası/gövde merkezi

    [Header("Drift ve Yön Harmanlama")]
    [Range(0f, 1f)]
    public float driftVelocityBlend = 0.45f;
    public float minSpeedForVelocity = 2.0f;
    public float yawSmoothTime = 0.22f;

    [Header("360 Orbit Fare Bakışı (Tıklamasız)")]
    public float mouseSensitivity = 2.2f;
    public float minLookPitch = -15f;
    public float maxLookPitch = 45f;

    [Header("Otomatik Geri Dönüş")]
    public float returnDelay = 2.0f; // Fare durduktan kaç saniye sonra dönsün
    public float returnSpeed = 5.0f;

    [Header("Yükseklik Yumuşatma")]
    public float heightSmoothTime = 0.12f;

    [Header("Hıza Göre FOV")]
    public Camera targetCamera;       // Boşsa GetComponent<Camera>() ile alır
    public float baseFov = 60f;
    public float maxFov = 78f;
    public float minFovSpeed = 5f;    // Bu hızın altında FOV artmaya başlamaz (m/s)
    public float maxFovSpeed = 35f;   // Bu hızda maxFov'a ulaşır (m/s)
    public float fovSmoothSpeed = 8f;

    private float currentYaw;
    private float yawVelocity;
    private float currentHeight;
    private float heightVelocity;

    // Serbest bakışta dünya eksenine sabitlenen yaw ve pitch açıları
    private float worldOrbitYaw;
    private float orbitPitchOffset;
    private bool isFreeLooking;

    private float idleTimer;
    private bool cursorLocked;
    private bool initialized;

    private void Start()
    {
        ResolveRigidbody();
        ResolveCamera();
        LockCursor(true);
    }

    private void OnDisable()
    {
        LockCursor(false);
    }

    private void ResolveRigidbody()
    {
        if (targetRigidbody == null && target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody>();
            if (targetRigidbody == null)
            {
                targetRigidbody = target.GetComponentInParent<Rigidbody>();
            }
        }

        if (targetRigidbody != null && targetRigidbody.interpolation == RigidbodyInterpolation.None)
        {
            targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void ResolveCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (targetCamera != null)
        {
            targetCamera.fieldOfView = baseFov;
        }
    }

    private void LockCursor(bool lockState)
    {
        cursorLocked = lockState;
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }

    private void HandleMouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
            return;
        }

        if (!cursorLocked && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            LockCursor(true);
            return;
        }

        if (cursorLocked)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            bool movedMouse = Mathf.Abs(mouseX) > 0.0001f || Mathf.Abs(mouseY) > 0.0001f;

            if (movedMouse)
            {
                if (!isFreeLooking)
                {
                    // Serbest bakışa geçerken mevcut kamera açısını dünya yaw'ı olarak kilitle
                    worldOrbitYaw = currentYaw;
                    isFreeLooking = true;
                }

                idleTimer = 0f;
                worldOrbitYaw = Mathf.Repeat(worldOrbitYaw + mouseX * mouseSensitivity, 360f);
                orbitPitchOffset = Mathf.Clamp(orbitPitchOffset - mouseY * mouseSensitivity, minLookPitch, maxLookPitch);
                return;
            }
        }

        if (isFreeLooking)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= returnDelay)
            {
                // Otomatik dönüş başladığında mevcut dünya yaw'ını currentYaw'a aktar ve arabaya hizala
                currentYaw = worldOrbitYaw;
                orbitPitchOffset = Mathf.LerpAngle(orbitPitchOffset, 0f, returnSpeed * Time.deltaTime);
                isFreeLooking = false;
            }
        }
        else
        {
            orbitPitchOffset = Mathf.LerpAngle(orbitPitchOffset, 0f, returnSpeed * Time.deltaTime);
        }
    }

    private void UpdateFov()
    {
        if (targetCamera == null) ResolveCamera();
        if (targetCamera == null || targetRigidbody == null) return;

#if UNITY_6000_0_OR_NEWER
        float speed = targetRigidbody.linearVelocity.magnitude;
#else
        float speed = targetRigidbody.velocity.magnitude;
#endif

        float speedRatio = Mathf.InverseLerp(minFovSpeed, maxFovSpeed, speed);
        float desiredFov = Mathf.Lerp(baseFov, maxFov, speedRatio);
        targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, desiredFov, fovSmoothSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (targetRigidbody == null) ResolveRigidbody();

        Vector3 carForward = target.forward;
        carForward.y = 0f;
        if (carForward.sqrMagnitude < 0.0001f)
        {
            carForward = Vector3.forward;
        }
        carForward.Normalize();

        float desiredYaw = Mathf.Atan2(carForward.x, carForward.z) * Mathf.Rad2Deg + yawOffset;

        // Drift sırasında burun yönü ile gidiş yönünü harmanla
        if (targetRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 vel = targetRigidbody.linearVelocity;
#else
            Vector3 vel = targetRigidbody.velocity;
#endif
            Vector3 flatVel = new Vector3(vel.x, 0f, vel.z);
            float speed = flatVel.magnitude;

            if (speed > minSpeedForVelocity)
            {
                Vector3 moveDir = flatVel.normalized;
                float moveYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + yawOffset;

                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(desiredYaw, moveYaw));
                float driftFactor = Mathf.Clamp01(angleDiff / 75f) * driftVelocityBlend;

                desiredYaw = Mathf.LerpAngle(desiredYaw, moveYaw, driftFactor);
            }
        }

        Vector3 orbitCenter = target.position + lookOffset;

        if (!initialized)
        {
            currentYaw = desiredYaw;
            worldOrbitYaw = desiredYaw;
            currentHeight = height;
            transform.position = CalculateOrbitPosition(orbitCenter, currentYaw, currentHeight, orbitPitchOffset);
            transform.LookAt(orbitCenter);
            initialized = true;
            return;
        }

        HandleMouseLook();

        float activeYaw;
        if (isFreeLooking)
        {
            // Serbest bakışta arabanın dönüşü kamerayı sürüklemez; dünya açısı sabit kalır
            activeYaw = worldOrbitYaw;
        }
        else
        {
            currentYaw = Mathf.SmoothDampAngle(currentYaw, desiredYaw, ref yawVelocity, yawSmoothTime);
            activeYaw = currentYaw;
        }

        currentHeight = Mathf.SmoothDamp(currentHeight, height, ref heightVelocity, heightSmoothTime);

        transform.position = CalculateOrbitPosition(orbitCenter, activeYaw, currentHeight, orbitPitchOffset);
        transform.LookAt(orbitCenter);

        UpdateFov();
    }

    private Vector3 CalculateOrbitPosition(Vector3 center, float yawDeg, float camHeight, float pitchOrbit)
    {
        Quaternion yawRot = Quaternion.Euler(0f, yawDeg, 0f);
        Vector3 backDir = -(yawRot * Vector3.forward);

        float pitchRad = pitchOrbit * Mathf.Deg2Rad;
        float horizontalDist = distance * Mathf.Cos(pitchRad);
        float verticalOffset = camHeight + distance * Mathf.Sin(pitchRad);

        return center + backDir * horizontalDist + Vector3.up * verticalOffset;
    }
}