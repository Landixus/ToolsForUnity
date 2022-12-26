//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Dreamteck.Splines;
using System.Linq;
using UnityEngine.AzureSky;
using UnityEngine.EventSystems;



// Please use using SBPScripts; directive to refer to or append the SBP library
namespace SBPScripts {
    // Cycle Geometry Class - Holds Gameobjects pertaining to the specific bicycle
    [System.Serializable]
    public class CycleGeometry {
        public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
    }
    //Pedal Adjustments Class - Manipulates pedals and their positioning.  
    [System.Serializable]
    public class PedalAdjustments {
        public float crankRadius;
        public Vector3 lPedalOffset, rPedalOffset;
        public float pedalingSpeed;
    }
    // Wheel Friction Settings Class - Uses Physics Materials and Physics functions to control the 
    // static / dynamic slipping of the wheels 
    [System.Serializable]
    public class WheelFrictionSettings {
        public PhysicMaterial fPhysicMaterial, rPhysicMaterial;
        public Vector2 fFriction, rFriction;
    }

    [System.Serializable]
    public class AirTimeSettings {
        public bool freestyle;
        public float airTimeRotationSensitivity;
        [Range(0.5f, 10)]
        public float heightThreshold;
        public float groundSnapSensitivity;
    }

    public static class CustomGravity {

        public static Vector3 GetGravity(Vector3 position) {
            return Physics.gravity;
        }
    }
    //public class BicycleController : MonoBehaviour {
    public class BicycleController : NetworkBehaviour {
        public CycleGeometry cycleGeometry;
        public GameObject fPhysicsWheel, rPhysicsWheel;
        public WheelFrictionSettings wheelFrictionSettings;
        // Curve of Power Exerted over Input time by the cyclist
        // This class sets the physics materials on to the
        // tires of the bicycle. F Friction pertains to the front tire friction and R Friction to
        // the rear. They are of the Vector2 type. X field edits the static friction
        // information and Y edits the dynamic friction. Please keep the values over 0.5.
        // For more information, please read the commented scripts.
        // public AnimationCurve accelerationCurve;
        [Tooltip("Steer Angle over Speed")]
        public AnimationCurve steerAngle;
        public float axisAngle;
        // Defines the leaning curve of the bicycle
        public AnimationCurve leanCurve;

        // The slider refers to the ratio of Relaxed mode to Top Speed. 
        // Torque is a physics based function which acts as the actual wheel driving force.
        public float topSpeed;
        //  public float torque, topSpeed;
        [Range(0.1f, 0.9f)]
        [Tooltip("Ratio of Relaxed mode to Top Speed")]
        public float relaxedSpeed;
        //  public float reversingSpeed;
        public Vector3 centerOfMassOffset;
        [HideInInspector]
        //public bool isReversing, isAirborne, stuntMode;
        public bool isAirborne, stuntMode;
        // Controls Cycle sway from left to right.
        // The degree of cycle waddling side to side upon pedaling.
        // Higher values correspond to higher waddling. This property also affects
        // character IK. 

        [Range(0, 8)]
        public float oscillationAmount;
        // Following the natural movement of a cyclist, the
        // oscillation of the cycle from side to side also affects the steering to a certain
        // extent. This value refers to the counter steer upon cycle oscillation. Higher
        // values correspond to a higher percentage of the oscillation being transferred
        // to the steering handles. 

        [Range(0, 1)]
        public float oscillationAffectSteerRatio;
        float oscillationSteerEffect;
        [HideInInspector]
        public float cycleOscillation;
        [HideInInspector]
        public Rigidbody rb, fWheelRb, rWheelRb;
        float turnAngle;
        float xQuat, zQuat;
        [HideInInspector]
        public float crankSpeed, crankCurrentQuat, crankLastQuat, restingCrank;
        public PedalAdjustments pedalAdjustments;
        // [HideInInspector]
        public float turnLeanAmount;
        RaycastHit hit;
        [HideInInspector]
        public float customSteerAxis, customLeanAxis, customAccelerationAxis, rawCustomAccelerationAxis;
        bool isRaw, sprint;
        [HideInInspector]
        public int bunnyHopInputState;
        [HideInInspector]
        public float currentTopSpeed, pickUpSpeed;
        Quaternion initialLowerForkLocalRotaion, initialHandlesRotation;
        ConfigurableJoint fPhysicsWheelConfigJoint, rPhysicsWheelConfigJoint;
        // Ground Conformity refers to vehicles that do not need a gyroscopic force to keep them upright.
        // For non-gyroscopic wheel systems like the tricycle,
        // enabling ground conformity ensures that the tricycle is not always upright and
        // follows the curvature of the terrain. 
        public bool groundConformity;
        RaycastHit hitGround;
        Vector3 theRay;
        float groundZ;
        JointDrive fDrive, rYDrive, rZDrive;
        // Attempts to Reduce/eliminate bouncing of the bicycle after a fall impact 
        public bool inelasticCollision;
        [HideInInspector]
        public Vector3 lastVelocity, deceleration, lastDeceleration;
        int impactFrames;
        bool isBunnyHopping;
        [HideInInspector]
        public float bunnyHopAmount;
        // The upward force the rider can bunny hop with. 
        public float bunnyHopStrength;
        public AirTimeSettings airTimeSettings;
        public float UnityMass = 50;
        public float RealBikeMass = 6.9f;
        public float RealRiderMass = 50f;

        public float currentVehicleFrontalArea = 0;
        public float currentVehicleDragCoeff = 0;
        public float wheelFrictionAverage = 0.5f;
        //Dreamteck Splines
        public SplineProjector follower;
          public SplineComputer splineComputer;
        //BikeValues
        public CentralSensor centralSensor;
        private VirtualTrainer virtualTrainer;
        public float currentSpeed;
        public float targetSpeed;
        //Only for Debug without Trainer or Simulator
        public float debugSpeed = 0f;
        [Range(0f, 1f)]
        public float minSpeedMultiplier = 0.5f;
        public FitnessEquipmentDisplay fitnessEquipmentDisplay;
        private float nextActionTime = 0.2f;
        [HideInInspector]
        public float period = 0.2f;
        [HideInInspector]
        public float minValue = -20f;  //dont go under 20 we clamp the values between
        [HideInInspector]
        public float maxValue = 20f;   //dont go over 20
        //Value used to calculate maximum speed from the angle between spline samples
        //Higher values will allow higher speeds during curves
        public float maxSpeedAngleDivisor = 14;

        //distances and Heightmeter
        [HideInInspector]
        public float distanceStep = 0f;
        [HideInInspector]
        public float distanceTravelled = 0.001f;
        [HideInInspector]
        public float distanceToGo = 0.001f;
        [HideInInspector]
        private Vector3 lastYPosition;
        [HideInInspector]
        private Vector3 currentYPosition;
        [HideInInspector]
        public float totalClimb = 0.001f;
        public float heightMeter;
        [HideInInspector]
        public float lastHeightToGo;
        [HideInInspector]
        public float totalHeightToGo;
        [HideInInspector]
        public float RealHeightToGo = 0.001f;
        private Vector3 lastPosition;
        public List<float> pointers = new List<float>();
        public float sumHeightMeter;
        public float heighttogo;
        //Default value is 1, meaning it takes 1 second for the player to move from the middle to the edge on 1 side (WITHOUT ACCOUNTING FOR INPUT SMOOTHENING)
        public float playerHorizontalSpeed = 1;
        //Position shift value clamped between -1 and 1
        private float posShifter;
        //Mutliplier for the posshifter, determines how far away from the spline the bicycle can go

        public float windSpeed = 0.0001f;

        //Static so that is is shared between all bikes
        public static float posShiftMultiplier = 3;
        //Bike ComputerAndVAlueCounter
        public GameObject bikeComputer;
        public GameObject countAllValues;

        //AI Section
        public bool is_ai = false;
        [HideInInspector]
        public float smoothenedSlope = 0;
        [HideInInspector]
        public float slopeDivider = 1;

        public float MaxLeanForAngle = 15f;  //: Defines at what angle the player will fully lean into the corner
        [HideInInspector]                                     // public float MaxLean = 30f;
        public float LeanLookAhead = 2f;
        [HideInInspector]
        public float dampingTheLeanEffect = 0.2f;
        [Range(0, 2500)]
        public ushort DebugPower = 283;
        public float DebugCadence = 0;
        private float curPower;
        public bool DebugMODE = false;
        [HideInInspector] public static float playerPower;

        [InspectorButton("DebugStartMeasuring")]
        public bool DebugStartMeasure = false;
        private double _startMeasurePercent = 0d;
        private System.DateTime _startMeasureTime;

        [Range(0, 1)]
        public float AdaptivePowerInfluence = 0.8f;
        public GameObject roadNet;
        public int fec_divisor;
        [HideInInspector]
        public float speedBoost = 10;
        public GameObject nameTag;
        public GameObject wkg_tag;
        public GameObject TagNameWKg;

        private float targetXRotation;

        //  public Transform target;

        [HideInInspector]
        public GameObject ChatObject;
      //  public NetPlayerList netPlayerList;

        [HideInInspector] public bool isNetworked = false;
        [HideInInspector] public NetworkVariable<float> networkCadence = new NetworkVariable<float>();
        [HideInInspector] public NetworkVariable<float> networkSteerAxis = new NetworkVariable<float>();
        [HideInInspector] public NetworkVariable<float> networkSpeed = new NetworkVariable<float>();

        [ServerRpc]
        public void SetCadenceServerRpc(float cadence) {
            networkCadence.Value = cadence;
        }
        [ServerRpc]
        public void SetSteerAxisServerRpc(float steerAxis) {
            networkSteerAxis.Value = steerAxis;
        }
        //Make sure this is called if the commented out codes setting the current speed are re-introduced
        [ServerRpc]
        public void SetSpeedServerRpc(float speed) {
            networkSpeed.Value = speed;
        }

        public static Vector3 GetUpAxis(Vector3 position) {
            return position.normalized;
        }
        public static Vector3 GetGravity(Vector3 position) {
            return position.normalized * Physics.gravity.y;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis) {
            upAxis = position.normalized;
            return upAxis * Physics.gravity.y;
        }

        public bl_MiniMapEntity minimap_ent;

        void Awake() {
            if (isNetworked) {
                return;
            }
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            rb = GetComponent<Rigidbody>();
            //   rb.useGravity = false;

        }

        public override void OnNetworkSpawn() {
            //Incase the nametag is not assigned on the network bicycle script
            if (nameTag != null) {
                GetComponent<NetworkBicycle>().nameTag = nameTag;
                GetComponent<NetworkBicycle>().wkg = wkg_tag;
                
                

            }

            if (!IsOwner) {
                gameObject.tag = "NPlayer";
                follower = GetComponent<SplineProjector>();
                Destroy(follower);

                foreach (Transform child in gameObject.transform) {
                    if (child.tag == "MainCamera") {
                        Destroy(child.gameObject);
                    }
                }
     
                isNetworked = true;
                this.enabled = true;
                minimap_ent.enabled = true;
                

            } else {
                if (nameTag != null) {

                    //  TagNameWKg;
                    //    Destroy(nameTag);
                    //    Destroy(wkg_tag);
                    //    Destroy(TagNameWKg);
                    TagNameWKg.SetActive(false);
                    
                }
                LoadCharacter.singleton.InitializeSpecificPlayer(gameObject);
            }

            //   base.OnNetworkSpawn();
        }

        void Start()
        {
            if (!is_ai && GetComponent<NetworkBicycle>() == null)
            {
                Debug.LogError("Bicyle prefab should have network bicycle script added");
            }
            DebugMODE = false;
            DebugCadence = 0f;
            topSpeed = 80f;

          //  InvokeRepeating("ChangeWind", 2.0f, 600f);
            /*
            if (IsOwner)
            {
            netPlayerList = GameObject.Find("PlayerList").GetComponent<NetPlayerList>();

            netPlayerList.joinedPlayers.Add(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
               Debug.Log("We add some to a list someWhere");
            }
            */


            // rb.maxAngularVelocity = Mathf.Infinity;

            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            //fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            //  rWheelRb.maxAngularVelocity = Mathf.Infinity;

            currentTopSpeed = topSpeed;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            initialHandlesRotation = cycleGeometry.handles.transform.localRotation;
            initialLowerForkLocalRotaion = cycleGeometry.lowerFork.transform.localRotation;
            if (isNetworked)
            {
                return;
            }
            if (GlobalValues.GetGameMode() == GlobalValues.GameMode.Single)
            {
                Destroy(GetComponent<NetworkBicycle>());
                if (nameTag != null)
                {
                    TagNameWKg.SetActive(false);
                    /*
                    Destroy(nameTag);
                    Destroy(wkg_tag);
                    Destroy(TagNameWKg);
                    */
                }
            }
            rb = GetComponent<Rigidbody>();
            //   rb.maxAngularVelocity = Mathf.Infinity;
            rb.constraints = RigidbodyConstraints.FreezeRotationZ;
            splineComputer = GameObject.FindGameObjectWithTag("SplinePath").GetComponent<SplineComputer>(); // The Path we move on
            centralSensor = GameObject.FindGameObjectWithTag("CentralSensor").GetComponent<CentralSensor>(); //the Central Sensor holds cadence, power....
            follower = GetComponent<SplineProjector>(); //We are the follower of Path

            if (PlayerPrefs.GetInt("FEC_CHOOSED").ToString() == "1")
            {
                fitnessEquipmentDisplay = GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>();
            }
            follower.spline = splineComputer;          //The Computer holds the PathData
            follower.motion.velocityHandleMode = TransformModule.VelocityHandleMode.Align;
            follower.updateMethod = SplineUser.UpdateMethod.FixedUpdate; // Should solve the glitches
            if (!is_ai)
            {
                follower.motion.applyPositionX = false;
                follower.motion.applyPositionY = false;
                follower.motion.applyPositionZ = false;

            }
            else
            {
                follower.motion.applyPositionX = true;
                follower.motion.applyPositionY = false;
                follower.motion.applyPositionZ = false;


                posShifter = Random.Range(-1f, 1f);
            }
            //FindBikeComputerAndActivate it
            if (!is_ai)
            {
                bikeComputer = GameObject.FindGameObjectWithTag("BikeComputer");
                bikeComputer.GetComponent<BikeComputerUI>().enabled = true;
            }
            if (!is_ai)
            {
                countAllValues = GameObject.FindGameObjectWithTag("CAV");

                countAllValues.GetComponent<CountAllValues>().enabled = true;
            }
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<BicycleCamera>().enabled = true;
            GameObject.FindGameObjectWithTag("AZT").GetComponent<AzureTimeController>().enabled = true;
            GameObject.FindGameObjectWithTag("TVP").GetComponent<tvpanel>().enabled = true;
            //  Instantiate(ChatObject, new Vector3(0, 0, 0), Quaternion.identity);


            // GameObject.FindGameObjectWithTag("CamSwitch").GetComponent<CameraSwitcher>().enabled = true;

            // GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MSCameraController>().enabled = true;
            /*  if (!is_ai) {
                  roadNet = GameObject.FindGameObjectWithTag("RoadNet");
                  roadNet.SetActive(false);
                  roadNet.SetActive(true);
              }*/


            //points for calculations
            foreach (var point in splineComputer.GetPoints())
            {
                var yPosition = point.position.y;
                pointers.Add(yPosition);
            }
            float sumHeightMeter = pointers.Sum();
            heighttogo = Mathf.Tan(sumHeightMeter / splineComputer.CalculateLength()) * 100;
            heighttogo = heighttogo * (-1);
            // Debug.Log("HeightTOGOO" + heighttogo);
            //  autoBreakSampleCount = 0;
            //  autoBreakSamplePercentLength = 0.001f;
            topSpeed = 80f;

            // rb.maxAngularVelocity = Mathf.Infinity;

            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            //fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            //  rWheelRb.maxAngularVelocity = Mathf.Infinity;

            currentTopSpeed = topSpeed;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            virtualTrainer = new VirtualTrainer();
            float userWeight = PlayerPrefs.GetFloat("BikerWeight");   //80f; The BikerWeight is save in PlayerPrefs Try in Unity Menu "Window"AdvancedPlayerPrefs
            float bikeMass = RealBikeMass;
            virtualTrainer.SetUserSettings(userWeight, bikeMass);
            SetPower(0);
            //for distance 
            LoadCharacter.singleton.PositionToSpawn(gameObject);
            lastPosition = transform.position;

        }
        private void DebugStartMeasuring() {
            _startMeasurePercent = follower.result.percent;
            _startMeasureTime = System.DateTime.Now;
        }

        //Method used to set the current power level of the bicycle
        public void SetPower(float power) {
            curPower = power;
            virtualTrainer.SetPower((ushort)curPower);
        }

        //Fixed update to run only when networked and not the owner
        private void NetworkedFixedUpdate()
        {
          //  if (EventSystem.current.currentSelectedGameObject != null) return;

            pedalAdjustments.pedalingSpeed = networkCadence.Value / 20f;

            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (true) {
                if (networkCadence.Value == 255) {
                    crankSpeed += 75 / 5f;
                    crankSpeed %= 360;
                } else {
                    crankSpeed += networkCadence.Value / 5f;
                    crankSpeed %= 360;
                }

            }
     

            crankLastQuat = crankCurrentQuat;
            cycleGeometry.crank.transform.localRotation = Quaternion.Euler(crankSpeed, 0, 0);
            cycleGeometry.lPedal.transform.localPosition = pedalAdjustments.lPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius);
            cycleGeometry.rPedal.transform.localPosition = pedalAdjustments.rPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius);
            //Handles
            cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, networkSteerAxis.Value * steerAngle.Evaluate(networkSpeed.Value) /*+ oscillationSteerEffect * 5*/, 0) * initialHandlesRotation;
            //LowerFork
            cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, networkSteerAxis.Value * steerAngle.Evaluate(networkSpeed.Value) /*+ oscillationSteerEffect * 5*/, networkSteerAxis.Value * -axisAngle) * initialLowerForkLocalRotaion;
            //FWheelVisual
            xQuat = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            zQuat = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler(xQuat * (networkSteerAxis.Value * -axisAngle), networkSteerAxis.Value * steerAngle.Evaluate(networkSpeed.Value) /*+ oscillationSteerEffect * 5*/, zQuat * (networkSteerAxis.Value * -axisAngle));
            cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;
        }

        void FixedUpdate() {
            if (isNetworked) {
                NetworkedFixedUpdate();
                return;
            }

         //   if (EventSystem.current.currentSelectedGameObject != null) return;
            UpdateSlopeGrade();

            // Need to be tested
            float sloper = PlayerPrefs.GetInt("Sloper");
            float slope = smoothenedSlope /sloper; // / slopeDivider;
            
            if (Time.time > nextActionTime) {
                nextActionTime += period;
                playerPower = centralSensor.power;

                if (!is_ai) {
                    if (!DebugMODE) {
                        //For real
                        playerPower = centralSensor.power;
                        SetPower(playerPower);
                    }
                    //for Debug
                    else {
                        playerPower = DebugPower;
                        SetPower(playerPower);
                    }
                }

                float terrainFriction = 0.002f;
                float vehicleFrontalArea = currentVehicleFrontalArea; // 0.46 should be less, because of the vehicleDragCoeff 0.1 race 0.2 upright
                //Debug.Log("FrontalArea"+ currentVehicleFrontalArea);
                float vehicleDragCoeff = currentVehicleDragCoeff; //was 0.89f Race upright is 1.1
                float temperature = 20.0f + 273.15f;
                float groundElevation = 0.0f;
                float airPressure = (float)(101325.0f * Mathf.Pow(2.71f, (-(9.8f * 0.02896f * groundElevation) / (8.31447f * 288.15f))));
                float airDensity = airPressure / (287.05f * temperature);
                float windResCoeff = vehicleFrontalArea * vehicleDragCoeff * airDensity;
                float realWindSpeed_kmh = windSpeed;  // 12 = 30KmH at 150W
              //  Debug.Log("WindSpeed" + windSpeed.ToString("F0"));
                float draftingCoeff = 1.0f;
                virtualTrainer.SetTerrainResistance(slope, terrainFriction);
            //    Debug.Log("Slope is?" + slope);
                virtualTrainer.SetWindResistance(windResCoeff * 0.5f, (sbyte)realWindSpeed_kmh, draftingCoeff);
                virtualTrainer.SetCadence((byte)centralSensor.cadence);
            }

            if (smoothenedSlope > 0.5) {

                slopeDivider = 1;

            } else {
                slopeDivider = 1;
                //  Debug.Log("Slope is 2?" + slopeDivider);
            }
            float maxSpeed = 80;
            maxSpeed /= 3.6f;
            targetSpeed = Mathf.Min(virtualTrainer.GetSpeed(), maxSpeed);
            
          
            //  Debug.Log("RB_Speed" + rb.velocity.magnitude * 3.6f); // / 3.6f);
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, 0.9f); //15f * (Time.fixedDeltaTime * 200));
            if (GlobalValues.GetGameMode() != GlobalValues.GameMode.Single && currentSpeed != networkSpeed.Value) {
                SetSpeedServerRpc(currentSpeed);
            }

            //get the cadence out of the sensor for pedaling speed
            //for Real Mode
            if (centralSensor && DebugMODE == false) {
                if (GlobalValues.GetGameMode() != GlobalValues.GameMode.Single && centralSensor.cadence != networkCadence.Value) {
                    SetCadenceServerRpc(centralSensor.cadence);
                }
                else
                {
                    DebugCadence = centralSensor.cadence;
                }
                pedalAdjustments.pedalingSpeed = centralSensor.cadence / 20f; //* 0.15f;
            }
            //DebugMode
           if (DebugMODE)
            {
                if (GlobalValues.GetGameMode() != GlobalValues.GameMode.Single && DebugCadence != networkCadence.Value) {
                    SetCadenceServerRpc(DebugCadence);
                }
                pedalAdjustments.pedalingSpeed = DebugCadence / 20f; //* 0.15f;
            } else {
                pedalAdjustments.pedalingSpeed = 7;
            }
            //How many distance we have traveled after start
            var speed = currentSpeed;
            if (speed > 0.25f) {
                distanceTravelled += Vector3.Distance(transform.position, lastPosition);
                distanceTravelled += distanceStep;
                lastPosition = transform.position;
                //Debug.Log("DistanceTraveled" + distanceTravelled);
            }
            //How many heightMeter we have climbed
            currentYPosition = transform.position;

            if (currentYPosition.y > lastYPosition.y)
                totalClimb += currentYPosition.y - lastYPosition.y;
            heightMeter = currentYPosition.y;
            lastYPosition = currentYPosition;
            totalHeightToGo = heighttogo - (totalClimb % heighttogo);
            // Debug.Log("TOTAL_HEIGHT2GO" + totalHeightToGo);

            //Physics based Steering Control.
            fPhysicsWheel.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + customSteerAxis * steerAngle.Evaluate(rb.velocity.magnitude) + oscillationSteerEffect, 0);
            fPhysicsWheelConfigJoint.axis = new Vector3(1, 0, 0);

            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0 && !isAirborne && !isBunnyHopping) {

                float boost = Mathf.Clamp(Vector3.Dot(transform.forward, Vector3.up), 0, 1) * speedBoost;
                rb.velocity = follower.result.forward * (currentSpeed + boost);


                // Adaptive velocity
                var realSpeed = distanceStep / Time.fixedDeltaTime;
                var speedDelta = targetSpeed - realSpeed;
                if (Mathf.Abs(speedDelta) < 10f) {

                    if (realSpeed > 0.001f && targetSpeed > 0.001f) {
                        var adaptiveSpeed = Mathf.Lerp(currentSpeed, targetSpeed + speedDelta, AdaptivePowerInfluence);
                        rb.velocity = follower.result.forward * (adaptiveSpeed + boost);
                    }

                } else {
                //Debug.LogWarning($"Massive speed delta: Target is {targetSpeed * 3.6f:F2} km/h while the actual speed is {realSpeed * 3.6f:F2} km/h!");
                }


                //Debug.Log($"targetSpeed: {targetSpeed * 3.6f:F2} km/h");
                //Debug.Log($"realSpeed: {realSpeed * 3.6f:F2} km/h");
                //Debug.Log($"speedDelta: {speedDelta * 3.6f:F2} km/h");
               //Debug.Log($"rb.velocity: {rb.velocity.magnitude * 3.6f:F2} km/h");
            }
            Vector3 newTransform = new Vector3(0, transform.position.y, 0);
            Vector3 followerPos = follower.result.position;
            followerPos += (transform.right * posShifter * posShiftMultiplier);
            newTransform.x = Mathf.Lerp(transform.position.x, followerPos.x, 0.3f);
            newTransform.z = Mathf.Lerp(transform.position.z, followerPos.z, 0.3f);
            transform.position = newTransform;
            rb.centerOfMass = Vector3.zero + centerOfMassOffset;
            //Handles
            cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) /*+ oscillationSteerEffect * 5*/, 0) * initialHandlesRotation;

            //LowerFork
            cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) /*+ oscillationSteerEffect * 5*/, customSteerAxis * -axisAngle) * initialLowerForkLocalRotaion;

            //FWheelVisual
            xQuat = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            zQuat = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler(xQuat * (customSteerAxis * -axisAngle), customSteerAxis * steerAngle.Evaluate(currentSpeed) /*+ oscillationSteerEffect * 5*/, zQuat * (customSteerAxis * -axisAngle));
            cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;

            //Crank
            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (customAccelerationAxis > 0 && !isAirborne && !isBunnyHopping) {
 
                if (centralSensor.cadence == 255) {
                    crankSpeed += 75 / 5f; 
                    crankSpeed %= 360;
                }
                else
                {
                    //Real
                 //debugCAdence
                    crankSpeed += DebugCadence / 5f;
                    crankSpeed %= 360;
                }

            } else if (Mathf.Floor(crankSpeed) > restingCrank)
                crankSpeed += -6;
            else if (Mathf.Floor(crankSpeed) < restingCrank)
                crankSpeed = Mathf.Lerp(crankSpeed, restingCrank, Time.deltaTime * 5);

            crankLastQuat = crankCurrentQuat;
            cycleGeometry.crank.transform.localRotation = Quaternion.Euler(crankSpeed, 0, 0);

            //Pedals
            cycleGeometry.lPedal.transform.localPosition = pedalAdjustments.lPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 180)) * pedalAdjustments.crankRadius);
            cycleGeometry.rPedal.transform.localPosition = pedalAdjustments.rPedalOffset + new Vector3(0, Mathf.Cos(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius, Mathf.Sin(Mathf.Deg2Rad * (crankSpeed)) * pedalAdjustments.crankRadius);

            //FGear
            if (cycleGeometry.fGear != null)
                cycleGeometry.fGear.transform.rotation = cycleGeometry.crank.transform.rotation;
            //RGear
            if (cycleGeometry.rGear != null)
                cycleGeometry.rGear.transform.rotation = rPhysicsWheel.transform.rotation;

            //CycleOscillation
            if ((centralSensor.power > 450 && currentSpeed > 5) || isAirborne || isBunnyHopping)
                pickUpSpeed += Time.deltaTime * 2;
            else
                pickUpSpeed -= Time.deltaTime * 2;

            pickUpSpeed = Mathf.Clamp(pickUpSpeed, 0.1f, 1);

            //FrictionSettings
            wheelFrictionSettings.fPhysicMaterial.staticFriction = wheelFrictionSettings.fFriction.x;
            wheelFrictionSettings.fPhysicMaterial.dynamicFriction = wheelFrictionSettings.fFriction.y;
            wheelFrictionSettings.rPhysicMaterial.staticFriction = wheelFrictionSettings.rFriction.x;
            wheelFrictionSettings.rPhysicMaterial.dynamicFriction = wheelFrictionSettings.rFriction.y;

            if (Physics.Raycast(fPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f) {
                    Vector3 velf = fPhysicsWheel.transform.InverseTransformDirection(fWheelRb.velocity);
                    velf.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.fFriction.x + wheelFrictionSettings.fFriction.y));
                    fWheelRb.velocity = fPhysicsWheel.transform.TransformDirection(velf);
                }
            if (Physics.Raycast(rPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f) {
                    Vector3 velr = rPhysicsWheel.transform.InverseTransformDirection(rWheelRb.velocity);
                    velr.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.rFriction.x + wheelFrictionSettings.rFriction.y));
                    rWheelRb.velocity = rPhysicsWheel.transform.TransformDirection(velr);
                }
          
            //AirControl
            if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), Vector3.down, out hit, Mathf.Infinity)) {
                if (hit.distance > 1.5f || impactFrames > 0) {
                    isAirborne = true;
                    restingCrank = 100;
                } else if (isBunnyHopping) {
                    restingCrank = 100;
                } else {
                    isAirborne = false;
                    restingCrank = 10;
                }
                // For stunts
                // 5f is the snap to ground distance
                if (hit.distance > airTimeSettings.heightThreshold && airTimeSettings.freestyle) {
                    stuntMode = true;
                    // Stunt + flips controls (Not available for Waypoint system as of yet)
                    // You may use Numpad Inputs as well.
                    rb.AddTorque(Vector3.up * customSteerAxis * 4 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                    rb.AddTorque(transform.right * rawCustomAccelerationAxis * -3 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                } else
                    stuntMode = false;
            }
            //limit Rotation to 0
            Vector3 _angularVelocity = rb.transform.InverseTransformVector(rb.angularVelocity);

            _angularVelocity.z = 0f;
            rb.angularVelocity = _angularVelocity;

            if (smoothenedSlope > 5) {
                rb.useGravity = false;
                rb.AddForce(CustomGravity.GetGravity(rb.position), ForceMode.Acceleration);
            } else { rb.useGravity = true; }



        }
        public bool stopRider = false;

        void Update() {
            if (isNetworked) {
                return;
            }
/*
           target = GameObject.FindGameObjectWithTag("Player").transform; 

            Vector3 targetDir = target.position - transform.position;
            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
            Debug.Log("Angle" + angle);
            */

            if (stopRider == true)
        {
                currentSpeed = 0;
                targetSpeed = 0;
                SetPower(0);
                playerPower = 0;

                    
        }
          
          //  if (EventSystem.current.currentSelectedGameObject != null) return;
            ApplyCustomInput();

            //GetKeyUp/Down requires an Update Cycle
            //BunnyHopping
            if (bunnyHopInputState == 1) {
                isBunnyHopping = true;
                bunnyHopAmount += Time.deltaTime * 8f;
            }
            if (bunnyHopInputState == -1)
                StartCoroutine(DelayBunnyHop());

            if (bunnyHopInputState == -1 && !isAirborne)
                rb.AddForce(transform.up * bunnyHopAmount * bunnyHopStrength, ForceMode.VelocityChange);
            else
                bunnyHopAmount = Mathf.Lerp(bunnyHopAmount, 0, Time.deltaTime * 8f);

            bunnyHopAmount = Mathf.Clamp01(bunnyHopAmount);
        }
        float GroundConformity(bool toggle) {
            if (toggle) {
                groundZ = transform.rotation.eulerAngles.z;
            }
            return groundZ;

        }

        private void UpdateSlopeGrade() {
            //if slope longer present > 1 second { do the function below else the slope is 0, so we have no small hickups}
            // Debug.Log("Slope is in %" + slope);
            float targetSlope = 0;
            if (!is_ai && fitnessEquipmentDisplay) {
                fitnessEquipmentDisplay.SetTrainerSlope(Mathf.RoundToInt(smoothenedSlope / bikeComputer.GetComponent<BikeComputerUI>().fecDivide.value));
               // Debug.Log("Slope Send is" + (Mathf.RoundToInt(smoothenedSlope / bikeComputer.GetComponent<BikeComputerUI>().fecDivide.value))); 
            }
            float grade = Mathf.Tan(transform.localEulerAngles.x * Mathf.Deg2Rad);
            float gradePercent = grade * 100f; // / 2 for better feeling
            targetSlope = gradePercent * (-1);
            targetSlope = Mathf.Clamp(targetSlope, minValue, maxValue);
            smoothenedSlope = Mathf.Lerp(smoothenedSlope, targetSlope, 0.1f);

        }

        //Calculates the steer axis between -1 and 1 using the dreamteck spline
        /* private float GetSteerAxisFromSpline() {
             float steerAxis = 0;
             if (splineComputer == null || follower == null) {
                 return 0;
             }
             var forwardA = follower.result.forward;
             var forwardB = transform.forward;
             var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
             var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
             steerAxis = -Mathf.DeltaAngle(angleA, angleB);
             if (steerAngleDivisor != 0) {
                 steerAxis /= steerAngleDivisor;
             }
             return steerAxis;
         }*/

        private float CalculateLeanFromSpline(float lookAheadDistance) {
            if (splineComputer == null || follower == null) { return 0; }

            if (lookAheadDistance < 0.001f) return 0;

            // Move along the spline to find the next percent, with support for looping
            double next = follower.Travel(follower.result.percent, lookAheadDistance, Spline.Direction.Forward, out float moved);
            if (moved < lookAheadDistance - 0.001f) next = follower.Travel(0, lookAheadDistance - moved, Spline.Direction.Forward);

            // Get the turn angle
            Vector3 currentDirection = follower.result.forward;
            var target = follower.Evaluate(next);
            Vector3 targetDirection = target.forward;
            float angle = -Vector3.SignedAngle(currentDirection, targetDirection, follower.result.up);

            // Map the angle to lerp alpha
            // public float MaxLeanForAngle: Defines at what angle the player will fully lean into the corner
            float alpha = Mathf.Clamp(angle / MaxLeanForAngle, -1f, 1f);

            // public float MaxLean: Defines the full Lean angle for the player 
            return leanCurve.Evaluate(currentSpeed) * alpha;

        }

        //changed for not need to press a button for accerlation
        void ApplyCustomInput() {
            // if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            // {
            //CustomInput("Horizontal", ref customSteerAxis, 5, 5, false);
            //CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
            CustomInput(1f, ref customAccelerationAxis, 1, 1, false); //1f simulates the button is always 1
                                                                      //CustomInput("Horizontal", ref customLeanAxis, 1, 1, false);
                                                                      // CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);
            CustomInput(1f, ref rawCustomAccelerationAxis, 1, 1, true); //1f simulates the button is always 1

            //STEER AXIS OVERRIDE
            /* float steerAxis = GetSteerAxisFromSpline();
             steerAxis = Mathf.Clamp(steerAxis, -1, 1);
             realSteerAxis = Mathf.Lerp(realSteerAxis, steerAxis, 0.12f * (Time.fixedDeltaTime * 50));  //0.2
             customSteerAxis = realSteerAxis;
             */
            // customLeanAxis = Mathf.Clamp(realSteerAxis * (steerAngleDivisor / leanAngleDivisor), -1, 1);

            //PLAYER MOVE LEFT RIGHT
            if (!is_ai) {
                shiftPos(Input.GetAxis("Horizontal") * Time.deltaTime * playerHorizontalSpeed);
            }

            sprint = Input.GetKey(KeyCode.LeftShift);

            //Stateful Input - bunny hopping
            if (Input.GetKey(KeyCode.Space))
                bunnyHopInputState = 1;
            else if (Input.GetKeyUp(KeyCode.Space))
                bunnyHopInputState = -1;
            else
                bunnyHopInputState = 0;
            //}
        }

        //Changes the posShifter by the certain amount while keeping it clamped between -1 and 1
        public void shiftPos(float amount) {
            posShifter = Mathf.Clamp(posShifter + amount, -1, 1);
        }

        //Input Manager Controls  to have always a Value of 1f for vertical accerlation
        float CustomInput(string name, ref float axis, float sensitivity, float gravity, bool isRaw) {
            var r = Input.GetAxisRaw(name);
            return CustomInput(r, ref axis, sensitivity, gravity, isRaw);
        }

        float CustomInput(float r, ref float axis, float sensitivity, float gravity, bool isRaw) {
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

       
        public void LateUpdate()
        {
            if (isNetworked) {
                return;
            }

            if (Input.GetKey(KeyCode.T))
            {
                stopRider = !stopRider;
            }

            Quaternion targetRot = Quaternion.Euler(follower.result.rotation.eulerAngles.x, follower.result.rotation.eulerAngles.y, transform.eulerAngles.z);
            targetRot = Quaternion.Lerp(transform.rotation, targetRot, 0.2f);
            Quaternion leanRot = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y, CalculateLeanFromSpline(LeanLookAhead * currentSpeed));
            transform.rotation = Quaternion.Lerp(targetRot, leanRot, dampingTheLeanEffect);
           
            //test to clamp
            /*
            var targetRotation = Quaternion.Euler(Vector3.right * targetXRotation);
            targetXRotation = Mathf.Clamp(targetXRotation, -3, 3);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1 * Time.deltaTime);
            */


        }

        IEnumerator DelayBunnyHop() {
            yield return new WaitForSeconds(0.5f);
            isBunnyHopping = false;
            yield return null;
        }

        public void ChangeWind()
        {
          //  windSpeed = windSpeed + Random.Range(-20, 20);
         //   Debug.Log ("Wind is" + windSpeed);
        }


    }
}
