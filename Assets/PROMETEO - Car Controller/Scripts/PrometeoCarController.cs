/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{

    //CAR SETUP

      [Space(20)]
      [Space(10)]
      [Range(20, 220)]
      public int maxSpeed = 140;
      [Range(10, 120)]
      public int maxReverseSpeed = 45;
      [Range(1, 10)]
      public int accelerationMultiplier = 4;
      [Space(10)]
      [Range(10, 45)]
      public int maxSteeringAngle = 27;
      [Range(0.1f, 1f)]
      public float steeringSpeed = 0.5f;
      [Space(10)]
      [Range(100, 600)]
      public int brakeForce = 350;
      [Range(0.5f, 10f)]
      public float brakeBuildSpeed = 2.5f;
      [Range(1, 10)]
      public int decelerationMultiplier = 2;
      [Range(1, 10)]
      public int handbrakeDriftMultiplier = 4;
      [Space(10)]
      public Vector3 bodyMassCenter = new Vector3(0f, -0.35f, 0f);

    //GTA 5 FEEL (SIMPLE & CLEAN)

      [Space(15)]
      [Space(5)]
      [Range(0.8f, 3.0f)]
      public float roadGrip = 1.65f;                 // Normal yolda yere tutunma (el frenini etkilemez)
      public bool launchSpinEnabled = true;          // Kalkista pati ve duman acik/kapali
      [Range(4f, 35f)]
      public float launchSpinSpeed = 22f;            // Kalkis patisinin uzadigi hiz siniri (km/h)
      [Range(0.1f, 1.0f)]
      public float launchGrip = 0.26f;               // Kalkis suresi boyunca dusuk arka tutus
      [Range(0f, 3.5f)]
      public float launchSteerKick = 1.35f;          // Kalkista sag/sol yapinca drifte sokan yanal savurma gucu
      [Range(0f, 2500f)]
      public float launchSpinVisualSpeed = 1100f;    // Launch spin sirasinda arka tekerleklerin gorsel donus hizi (deg/s)
      [Range(0.5f, 12f)]
      public float launchBlendSpeed = 3.2f;          // Launch spin ile road grip arasindaki yumusak gecis hizi
      [Range(0f, 1f)]
      public float counterSteerAssist = 0.45f;       // Drift sirasinda arabayi toparlamaya yardimci karsi direksiyon gucu
      [Range(0.2f, 1f)]
      public float highSpeedSteerScale = 0.42f;      // Yuksek hizda direksiyon hareket carpanı (dusuk hizda 1.0)
      [Range(0.2f, 0.85f)]
      public float handbrakeGrip = 0.42f;            // El freni (Space) cekince arka teker tutusu
      [Range(0.1f, 1.5f)]
      public float handbrakeBrakeStrength = 0.65f;   // El freninin arabayi yavaslatma gucu (brakeForce carpani)
      [Range(0.8f, 4.5f)]
      public float handbrakeSpinLimit = 1.85f;       // El freninde spin onleyici max donus hizi

    //WHEELS

      public GameObject frontLeftMesh;
      public WheelCollider frontLeftCollider;
      [Space(10)]
      public GameObject frontRightMesh;
      public WheelCollider frontRightCollider;
      [Space(10)]
      public GameObject rearLeftMesh;
      public WheelCollider rearLeftCollider;
      [Space(10)]
      public GameObject rearRightMesh;
      public WheelCollider rearRightCollider;

    //PARTICLE SYSTEMS

      [Space(20)]
      [Space(10)]
      public bool useEffects = false;
      public ParticleSystem RLWParticleSystem;
      public ParticleSystem RRWParticleSystem;
      [Space(10)]
      public TrailRenderer RLWTireSkid;
      public TrailRenderer RRWTireSkid;

    //SPEED TEXT (UI)

      [Space(20)]
      [Space(10)]
      public bool useUI = false;
      public Text carSpeedText;

    //SOUNDS

      [Space(20)]
      [Space(10)]
      public bool useSounds = false;
      public AudioSource carEngineSound;
      public AudioSource tireScreechSound;
      float initialCarEngineSoundPitch;
      float initialCarEngineSoundVolume;
      float initialTireScreechVolume;
      float currentEnginePitch;

    //CONTROLS

      [Space(20)]
      [Space(10)]
      public bool useTouchControls = false;
      public GameObject throttleButton;
      PrometeoTouchInput throttlePTI;
      public GameObject reverseButton;
      PrometeoTouchInput reversePTI;
      public GameObject turnRightButton;
      PrometeoTouchInput turnRightPTI;
      public GameObject turnLeftButton;
      PrometeoTouchInput turnLeftPTI;
      public GameObject handbrakeButton;
      PrometeoTouchInput handbrakePTI;

    //CAR DATA

      [HideInInspector]
      public float carSpeed;
      [HideInInspector]
      public bool isDrifting;
      [HideInInspector]
      public bool isTractionLocked;

    //PRIVATE VARIABLES

      Rigidbody carRigidbody;
      float steeringAxis;
      float throttleAxis;
      float brakeAxis;
      float driftingAxis;
      float localVelocityZ;
      float localVelocityX;
      bool deceleratingCar;
      bool touchControlsSetup = false;
      bool steeringInputHeld = false;
      bool wasSlidingOrDrifting = false;
      bool isLaunchSpinning = false;
      float launchSpinDirection = 1f;
      float launchBlend = 0f;
      float postHandbrakeWheelspinTimer = 0f;
      float extraRearSpinAngle = 0f;
      bool rearWheelsLocked = false;
      Quaternion lockedRearLeftLocalRot;
      Quaternion lockedRearRightLocalRot;

      WheelFrictionCurve RLwheelFriction;
      float RLWextremumSlip;
      float RLWinitialStiffness;
      WheelFrictionCurve RRwheelFriction;
      float RRWextremumSlip;
      float RRWinitialStiffness;

    void Start()
    {
      carRigidbody = GetComponent<Rigidbody>();
      if (carRigidbody != null)
      {
        carRigidbody.centerOfMass = bodyMassCenter;
        if (carRigidbody.interpolation == RigidbodyInterpolation.None)
        {
          carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
      }

      RLwheelFriction = rearLeftCollider.sidewaysFriction;
      RLWextremumSlip = RLwheelFriction.extremumSlip;
      RLWinitialStiffness = Mathf.Max(0.1f, RLwheelFriction.stiffness);

      RRwheelFriction = rearRightCollider.sidewaysFriction;
      RRWextremumSlip = RRwheelFriction.extremumSlip;
      RRWinitialStiffness = Mathf.Max(0.1f, RRwheelFriction.stiffness);

      if (carEngineSound != null)
      {
        initialCarEngineSoundPitch = Mathf.Max(0.5f, carEngineSound.pitch);
        initialCarEngineSoundVolume = Mathf.Max(0.1f, carEngineSound.volume);
        currentEnginePitch = initialCarEngineSoundPitch;
        carEngineSound.loop = true;
      }

      if (tireScreechSound != null)
      {
        initialTireScreechVolume = Mathf.Max(0.1f, tireScreechSound.volume);
        tireScreechSound.loop = true;
      }

      if (useUI) InvokeRepeating("CarSpeedUI", 0f, 0.1f);
      else if (carSpeedText != null) carSpeedText.text = "0";

      if (useSounds)
      {
        if (carEngineSound != null && !carEngineSound.isPlaying)
          carEngineSound.Play();
        InvokeRepeating("CarSounds", 0f, 0.05f);
      }
      else
      {
        if (carEngineSound != null) carEngineSound.Stop();
        if (tireScreechSound != null) tireScreechSound.Stop();
      }

      if (!useEffects)
      {
        if (RLWParticleSystem != null) RLWParticleSystem.Stop();
        if (RRWParticleSystem != null) RRWParticleSystem.Stop();
        if (RLWTireSkid != null) RLWTireSkid.emitting = false;
        if (RRWTireSkid != null) RRWTireSkid.emitting = false;
      }

      if (useTouchControls && throttleButton != null && reverseButton != null &&
          turnRightButton != null && turnLeftButton != null && handbrakeButton != null)
      {
        throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
        reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
        turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
        turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
        handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
        touchControlsSetup = true;
      }
    }

    void Update()
    {
      float avgFrontRpm = (frontLeftCollider.rpm + frontRightCollider.rpm) * 0.5f;
      carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * avgFrontRpm * 60) / 1000f;
      localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
      localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

      if (postHandbrakeWheelspinTimer > 0f)
      {
        postHandbrakeWheelspinTimer = Mathf.Max(0f, postHandbrakeWheelspinTimer - Time.deltaTime);
      }

      if (useTouchControls && touchControlsSetup)
      {
        steeringInputHeld = turnLeftPTI.buttonPressed || turnRightPTI.buttonPressed;
        if (turnLeftPTI.buttonPressed) TurnLeft();
        if (turnRightPTI.buttonPressed) TurnRight();

        if (throttlePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; ReleaseBrakes(); GoForward(); }
        if (reversePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; if (localVelocityZ > 1.5f) Brakes(); else { ReleaseBrakes(); GoReverse(); } }
        else if (!throttlePTI.buttonPressed) ReleaseBrakes();

        if (handbrakePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake(); }
        else RecoverTraction();

        if (!throttlePTI.buttonPressed && !reversePTI.buttonPressed) ThrottleOff();
        if (!reversePTI.buttonPressed && !throttlePTI.buttonPressed && !handbrakePTI.buttonPressed && !deceleratingCar)
        {
          InvokeRepeating("DecelerateCar", 0f, 0.1f);
          deceleratingCar = true;
        }
        if (!steeringInputHeld) ResetSteeringAngle();
      }
      else
      {
        steeringInputHeld = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
        if (Input.GetKey(KeyCode.A)) TurnLeft();
        if (Input.GetKey(KeyCode.D)) TurnRight();

        if (Input.GetKey(KeyCode.W)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; ReleaseBrakes(); GoForward(); }
        if (Input.GetKey(KeyCode.S)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; if (localVelocityZ > 1.5f) Brakes(); else { ReleaseBrakes(); GoReverse(); } }
        else if (!Input.GetKey(KeyCode.W)) ReleaseBrakes();

        if (Input.GetKey(KeyCode.Space)) { CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake(); }
        if (Input.GetKeyUp(KeyCode.Space)) RecoverTraction();

        if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) ThrottleOff();
        if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.Space) && !deceleratingCar)
        {
          InvokeRepeating("DecelerateCar", 0f, 0.1f);
          deceleratingCar = true;
        }
        if (!steeringInputHeld) ResetSteeringAngle();
      }

      // Duman ve lastik izi durumunu guncelle
      bool throttleActive = useTouchControls && touchControlsSetup
        ? (throttlePTI.buttonPressed || reversePTI.buttonPressed)
        : (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S));

      float absSpd = Mathf.Abs(carSpeed);
      bool slidingAfterDrift = !isTractionLocked && absSpd > 6f && (Mathf.Abs(localVelocityX) > 2.5f || postHandbrakeWheelspinTimer > 0f || driftingAxis > 0.05f);

      if (!throttleActive && !isTractionLocked && !slidingAfterDrift)
      {
        isLaunchSpinning = false;
        isDrifting = false;
      }
      else if (slidingAfterDrift)
      {
        isDrifting = true;
      }

      bool currentlySlidingOrDrifting = isDrifting || isTractionLocked;

      // Driftten cikis aninda direksiyon acisini ani koparmadan mevcut teker acisiyla esitle
      if (wasSlidingOrDrifting && !currentlySlidingOrDrifting && frontLeftCollider != null)
      {
        float activeMaxAngle = Mathf.Max(1f, maxSteeringAngle * GetSpeedSteerMultiplier());
        steeringAxis = Mathf.Clamp(frontLeftCollider.steerAngle / activeMaxAngle, -1f, 1f);
      }
      wasSlidingOrDrifting = currentlySlidingOrDrifting;

      ApplyCounterSteerAssist();

      // Drift aktif degilse ve direksiyon inputu gelmiyorsa tekerlekleri yumusakca 0'a (duz konuma) cek
      if (!steeringInputHeld && !currentlySlidingOrDrifting)
      {
        float axisReturnSpeed = Time.deltaTime * (4.2f + steeringSpeed * 5.5f);
        steeringAxis = Mathf.MoveTowards(steeringAxis, 0f, axisReturnSpeed);

        float maxDegPerSec = maxSteeringAngle * (1.35f + steeringSpeed * 2.8f);
        float targetAngle = steeringAxis * maxSteeringAngle * GetSpeedSteerMultiplier();
        float nextLeftAngle = Mathf.MoveTowards(frontLeftCollider.steerAngle, targetAngle, maxDegPerSec * Time.deltaTime);
        float nextRightAngle = Mathf.MoveTowards(frontRightCollider.steerAngle, targetAngle, maxDegPerSec * Time.deltaTime);

        float smoothRate = Mathf.Clamp01(Time.deltaTime * (4.5f + steeringSpeed * 6f));
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, nextLeftAngle, smoothRate);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, nextRightAngle, smoothRate);

        if (Mathf.Abs(frontLeftCollider.steerAngle) < 0.03f && Mathf.Abs(steeringAxis) < 0.01f)
        {
          frontLeftCollider.steerAngle = 0f;
          frontRightCollider.steerAngle = 0f;
          steeringAxis = 0f;
        }
      }

      // El frenindeyken spin onleyici donus hizi siniri
      if (carRigidbody != null && isTractionLocked)
      {
        Vector3 angVel = carRigidbody.angularVelocity;
        angVel.y = Mathf.Clamp(angVel.y, -handbrakeSpinLimit, handbrakeSpinLimit);
        carRigidbody.angularVelocity = angVel;
      }

      // Gorsel arka teker donusu sadece launch spin veya bosluktan cikis wheelspin durumuna bagli kalsin
      if (!isTractionLocked && isLaunchSpinning && launchSpinVisualSpeed > 0f)
      {
        extraRearSpinAngle = (extraRearSpinAngle + launchSpinDirection * launchSpinVisualSpeed * Time.deltaTime) % 360f;
      }
      else
      {
        extraRearSpinAngle = 0f;
      }

      AnimateWheelMeshes();
      DriftCarPS();
    }

    float GetSpeedSteerMultiplier()
    {
      float speedRatio = Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(20f, maxSpeed));
      // 0 km/h -> 1.0, yuksek hiz -> highSpeedSteerScale
      return Mathf.Lerp(1f, Mathf.Clamp(highSpeedSteerScale, 0.2f, 1f), speedRatio);
    }

    void ApplySteerAngle()
    {
      float speedSteerMult = GetSpeedSteerMultiplier();
      float effectiveRate = Mathf.Clamp01(steeringSpeed * Mathf.Lerp(1.25f, 0.55f, Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(20f, maxSpeed))));
      float angle = steeringAxis * maxSteeringAngle * speedSteerMult;

      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, effectiveRate);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, effectiveRate);
    }

    void ApplyCounterSteerAssist()
    {
      if (counterSteerAssist <= 0f || frontLeftCollider == null || frontRightCollider == null) return;

      float absSpd = Mathf.Abs(carSpeed);
      bool activelyDrifting = absSpd > 4f && (isDrifting || isTractionLocked);
      if (!activelyDrifting) return;

      // Dusuk hizda (0..45 km/h) daha fazla topla, yuksek hizda daha az ve yumusak topla
      float lowSpeedFactor = 1f - Mathf.Clamp01(absSpd / 50f);
      float highSpeedFactor = Mathf.Clamp01(absSpd / Mathf.Max(40f, maxSpeed));

      float speedAdjustedAssist = counterSteerAssist * Mathf.Lerp(1.65f, 0.45f, highSpeedFactor);
      float slipNorm = Mathf.Clamp(localVelocityX / 8.5f, -1f, 1f);
      float counterAngle = slipNorm * maxSteeringAngle * speedAdjustedAssist * Mathf.Lerp(0.7f, 1.15f, lowSpeedFactor);

      float playerAngle = steeringAxis * maxSteeringAngle * GetSpeedSteerMultiplier();
      float targetAngle = Mathf.Clamp(playerAngle + counterAngle, -maxSteeringAngle, maxSteeringAngle);

      // Yuksek hizda aninda toplamayi onlemek icin lerp hizini hizla birlikte dusur
      float blendFactor = Mathf.Clamp01(steeringSpeed * Mathf.Lerp(2.8f, 0.65f, highSpeedFactor) * (0.5f + counterSteerAssist));
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, targetAngle, blendFactor);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, targetAngle, blendFactor);
    }

    void ApplyNormalAndLaunchGrip()
    {
      if (isTractionLocked)
      {
        isLaunchSpinning = false;
        launchBlend = Mathf.MoveTowards(launchBlend, 0f, launchBlendSpeed * 2f * Time.deltaTime);
        return;
      }

      float absSpeed = Mathf.Abs(carSpeed);
      bool throttleHeld = useTouchControls && touchControlsSetup
        ? (throttlePTI.buttonPressed || reversePTI.buttonPressed)
        : (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S));

      bool reverseHeld = useTouchControls && touchControlsSetup
        ? reversePTI.buttonPressed
        : Input.GetKey(KeyCode.S);

      bool inPostHandbrakeWheelspin = launchSpinEnabled && postHandbrakeWheelspinTimer > 0f && absSpeed < Mathf.Max(launchSpinSpeed * 2.5f, 65f);
      bool inLaunch = launchSpinEnabled && ((throttleHeld && absSpeed > 0.25f && absSpeed < launchSpinSpeed) || inPostHandbrakeWheelspin);
      bool steeringDuringLaunch = inLaunch && Mathf.Abs(steeringAxis) > 0.15f;

      isLaunchSpinning = inLaunch;
      if (inLaunch)
      {
        launchSpinDirection = (reverseHeld || localVelocityZ < -0.5f) ? -1f : 1f;
      }

      // W veya S basilmadiginda blend degerini hizla sifirla
      float targetBlend = inLaunch ? 1f : 0f;
      float blendRate = throttleHeld ? launchBlendSpeed : launchBlendSpeed * 4f;
      launchBlend = Mathf.MoveTowards(launchBlend, targetBlend, blendRate * Time.deltaTime);

      float activeLaunchGrip = steeringDuringLaunch ? Mathf.Max(0.08f, launchGrip * 0.65f) : launchGrip;
      float targetGrip = Mathf.Lerp(roadGrip, activeLaunchGrip, launchBlend);

      if (throttleHeld && launchBlend > 0.02f && steeringDuringLaunch && launchSteerKick > 0f && carRigidbody != null)
      {
        float dirSign = localVelocityZ < -0.5f ? -1f : 1f;
        float blendedKick = launchSteerKick * launchBlend;

        carRigidbody.AddTorque(transform.up * (steeringAxis * blendedKick * dirSign * 1.65f), ForceMode.Acceleration);
      }

      if (inLaunch && Mathf.Abs(throttleAxis) > 0.1f)
      {
        float spinTorque = accelerationMultiplier * 65f * Mathf.Sign(throttleAxis);
        rearLeftCollider.motorTorque = spinTorque;
        rearRightCollider.motorTorque = spinTorque;
      }

      bool slidingAfterDrift = absSpeed > 6f && (Mathf.Abs(localVelocityX) > 2.5f || postHandbrakeWheelspinTimer > 0f || driftingAxis > 0.05f);
      isDrifting = slidingAfterDrift || (throttleHeld && (inLaunch || (Mathf.Abs(localVelocityX) > 4.2f && absSpeed > 14f)));

      RLwheelFriction.stiffness = RLWinitialStiffness * targetGrip;
      RLwheelFriction.extremumSlip = RLWextremumSlip * Mathf.Lerp(1f, 1.75f, launchBlend);
      rearLeftCollider.sidewaysFriction = RLwheelFriction;

      RRwheelFriction.stiffness = RRWinitialStiffness * targetGrip;
      RRwheelFriction.extremumSlip = RRWextremumSlip * Mathf.Lerp(1f, 1.75f, launchBlend);
      rearRightCollider.sidewaysFriction = RRwheelFriction;
    }

    public void CarSpeedUI()
    {
      if (useUI && carSpeedText != null)
      {
        try { carSpeedText.text = Mathf.RoundToInt(Mathf.Abs(carSpeed)).ToString(); }
        catch (Exception ex) { Debug.LogWarning(ex); }
      }
    }

    public void CarSounds()
    {
      if (useSounds)
      {
        try
        {
          float speedNorm = Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(1f, maxSpeed));
          float throttleLoad = Mathf.Abs(throttleAxis);
          float spinBoost = isLaunchSpinning ? 0.35f : 0f;

          if (carEngineSound != null)
          {
            carEngineSound.loop = true;
            if (!carEngineSound.isPlaying && carEngineSound.clip != null)
              carEngineSound.Play();

            // Gaz verildiginde ve wheelspin sirasinda devir artisi hissini yumusak takip et
            float targetPitch = initialCarEngineSoundPitch + (speedNorm * 0.85f) + (throttleLoad * 0.45f) + spinBoost;
            currentEnginePitch = Mathf.Lerp(currentEnginePitch, targetPitch, 0.25f);
            carEngineSound.pitch = Mathf.Clamp(currentEnginePitch, 0.65f, 2.4f);

            float targetVolume = initialCarEngineSoundVolume * Mathf.Clamp(0.55f + (speedNorm * 0.25f) + (throttleLoad * 0.25f) + (spinBoost * 0.2f), 0.45f, 1.15f);
            carEngineSound.volume = Mathf.Lerp(carEngineSound.volume, targetVolume, 0.25f);
          }

          if (tireScreechSound != null)
          {
            tireScreechSound.loop = true;
            bool shouldScreech = (isDrifting && Mathf.Abs(carSpeed) > 3f) || (isTractionLocked && Mathf.Abs(carSpeed) > 10f);
            if (shouldScreech)
            {
              if (!tireScreechSound.isPlaying && tireScreechSound.clip != null)
                tireScreechSound.Play();

              float slipFactor = Mathf.Clamp01(Mathf.Abs(localVelocityX) / 10f);
              float spinFactor = isLaunchSpinning ? 0.55f : 0.35f;
              float intensity = Mathf.Clamp01(Mathf.Max(slipFactor, spinFactor));

              tireScreechSound.volume = Mathf.Lerp(tireScreechSound.volume, initialTireScreechVolume * intensity, 0.3f);
              tireScreechSound.pitch = Mathf.Lerp(tireScreechSound.pitch, Mathf.Clamp(0.85f + (intensity * 0.35f), 0.8f, 1.35f), 0.3f);
            }
            else if (tireScreechSound.isPlaying)
            {
              tireScreechSound.volume = Mathf.Lerp(tireScreechSound.volume, 0f, 0.4f);
              if (tireScreechSound.volume < 0.02f) tireScreechSound.Stop();
            }
          }
        }
        catch (Exception ex) { Debug.LogWarning(ex); }
      }
      else
      {
        if (carEngineSound != null && carEngineSound.isPlaying) carEngineSound.Stop();
        if (tireScreechSound != null && tireScreechSound.isPlaying) tireScreechSound.Stop();
      }
    }

    public void TurnLeft()
    {
      float speedRateMult = Mathf.Lerp(1.25f, 0.55f, Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(20f, maxSpeed)));
      steeringAxis = Mathf.Max(-1f, steeringAxis - Time.deltaTime * 10f * steeringSpeed * speedRateMult);
      ApplySteerAngle();
    }

    public void TurnRight()
    {
      float speedRateMult = Mathf.Lerp(1.25f, 0.55f, Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(20f, maxSpeed)));
      steeringAxis = Mathf.Min(1f, steeringAxis + Time.deltaTime * 10f * steeringSpeed * speedRateMult);
      ApplySteerAngle();
    }

    public void ResetSteeringAngle()
    {
      float speedRateMult = Mathf.Lerp(1.25f, 0.65f, Mathf.Clamp01(Mathf.Abs(carSpeed) / Mathf.Max(20f, maxSpeed)));
      if (steeringAxis < 0f) steeringAxis = Mathf.Min(0f, steeringAxis + Time.deltaTime * 10f * steeringSpeed * speedRateMult);
      else if (steeringAxis > 0f) steeringAxis = Mathf.Max(0f, steeringAxis - Time.deltaTime * 10f * steeringSpeed * speedRateMult);
      if (Mathf.Abs(steeringAxis) < 0.01f) steeringAxis = 0f;

      ApplySteerAngle();
    }

    void AnimateWheelMeshes()
    {
      try
      {
        Quaternion rot;
        Vector3 pos;

        frontLeftCollider.GetWorldPose(out pos, out rot);
        frontLeftMesh.transform.SetPositionAndRotation(pos, rot);

        frontRightCollider.GetWorldPose(out pos, out rot);
        frontRightMesh.transform.SetPositionAndRotation(pos, rot);

        Quaternion extraSpinRot = Quaternion.Euler(extraRearSpinAngle, 0f, 0f);
        Quaternion invCarRot = Quaternion.Inverse(transform.rotation);

        rearLeftCollider.GetWorldPose(out pos, out rot);
        if (isTractionLocked)
        {
          if (!rearWheelsLocked)
          {
            lockedRearLeftLocalRot = invCarRot * rearLeftMesh.transform.rotation;
          }
          rearLeftMesh.transform.SetPositionAndRotation(pos, transform.rotation * lockedRearLeftLocalRot);
        }
        else
        {
          rearLeftMesh.transform.SetPositionAndRotation(pos, rot * extraSpinRot);
        }

        rearRightCollider.GetWorldPose(out pos, out rot);
        if (isTractionLocked)
        {
          if (!rearWheelsLocked)
          {
            lockedRearRightLocalRot = invCarRot * rearRightMesh.transform.rotation;
            rearWheelsLocked = true;
          }
          rearRightMesh.transform.SetPositionAndRotation(pos, transform.rotation * lockedRearRightLocalRot);
        }
        else
        {
          rearRightMesh.transform.SetPositionAndRotation(pos, rot * extraSpinRot);
        }
      }
      catch (Exception ex) { Debug.LogWarning(ex); }
    }

    public void GoForward()
    {
      ApplyNormalAndLaunchGrip();
      throttleAxis = Mathf.Min(1f, throttleAxis + Time.deltaTime * 3f);

      if (Mathf.RoundToInt(carSpeed) < maxSpeed)
      {
        float torque = (accelerationMultiplier * 50f) * throttleAxis;
        frontLeftCollider.brakeTorque = 0f;
        frontRightCollider.brakeTorque = 0f;
        rearLeftCollider.brakeTorque = 0f;
        rearRightCollider.brakeTorque = 0f;

        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
      }
      else ThrottleOff();
    }

    public void GoReverse()
    {
      ApplyNormalAndLaunchGrip();
      throttleAxis = Mathf.Max(-1f, throttleAxis - Time.deltaTime * 3f);

      if (localVelocityZ > 1.5f) Brakes();
      else if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
      {
        float torque = (accelerationMultiplier * 50f) * throttleAxis;
        frontLeftCollider.brakeTorque = 0f;
        frontRightCollider.brakeTorque = 0f;
        rearLeftCollider.brakeTorque = 0f;
        rearRightCollider.brakeTorque = 0f;

        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
      }
      else ThrottleOff();
    }

    public void ThrottleOff()
    {
      frontLeftCollider.motorTorque = 0f;
      frontRightCollider.motorTorque = 0f;
      rearLeftCollider.motorTorque = 0f;
      rearRightCollider.motorTorque = 0f;
    }

    public void DecelerateCar()
    {
      ApplyNormalAndLaunchGrip();

      if (throttleAxis != 0f)
      {
        throttleAxis = Mathf.MoveTowards(throttleAxis, 0f, Time.deltaTime * 10f);
        if (Mathf.Abs(throttleAxis) < 0.15f) throttleAxis = 0f;
      }

      carRigidbody.linearVelocity *= (1f / (1f + (0.025f * decelerationMultiplier)));
      ThrottleOff();

      if (carRigidbody.linearVelocity.magnitude < 0.25f)
      {
        carRigidbody.linearVelocity = Vector3.zero;
        CancelInvoke("DecelerateCar");
      }
    }

    public void Brakes()
    {
      brakeAxis = Mathf.Min(1f, brakeAxis + Time.deltaTime * brakeBuildSpeed);
      float currentBrake = brakeForce * brakeAxis;
      frontLeftCollider.brakeTorque = currentBrake;
      frontRightCollider.brakeTorque = currentBrake;
      rearLeftCollider.brakeTorque = currentBrake * 0.8f;
      rearRightCollider.brakeTorque = currentBrake * 0.8f;
    }

    public void ReleaseBrakes()
    {
      brakeAxis = 0f;
      frontLeftCollider.brakeTorque = 0f;
      frontRightCollider.brakeTorque = 0f;
      rearLeftCollider.brakeTorque = 0f;
      rearRightCollider.brakeTorque = 0f;
    }

    public void Handbrake()
    {
      isLaunchSpinning = false;
      launchBlend = 0f;
      postHandbrakeWheelspinTimer = 0.65f;
      CancelInvoke("RecoverTraction");
      driftingAxis = Mathf.Min(1f, driftingAxis + Time.deltaTime * 2.5f);
      isDrifting = Mathf.Abs(carSpeed) > 8f;

      float grip = Mathf.Lerp(roadGrip, handbrakeGrip, driftingAxis);

      RLwheelFriction.extremumSlip = RLWextremumSlip * (1f + (handbrakeDriftMultiplier * 0.35f * driftingAxis));
      RLwheelFriction.stiffness = RLWinitialStiffness * grip;
      rearLeftCollider.sidewaysFriction = RLwheelFriction;

      RRwheelFriction.extremumSlip = RRWextremumSlip * (1f + (handbrakeDriftMultiplier * 0.35f * driftingAxis));
      RRwheelFriction.stiffness = RRWinitialStiffness * grip;
      rearRightCollider.sidewaysFriction = RRwheelFriction;

      rearLeftCollider.motorTorque = 0f;
      rearRightCollider.motorTorque = 0f;
      float rearBrakeTorque = brakeForce * Mathf.Max(handbrakeBrakeStrength, 2.5f) * Mathf.Max(0.5f, driftingAxis);
      rearLeftCollider.brakeTorque = rearBrakeTorque;
      rearRightCollider.brakeTorque = rearBrakeTorque;

      isTractionLocked = true;
    }

    public void DriftCarPS()
    {
      if (useEffects)
      {
        try
        {
          if (isDrifting)
          {
            if (RLWParticleSystem != null && !RLWParticleSystem.isPlaying) RLWParticleSystem.Play();
            if (RRWParticleSystem != null && !RRWParticleSystem.isPlaying) RRWParticleSystem.Play();
          }
          else
          {
            if (RLWParticleSystem != null && RLWParticleSystem.isPlaying) RLWParticleSystem.Stop();
            if (RRWParticleSystem != null && RRWParticleSystem.isPlaying) RRWParticleSystem.Stop();
          }

          bool launchSkid = launchSpinEnabled && Mathf.Abs(carSpeed) > 0.6f && Mathf.Abs(carSpeed) < Mathf.Max(launchSpinSpeed * 2.5f, 65f) && isDrifting;
          bool emitSkid = (isTractionLocked && Mathf.Abs(carSpeed) > 10f) || launchSkid;
          if (RLWTireSkid != null) RLWTireSkid.emitting = emitSkid;
          if (RRWTireSkid != null) RRWTireSkid.emitting = emitSkid;
        }
        catch (Exception ex) { Debug.LogWarning(ex); }
      }
      else
      {
        if (RLWParticleSystem != null) RLWParticleSystem.Stop();
        if (RRWParticleSystem != null) RRWParticleSystem.Stop();
        if (RLWTireSkid != null) RLWTireSkid.emitting = false;
        if (RRWTireSkid != null) RRWTireSkid.emitting = false;
      }
    }

    public void RecoverTraction()
    {
      bool justReleasedHandbrake = isTractionLocked;
      isTractionLocked = false;
      rearWheelsLocked = false;
      if (justReleasedHandbrake)
      {
        postHandbrakeWheelspinTimer = 0.65f;
      }
      driftingAxis = Mathf.Max(0f, driftingAxis - Time.deltaTime / 0.65f);

      rearLeftCollider.brakeTorque = 0f;
      rearRightCollider.brakeTorque = 0f;

      if (driftingAxis > 0.01f)
      {
        float grip = Mathf.Lerp(roadGrip, handbrakeGrip, driftingAxis);

        RLwheelFriction.extremumSlip = RLWextremumSlip * (1f + (handbrakeDriftMultiplier * 0.35f * driftingAxis));
        RLwheelFriction.stiffness = RLWinitialStiffness * grip;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;

        RRwheelFriction.extremumSlip = RRWextremumSlip * (1f + (handbrakeDriftMultiplier * 0.35f * driftingAxis));
        RRwheelFriction.stiffness = RRWinitialStiffness * grip;
        rearRightCollider.sidewaysFriction = RRwheelFriction;

        Invoke("RecoverTraction", Time.deltaTime);
      }
      else
      {
        ApplyNormalAndLaunchGrip();
      }
    }

}