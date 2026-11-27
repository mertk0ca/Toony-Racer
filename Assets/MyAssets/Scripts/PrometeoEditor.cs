using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrometeoCarController))]
[System.Serializable]
public class PrometeoEditor : Editor{

  private SerializedProperty maxSpeed;
  private SerializedProperty maxReverseSpeed;
  private SerializedProperty accelerationMultiplier;
  private SerializedProperty maxSteeringAngle;
  private SerializedProperty steeringSpeed;
  private SerializedProperty brakeForce;
  private SerializedProperty brakeBuildSpeed;
  private SerializedProperty decelerationMultiplier;
  private SerializedProperty handbrakeDriftMultiplier;
  private SerializedProperty bodyMassCenter;

  private SerializedProperty roadGrip;
  private SerializedProperty launchSpinEnabled;
  private SerializedProperty launchSpinSpeed;
  private SerializedProperty launchGrip;
  private SerializedProperty launchSteerKick;
  private SerializedProperty launchSpinVisualSpeed;
  private SerializedProperty launchBlendSpeed;
  private SerializedProperty counterSteerAssist;
  private SerializedProperty highSpeedSteerScale;
  private SerializedProperty handbrakeGrip;
  private SerializedProperty handbrakeBrakeStrength;
  private SerializedProperty handbrakeSpinLimit;

  private SerializedProperty frontLeftMesh;
  private SerializedProperty frontLeftCollider;
  private SerializedProperty frontRightMesh;
  private SerializedProperty frontRightCollider;
  private SerializedProperty rearLeftMesh;
  private SerializedProperty rearLeftCollider;
  private SerializedProperty rearRightMesh;
  private SerializedProperty rearRightCollider;

  private SerializedProperty useEffects;
  private SerializedProperty RLWParticleSystem;
  private SerializedProperty RRWParticleSystem;
  private SerializedProperty RLWTireSkid;
  private SerializedProperty RRWTireSkid;

  private SerializedProperty useUI;
  private SerializedProperty carSpeedText;

  private SerializedProperty useSounds;
  private SerializedProperty carEngineSound;
  private SerializedProperty tireScreechSound;

  private SerializedProperty useTouchControls;
  private SerializedProperty throttleButton;
  private SerializedProperty reverseButton;
  private SerializedProperty turnRightButton;
  private SerializedProperty turnLeftButton;
  private SerializedProperty handbrakeButton;

  private void OnEnable(){
    SerializedObject so = serializedObject;

    maxSpeed = so.FindProperty("maxSpeed");
    maxReverseSpeed = so.FindProperty("maxReverseSpeed");
    accelerationMultiplier = so.FindProperty("accelerationMultiplier");
    maxSteeringAngle = so.FindProperty("maxSteeringAngle");
    steeringSpeed = so.FindProperty("steeringSpeed");
    brakeForce = so.FindProperty("brakeForce");
    brakeBuildSpeed = so.FindProperty("brakeBuildSpeed");
    decelerationMultiplier = so.FindProperty("decelerationMultiplier");
    handbrakeDriftMultiplier = so.FindProperty("handbrakeDriftMultiplier");
    bodyMassCenter = so.FindProperty("bodyMassCenter");

    roadGrip = so.FindProperty("roadGrip");
    launchSpinEnabled = so.FindProperty("launchSpinEnabled");
    launchSpinSpeed = so.FindProperty("launchSpinSpeed");
    launchGrip = so.FindProperty("launchGrip");
    launchSteerKick = so.FindProperty("launchSteerKick");
    launchSpinVisualSpeed = so.FindProperty("launchSpinVisualSpeed");
    launchBlendSpeed = so.FindProperty("launchBlendSpeed");
    counterSteerAssist = so.FindProperty("counterSteerAssist");
    highSpeedSteerScale = so.FindProperty("highSpeedSteerScale");
    handbrakeGrip = so.FindProperty("handbrakeGrip");
    handbrakeBrakeStrength = so.FindProperty("handbrakeBrakeStrength");
    handbrakeSpinLimit = so.FindProperty("handbrakeSpinLimit");

    frontLeftMesh = so.FindProperty("frontLeftMesh");
    frontLeftCollider = so.FindProperty("frontLeftCollider");
    frontRightMesh = so.FindProperty("frontRightMesh");
    frontRightCollider = so.FindProperty("frontRightCollider");
    rearLeftMesh = so.FindProperty("rearLeftMesh");
    rearLeftCollider = so.FindProperty("rearLeftCollider");
    rearRightMesh = so.FindProperty("rearRightMesh");
    rearRightCollider = so.FindProperty("rearRightCollider");

    useEffects = so.FindProperty("useEffects");
    RLWParticleSystem = so.FindProperty("RLWParticleSystem");
    RRWParticleSystem = so.FindProperty("RRWParticleSystem");
    RLWTireSkid = so.FindProperty("RLWTireSkid");
    RRWTireSkid = so.FindProperty("RRWTireSkid");

    useUI = so.FindProperty("useUI");
    carSpeedText = so.FindProperty("carSpeedText");

    useSounds = so.FindProperty("useSounds");
    carEngineSound = so.FindProperty("carEngineSound");
    tireScreechSound = so.FindProperty("tireScreechSound");

    useTouchControls = so.FindProperty("useTouchControls");
    throttleButton = so.FindProperty("throttleButton");
    reverseButton = so.FindProperty("reverseButton");
    turnRightButton = so.FindProperty("turnRightButton");
    turnLeftButton = so.FindProperty("turnLeftButton");
    handbrakeButton = so.FindProperty("handbrakeButton");
  }

  public override void OnInspectorGUI(){
    serializedObject.Update();

    GUILayout.Space(25);
    GUILayout.Label("CAR SETUP", EditorStyles.boldLabel);
    GUILayout.Space(10);

    maxSpeed.intValue = EditorGUILayout.IntSlider("Max Speed:", maxSpeed.intValue, 20, 220);
    maxReverseSpeed.intValue = EditorGUILayout.IntSlider("Max Reverse Speed:", maxReverseSpeed.intValue, 10, 120);
    accelerationMultiplier.intValue = EditorGUILayout.IntSlider("Acceleration Multiplier:", accelerationMultiplier.intValue, 1, 10);
    maxSteeringAngle.intValue = EditorGUILayout.IntSlider("Max Steering Angle:", maxSteeringAngle.intValue, 10, 45);
    steeringSpeed.floatValue = EditorGUILayout.Slider("Steering Speed:", steeringSpeed.floatValue, 0.1f, 1f);
    brakeForce.intValue = EditorGUILayout.IntSlider("Brake Force:", brakeForce.intValue, 100, 600);
    brakeBuildSpeed.floatValue = EditorGUILayout.Slider("Brake Build Speed:", brakeBuildSpeed.floatValue, 0.5f, 10f);
    decelerationMultiplier.floatValue = EditorGUILayout.Slider("Deceleration Multiplier:", decelerationMultiplier.floatValue, 0.1f, 5f);
    handbrakeDriftMultiplier.intValue = EditorGUILayout.IntSlider("Drift Multiplier:", handbrakeDriftMultiplier.intValue, 1, 10);
    EditorGUILayout.PropertyField(bodyMassCenter, new GUIContent("Mass Center of Car: "));

    GUILayout.Space(25);
    GUILayout.Label("GTA 5 FEEL", EditorStyles.boldLabel);
    GUILayout.Space(10);

    roadGrip.floatValue = EditorGUILayout.Slider("Road Grip:", roadGrip.floatValue, 0.8f, 3.0f);
    EditorGUILayout.PropertyField(launchSpinEnabled, new GUIContent("Launch Spin:"));
    launchSpinSpeed.floatValue = EditorGUILayout.Slider("Launch Spin Speed:", launchSpinSpeed.floatValue, 4f, 35f);
    launchGrip.floatValue = EditorGUILayout.Slider("Launch Grip:", launchGrip.floatValue, 0.1f, 1.0f);
    launchSteerKick.floatValue = EditorGUILayout.Slider("Launch Steer Kick:", launchSteerKick.floatValue, 0f, 3.5f);
    launchSpinVisualSpeed.floatValue = EditorGUILayout.Slider("Launch Spin Visual Speed:", launchSpinVisualSpeed.floatValue, 0f, 2500f);
    launchBlendSpeed.floatValue = EditorGUILayout.Slider("Launch Blend Speed:", launchBlendSpeed.floatValue, 0.5f, 12f);
    counterSteerAssist.floatValue = EditorGUILayout.Slider("Counter Steer Assist:", counterSteerAssist.floatValue, 0f, 3f);
    highSpeedSteerScale.floatValue = EditorGUILayout.Slider("High Speed Steer Scale:", highSpeedSteerScale.floatValue, 0.2f, 1f);
    handbrakeGrip.floatValue = EditorGUILayout.Slider("Handbrake Grip:", handbrakeGrip.floatValue, 0.2f, 0.85f);
    handbrakeBrakeStrength.floatValue = EditorGUILayout.Slider("Handbrake Brake Strength:", handbrakeBrakeStrength.floatValue, 0.1f, 1.5f);
    handbrakeSpinLimit.floatValue = EditorGUILayout.Slider("Handbrake Spin Limit:", handbrakeSpinLimit.floatValue, 0.8f, 4.5f);

    GUILayout.Space(25);
    GUILayout.Label("WHEELS", EditorStyles.boldLabel);
    GUILayout.Space(10);

    EditorGUILayout.PropertyField(frontLeftMesh, new GUIContent("Front Left Mesh: "));
    EditorGUILayout.PropertyField(frontLeftCollider, new GUIContent("Front Left Collider: "));
    EditorGUILayout.PropertyField(frontRightMesh, new GUIContent("Front Right Mesh: "));
    EditorGUILayout.PropertyField(frontRightCollider, new GUIContent("Front Right Collider: "));
    EditorGUILayout.PropertyField(rearLeftMesh, new GUIContent("Rear Left Mesh: "));
    EditorGUILayout.PropertyField(rearLeftCollider, new GUIContent("Rear Left Collider: "));
    EditorGUILayout.PropertyField(rearRightMesh, new GUIContent("Rear Right Mesh: "));
    EditorGUILayout.PropertyField(rearRightCollider, new GUIContent("Rear Right Collider: "));

    GUILayout.Space(25);
    GUILayout.Label("EFFECTS", EditorStyles.boldLabel);
    GUILayout.Space(10);

    useEffects.boolValue = EditorGUILayout.BeginToggleGroup("Use effects (particle systems)?", useEffects.boolValue);
    GUILayout.Space(10);
      EditorGUILayout.PropertyField(RLWParticleSystem, new GUIContent("Rear Left Particle System: "));
      EditorGUILayout.PropertyField(RRWParticleSystem, new GUIContent("Rear Right Particle System: "));
      EditorGUILayout.PropertyField(RLWTireSkid, new GUIContent("Rear Left Trail Renderer: "));
      EditorGUILayout.PropertyField(RRWTireSkid, new GUIContent("Rear Right Trail Renderer: "));
    EditorGUILayout.EndToggleGroup();

    GUILayout.Space(25);
    GUILayout.Label("UI", EditorStyles.boldLabel);
    GUILayout.Space(10);

    useUI.boolValue = EditorGUILayout.BeginToggleGroup("Use UI (Speed text)?", useUI.boolValue);
    GUILayout.Space(10);
      EditorGUILayout.PropertyField(carSpeedText, new GUIContent("Speed Text (UI): "));
    EditorGUILayout.EndToggleGroup();

    GUILayout.Space(25);
    GUILayout.Label("SOUNDS", EditorStyles.boldLabel);
    GUILayout.Space(10);

    useSounds.boolValue = EditorGUILayout.BeginToggleGroup("Use sounds (car sounds)?", useSounds.boolValue);
    GUILayout.Space(10);
      EditorGUILayout.PropertyField(carEngineSound, new GUIContent("Car Engine Sound: "));
      EditorGUILayout.PropertyField(tireScreechSound, new GUIContent("Tire Screech Sound: "));
    EditorGUILayout.EndToggleGroup();

    GUILayout.Space(25);
    GUILayout.Label("TOUCH CONTROLS", EditorStyles.boldLabel);
    GUILayout.Space(10);

    useTouchControls.boolValue = EditorGUILayout.BeginToggleGroup("Use touch controls (mobile devices)?", useTouchControls.boolValue);
    GUILayout.Space(10);
      EditorGUILayout.PropertyField(throttleButton, new GUIContent("Throttle Button: "));
      EditorGUILayout.PropertyField(reverseButton, new GUIContent("Brakes/Reverse Button: "));
      EditorGUILayout.PropertyField(turnRightButton, new GUIContent("Turn Left Button: "));
      EditorGUILayout.PropertyField(turnLeftButton, new GUIContent("Turn Right Button: "));
      EditorGUILayout.PropertyField(handbrakeButton, new GUIContent("Handbrake Button: "));
    EditorGUILayout.EndToggleGroup();

    GUILayout.Space(10);
    serializedObject.ApplyModifiedProperties();
  }
}