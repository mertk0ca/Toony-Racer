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
      bool isLaunchSpinning = false;
      float launchSpinDirection = 1f;
      float launchBlend = 0f;
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

      if (carEngineSound != null) initialCarEngineSoundPitch = carEngineSound.pitch;

      if (useUI) InvokeRepeating("CarSpeedUI", 0f, 0.1f);
      else if (carSpeedText != null) carSpeedText.text = "0";

      if (useSounds) InvokeRepeating("CarSounds", 0f, 0.1f);
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

      if (useTouchControls && touchControlsSetup)
      {
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
        if (!turnLeftPTI.buttonPressed && !turnRightPTI.buttonPressed && steeringAxis != 0f) ResetSteeringAngle();
      }
      else
      {
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
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && steeringAxis != 0f) ResetSteeringAngle();
      }

      // El frenindeyken spin onleyici donus hizi siniri
      if (carRigidbody != null && isTractionLocked)
      {
        Vector3 angVel = carRigidbody.angularVelocity;
        angVel.y = Mathf.Clamp(angVel.y, -handbrakeSpinLimit, handbrakeSpinLimit);
        carRigidbody.angularVelocity = angVel;
      }

      // Gorsel arka teker donusu sadece launch spin durumuna bagli kalsin (blend hizini etkilemesin)
      if (!isTractionLocked && isLaunchSpinning && launchSpinVisualSpeed > 0f)
      {
        extraRearSpinAngle = (extraRearSpinAngle + launchSpinDirection * launchSpinVisualSpeed * Time.deltaTime) % 360f;
      }
      else
      {
        extraRearSpinAngle = 0f;
      }

      AnimateWheelMeshes();
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

      bool inLaunch = launchSpinEnabled && throttleHeld && absSpeed > 0.25f && absSpeed < launchSpinSpeed;
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

      // W veya S basilmiyorsa pati/spin dumani cikmasin
      isDrifting = throttleHeld && (inLaunch || (Mathf.Abs(localVelocityX) > 4.2f && absSpeed > 14f));

      RLwheelFriction.stiffness = RLWinitialStiffness * targetGrip;
      RLwheelFriction.extremumSlip = RLWextremumSlip * Mathf.Lerp(1f, 1.75f, launchBlend);
      rearLeftCollider.sidewaysFriction = RLwheelFriction;

      RRwheelFriction.stiffness = RRWinitialStiffness * targetGrip;
      RRwheelFriction.extremumSlip = RRWextremumSlip * Mathf.Lerp(1f, 1.75f, launchBlend);
      rearRightCollider.sidewaysFriction = RRwheelFriction;

      DriftCarPS();
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
          if (carEngineSound != null)
            carEngineSound.pitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);

          if (tireScreechSound != null)
          {
            if (isDrifting || (isTractionLocked && Mathf.Abs(carSpeed) > 12f))
            {
              if (!tireScreechSound.isPlaying) tireScreechSound.Play();
            }
            else if (tireScreechSound.isPlaying) tireScreechSound.Stop();
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
      steeringAxis = Mathf.Max(-1f, steeringAxis - Time.deltaTime * 10f * steeringSpeed);
      float angle = steeringAxis * maxSteeringAngle;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }

    public void TurnRight()
    {
      steeringAxis = Mathf.Min(1f, steeringAxis + Time.deltaTime * 10f * steeringSpeed);
      float angle = steeringAxis * maxSteeringAngle;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
      if (steeringAxis < 0f) steeringAxis = Mathf.Min(0f, steeringAxis + Time.deltaTime * 10f * steeringSpeed);
      else if (steeringAxis > 0f) steeringAxis = Mathf.Max(0f, steeringAxis - Time.deltaTime * 10f * steeringSpeed);
      if (Mathf.Abs(frontLeftCollider.steerAngle) < 1f) steeringAxis = 0f;

      float angle = steeringAxis * maxSteeringAngle;
      frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
      frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
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
      DriftCarPS();
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

          bool launchSkid = launchSpinEnabled && Mathf.Abs(carSpeed) > 0.6f && Mathf.Abs(carSpeed) < launchSpinSpeed && isDrifting;
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
      isTractionLocked = false;
      rearWheelsLocked = false;
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