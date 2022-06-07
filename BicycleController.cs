using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;

// Please use using SBPScripts; directive to refer to or append the SBP library
namespace SBPScripts
{
    // Cycle Geometry Class - Holds Gameobjects pertaining to the specific bicycle
    [System.Serializable]
    public class CycleGeometry
    {
        public GameObject handles, lowerFork, fWheelVisual, RWheel, crank, lPedal, rPedal, fGear, rGear;
    }
    //Pedal Adjustments Class - Manipulates pedals and their positioning.  
    [System.Serializable]
    public class PedalAdjustments
    {
        public float crankRadius;
        public Vector3 lPedalOffset, rPedalOffset;
        public float pedalingSpeed;
    }
    // Wheel Friction Settings Class - Uses Physics Materials and Physics functions to control the 
    // static / dynamic slipping of the wheels 
    [System.Serializable]
    public class WheelFrictionSettings
    {
        public PhysicMaterial fPhysicMaterial, rPhysicMaterial;
        public Vector2 fFriction, rFriction;
    }
    // Way Point System Class - Replay Ghosting system
    [System.Serializable]
    public class WayPointSystem
    {
        public enum RecordingState { DoNothing, Record, Playback };
        public RecordingState recordingState = RecordingState.DoNothing;
        [Range(1, 10)]
        public int frameIncrement;
        [HideInInspector]
        public List<Vector3> bicyclePositionTransform;
        [HideInInspector]
        public List<Quaternion> bicycleRotationTransform;
        [HideInInspector]
        public List<Vector2Int> movementInstructionSet;
        [HideInInspector]
        public List<bool> sprintInstructionSet;
        [HideInInspector]
        public List<int> bHopInstructionSet;
    }
    [System.Serializable]
    public class AirTimeSettings
    {
        public bool freestyle;
        public float airTimeRotationSensitivity;
        [Range(0.5f, 10)]
        public float heightThreshold;
        public float groundSnapSensitivity;
    }
    public class BicycleController : MonoBehaviour
    {
        public CycleGeometry cycleGeometry;
        public GameObject fPhysicsWheel, rPhysicsWheel;
        public WheelFrictionSettings wheelFrictionSettings;
        // Curve of Power Exerted over Input time by the cyclist
        // This class sets the physics materials on to the
        // tires of the bicycle. F Friction pertains to the front tire friction and R Friction to
        // the rear. They are of the Vector2 type. X field edits the static friction
        // information and Y edits the dynamic friction. Please keep the values over 0.5.
        // For more information, please read the commented scripts.
        public AnimationCurve accelerationCurve;
        [Tooltip("Steer Angle over Speed")]
        public AnimationCurve steerAngle;
        public float axisAngle;
        // Defines the leaning curve of the bicycle
        public AnimationCurve leanCurve;
        // The slider refers to the ratio of Relaxed mode to Top Speed. 
        // Torque is a physics based function which acts as the actual wheel driving force.
        public float torque, topSpeed;
        [Range(0.1f, 0.9f)]
        [Tooltip("Ratio of Relaxed mode to Top Speed")]
        public float relaxedSpeed;
        public float reversingSpeed;
        public Vector3 centerOfMassOffset;
        [HideInInspector]
        public bool isReversing, isAirborne, stuntMode;
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
        [HideInInspector]
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
        public WayPointSystem wayPointSystem;
        public AirTimeSettings airTimeSettings;

        public float mass = 50;
        public float bikeMass = 6.9f;
        //Dreamteck Splines
        public SplineProjector follower;
        public SplineComputer splineComputer;
        //BikeValues
        public CentralSensor centralSensor;
        private VirtualTrainer virtualTrainer;
        public float currentSpeed;
        public FitnessEquipmentDisplay fitnessEquipmentDisplay;
       
        //for checking the slope only 0,5 seconds
        public float slope;
        private float nextActionTime = 0.2f;
        public float period = 0.2f;
        public float minValue = -20f;  //dont go under 20 we clamp the values between
        public float maxValue = 20f;   //dont go over 20

        public Vector3 currentPosition;


        void Awake()
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        void Start()
        {
            splineComputer = GameObject.FindGameObjectWithTag("SplinePath").GetComponent<SplineComputer>(); // The Path we move on
            centralSensor = GameObject.FindGameObjectWithTag("CentralSensor").GetComponent<CentralSensor>(); //the Central Sensor holds cadence, power....
            follower = GetComponent<SplineProjector>(); //We are the follower of Path
            follower.targetObject = GameObject.FindGameObjectWithTag("Player");
            fitnessEquipmentDisplay=  GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>();
            follower.spline = splineComputer;          //The Computer holds the PathData
            follower.motion.velocityHandleMode = TransformModule.VelocityHandleMode.Align;
            follower.motion.applyPositionY = false;   // DreamTeck Have no control over the Y Position
            follower.motion.applyPositionX = false;   // DreamTeck Have no control over the X Position
            follower.motion.applyPositionZ = false;   // DreamTeck Have no control over the Z Position
            follower.updateMethod = SplineUser.UpdateMethod.FixedUpdate; // Should solve the glitches
                                                      //  follower.motion.applyRotationY = true;    // We want that the Rotation is to the Path
                                                      //  follower.motion.applyRotationX = false;   // DreamTeck Have no control over the X Rotation
                                                      //  follower.motion.applyRotationZ = false;   // DreamTeck Have no control over the Z Rotation

            rb = GetComponent<Rigidbody>();
            rb.mass = PlayerPrefs.GetFloat("BikerWeight") + 6.9f;  //
            Debug.Log("Mass of Bike " + rb.mass);  //mayBe we can read the mass out of user setting and add to it, need to see what the physics say to it.

            rb.maxAngularVelocity = Mathf.Infinity;

            fWheelRb = fPhysicsWheel.GetComponent<Rigidbody>();
            fWheelRb.maxAngularVelocity = Mathf.Infinity;

            rWheelRb = rPhysicsWheel.GetComponent<Rigidbody>();
            rWheelRb.maxAngularVelocity = Mathf.Infinity;

            currentTopSpeed = topSpeed;

            initialHandlesRotation = cycleGeometry.handles.transform.localRotation;
            initialLowerForkLocalRotaion = cycleGeometry.lowerFork.transform.localRotation;

            fPhysicsWheelConfigJoint = fPhysicsWheel.GetComponent<ConfigurableJoint>();
            rPhysicsWheelConfigJoint = rPhysicsWheel.GetComponent<ConfigurableJoint>();

            //Recording is set to 0 to remove the recording previous data if not set to playback
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record || wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing)
            {
                wayPointSystem.bicyclePositionTransform.Clear();
                wayPointSystem.bicycleRotationTransform.Clear();
                wayPointSystem.movementInstructionSet.Clear();
                wayPointSystem.sprintInstructionSet.Clear();
                wayPointSystem.bHopInstructionSet.Clear();
            }

            virtualTrainer = new VirtualTrainer();
            float userWeight = PlayerPrefs.GetFloat("BikerWeight");   //80f; The BikerWeight is save in PlayerPrefs Try in Unity Menu "Window"AdvancedPlayerPrefs
            float bikeMass = 6.9f;
            virtualTrainer.SetUserSettings(userWeight, bikeMass);
        }

        void FixedUpdate()
        {
            // we transfer speed with the values of trainer devices of the central sensor 
            //The devices send Km/h so we need to convert to unity = m/s so we do a divide /3.6f 
            //if we want mph we can 1 Km/h = 0,62 mp/h
            //the central sensor speed / 3.6f (In UI Display we can convert to mp/h)
            // For that we need a Km/h and a Mp/h display if we want this.

            //for checking the slope only 0,2 seconds
            if (Time.time > nextActionTime)
            {
                nextActionTime += period;
                float slope = CheckSlopeGrade();
                float terrainFriction = 0.002f;
                float vehicleFrontalArea = 0.40f;
                float vehicleDragCoeff = 0.89f;
                float temperature = 20.0f + 273.15f;
                float groundElevation = 0.0f;
                float airPressure = (float)(101325.0f * Math.Pow(2.71f, (-(9.8f * 0.02896f * groundElevation) / (8.31447f * 288.15f))));
                float airDensity = airPressure / (287.05f * temperature);
                float windResCoeff = vehicleFrontalArea * vehicleDragCoeff * airDensity;
                float realWindSpeed_kmh = 0.0f;
                float draftingCoeff = 1.0f;
                virtualTrainer.SetTerrainResistance(slope, terrainFriction);
                virtualTrainer.SetWindResistance(windResCoeff * 0.5f, (sbyte)realWindSpeed_kmh, draftingCoeff);
                virtualTrainer.SetCadence((byte)centralSensor.cadence);
                virtualTrainer.SetPower((ushort)centralSensor.power);

                currentSpeed = virtualTrainer.GetSpeed();
                Debug.Log("Speed = " + currentSpeed * 3.6f + " km/h | Slope = " + slope);
            }

            //get the cadence out of the sensor for pedaling speed
            if (centralSensor)
            {
                pedalAdjustments.pedalingSpeed = centralSensor.cadence * 0.15f;
            }
            else
            {
                pedalAdjustments.pedalingSpeed = 10;
            }

           // transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime); This disable all Physics

            //Physics based Steering Control.
            fPhysicsWheel.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + customSteerAxis * steerAngle.Evaluate(rb.velocity.magnitude) + oscillationSteerEffect, 0);
            fPhysicsWheelConfigJoint.axis = new Vector3(1, 0, 0);

            //Power Control. Wheel Torque + Acceleration curves

            //cache rb velocity
            //float currentSpeed = rb.velocity.magnitude;

            if (!sprint)
                currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed * relaxedSpeed, Time.deltaTime);
            else
                currentTopSpeed = Mathf.Lerp(currentTopSpeed, topSpeed, Time.deltaTime);

            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0)
                rWheelRb.AddTorque(transform.right * torque * customAccelerationAxis);

            if (currentSpeed < currentTopSpeed && rawCustomAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
                //   rb.AddRelativeForce(transform.forward * currentSpeed - rb.velocity); cassio sets this active
                currentPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                rb.MovePosition(transform.position + (currentPosition * Time.fixedDeltaTime * currentSpeed));
            //rb.velocity = (transform.forward * currentSpeed); // Andreas sets this active
            //rb.AddForce(transform.forward * accelerationCurve.Evaluate(customAccelerationAxis));
            //rb.AddForce(transform.forward * centralSensor.power);
            //rb.AddForce(transform.forward * speed * 3.6f, ForceMode.VelocityChange);
            // Debug.Log("Move with Force" + centralSensor.power);


            if (currentSpeed < reversingSpeed && rawCustomAccelerationAxis < 0 && !isAirborne && !isBunnyHopping)
                rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 0.5f);

            if (transform.InverseTransformDirection(rb.velocity).z < 0)
                isReversing = true;
            else
                isReversing = false;

            if (rawCustomAccelerationAxis < 0 && isReversing == false && !isAirborne && !isBunnyHopping)
                rb.AddForce(-transform.forward * accelerationCurve.Evaluate(customAccelerationAxis) * 2);

            // Center of Mass handling
            if (stuntMode)
                rb.centerOfMass = GetComponent<BoxCollider>().center;
            else
                rb.centerOfMass = Vector3.zero + centerOfMassOffset;

            //Handles
            cycleGeometry.handles.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, 0) * initialHandlesRotation;

            //LowerFork
            cycleGeometry.lowerFork.transform.localRotation = Quaternion.Euler(0, customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, customSteerAxis * -axisAngle) * initialLowerForkLocalRotaion;

            //FWheelVisual
            xQuat = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            zQuat = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
            cycleGeometry.fWheelVisual.transform.rotation = Quaternion.Euler(xQuat * (customSteerAxis * -axisAngle), customSteerAxis * steerAngle.Evaluate(currentSpeed) + oscillationSteerEffect * 5, zQuat * (customSteerAxis * -axisAngle));
            cycleGeometry.fWheelVisual.transform.GetChild(0).transform.localRotation = cycleGeometry.RWheel.transform.rotation;

            //Crank
            crankCurrentQuat = cycleGeometry.RWheel.transform.rotation.eulerAngles.x;
            if (customAccelerationAxis > 0 && !isAirborne && !isBunnyHopping)
            {
                crankSpeed += Mathf.Sqrt(customAccelerationAxis * Mathf.Abs(Mathf.DeltaAngle(crankCurrentQuat, crankLastQuat) * pedalAdjustments.pedalingSpeed));
                crankSpeed %= 360;
            }
            else if (Mathf.Floor(crankSpeed) > restingCrank)
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
            if ((sprint && currentSpeed > 5 && isReversing == false) || isAirborne || isBunnyHopping)
                pickUpSpeed += Time.deltaTime * 2;
            else
                pickUpSpeed -= Time.deltaTime * 2;

            pickUpSpeed = Mathf.Clamp(pickUpSpeed, 0.1f, 1);

            cycleOscillation = -Mathf.Sin(Mathf.Deg2Rad * (crankSpeed + 90)) * (oscillationAmount * (Mathf.Clamp(currentTopSpeed / currentSpeed, 1f, 1.5f))) * pickUpSpeed;
            turnLeanAmount = -leanCurve.Evaluate(customLeanAxis) * Mathf.Clamp(currentSpeed * 0.1f, 0, 1);
            oscillationSteerEffect = cycleOscillation * Mathf.Clamp01(customAccelerationAxis) * (oscillationAffectSteerRatio * (Mathf.Clamp(topSpeed / currentSpeed, 1f, 1.5f)));

            //FrictionSettings
            wheelFrictionSettings.fPhysicMaterial.staticFriction = wheelFrictionSettings.fFriction.x;
            wheelFrictionSettings.fPhysicMaterial.dynamicFriction = wheelFrictionSettings.fFriction.y;
            wheelFrictionSettings.rPhysicMaterial.staticFriction = wheelFrictionSettings.rFriction.x;
            wheelFrictionSettings.rPhysicMaterial.dynamicFriction = wheelFrictionSettings.rFriction.y;

            if (Physics.Raycast(fPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velf = fPhysicsWheel.transform.InverseTransformDirection(fWheelRb.velocity);
                    velf.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.fFriction.x + wheelFrictionSettings.fFriction.y));
                    fWheelRb.velocity = fPhysicsWheel.transform.TransformDirection(velf);
                }
            if (Physics.Raycast(rPhysicsWheel.transform.position, Vector3.down, out hit, Mathf.Infinity))
                if (hit.distance < 0.5f)
                {
                    Vector3 velr = rPhysicsWheel.transform.InverseTransformDirection(rWheelRb.velocity);
                    velr.x *= Mathf.Clamp01(1 / (wheelFrictionSettings.rFriction.x + wheelFrictionSettings.rFriction.y));
                    rWheelRb.velocity = rPhysicsWheel.transform.TransformDirection(velr);
                }

            //Impact sensing
            deceleration = (fWheelRb.velocity - lastVelocity) / Time.fixedDeltaTime;
            lastVelocity = fWheelRb.velocity;
            impactFrames--;
            impactFrames = Mathf.Clamp(impactFrames, 0, 15);
            if (deceleration.y > 200 && lastDeceleration.y < -1)
                impactFrames = 30;

            lastDeceleration = deceleration;

            if (impactFrames > 0 && inelasticCollision)
            {
                fWheelRb.velocity = new Vector3(fWheelRb.velocity.x, -Mathf.Abs(fWheelRb.velocity.y), fWheelRb.velocity.z);
                rWheelRb.velocity = new Vector3(rWheelRb.velocity.x, -Mathf.Abs(rWheelRb.velocity.y), rWheelRb.velocity.z);
            }

            //AirControl
            if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), Vector3.down, out hit, Mathf.Infinity))
            {
                if (hit.distance > 1.5f || impactFrames > 0)
                {
                    isAirborne = true;
                    restingCrank = 100;
                }
                else if (isBunnyHopping)
                {
                    restingCrank = 100;
                }
                else
                {
                    isAirborne = false;
                    restingCrank = 10;
                }
                // For stunts
                // 5f is the snap to ground distance
                if (hit.distance > airTimeSettings.heightThreshold && airTimeSettings.freestyle)
                {
                    stuntMode = true;
                    // Stunt + flips controls (Not available for Waypoint system as of yet)
                    // You may use Numpad Inputs as well.
                    rb.AddTorque(Vector3.up * customSteerAxis * 4 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                    rb.AddTorque(transform.right * rawCustomAccelerationAxis * -3 * airTimeSettings.airTimeRotationSensitivity, ForceMode.Impulse);
                }
                else
                    stuntMode = false;
            }

            // Setting the Main Rotational movements of the bicycle
            if (airTimeSettings.freestyle)
            {
                if (!stuntMode && isAirborne)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.deltaTime * airTimeSettings.groundSnapSensitivity);
                else if (!stuntMode && !isAirborne)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity)), Time.deltaTime * 10 * airTimeSettings.groundSnapSensitivity);
            }
            else
            {
                //Pre-version 1.5
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, turnLeanAmount + cycleOscillation + GroundConformity(groundConformity));
            }


        }
        void Update()
        {
            ApplyCustomInput();

            //GetKeyUp/Down requires an Update Cycle
            //BunnyHopping
            if (bunnyHopInputState == 1)
            {
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
        float GroundConformity(bool toggle)
        {
            if (toggle)
            {
                groundZ = transform.rotation.eulerAngles.z;
            }
            return groundZ;

        }

        private float CheckSlopeGrade()
        {
            slope = GameObject.FindWithTag("Player").GetComponent<BicycleController>().slope;
           // Debug.Log("Slope is in %" + slope);
            if (fitnessEquipmentDisplay)
            {
                fitnessEquipmentDisplay.SetTrainerSlope(Mathf.RoundToInt(slope));
            }
            float grade = Mathf.Tan(transform.localEulerAngles.x * Mathf.Deg2Rad);
            float gradePercent = grade * 100f;
            slope = gradePercent * (-1);
            slope = Mathf.Clamp(slope, minValue, maxValue);
            return slope;
        }

        //changed for not need to press a button for accerlation
        void ApplyCustomInput()
        {
            if (wayPointSystem.recordingState == WayPointSystem.RecordingState.DoNothing || wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
            {
                CustomInput("Horizontal", ref customSteerAxis, 5, 5, false);
                //CustomInput("Vertical", ref customAccelerationAxis, 1, 1, false);
                CustomInput(1f, ref customAccelerationAxis, 1, 1, false); //1f simulates the button is always 1
                CustomInput("Horizontal", ref customLeanAxis, 1, 1, false);
                // CustomInput("Vertical", ref rawCustomAccelerationAxis, 1, 1, true);
                CustomInput(1f, ref rawCustomAccelerationAxis, 1, 1, true); //1f simulates the button is always 1

                sprint = Input.GetKey(KeyCode.LeftShift);

                //Stateful Input - bunny hopping
                if (Input.GetKey(KeyCode.Space))
                    bunnyHopInputState = 1;
                else if (Input.GetKeyUp(KeyCode.Space))
                    bunnyHopInputState = -1;
                else
                    bunnyHopInputState = 0;

                //Record
                if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Record)
                {
                    if (Time.frameCount % wayPointSystem.frameIncrement == 0)
                    {
                        wayPointSystem.bicyclePositionTransform.Add(new Vector3(Mathf.Round(transform.position.x * 100f) * 0.01f, Mathf.Round(transform.position.y * 100f) * 0.01f, Mathf.Round(transform.position.z * 100f) * 0.01f));
                        wayPointSystem.bicycleRotationTransform.Add(transform.rotation);
                        wayPointSystem.movementInstructionSet.Add(new Vector2Int((int)Input.GetAxisRaw("Horizontal"), (int)Input.GetAxisRaw("Vertical")));
                        wayPointSystem.sprintInstructionSet.Add(sprint);
                        wayPointSystem.bHopInstructionSet.Add(bunnyHopInputState);
                    }
                }
            }

            else
            {
                if (wayPointSystem.recordingState == WayPointSystem.RecordingState.Playback)
                {
                    if (wayPointSystem.movementInstructionSet.Count - 1 > Time.frameCount / wayPointSystem.frameIncrement)
                    {
                        transform.position = Vector3.Lerp(transform.position, wayPointSystem.bicyclePositionTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
                        transform.rotation = Quaternion.Lerp(transform.rotation, wayPointSystem.bicycleRotationTransform[Time.frameCount / wayPointSystem.frameIncrement], Time.deltaTime * wayPointSystem.frameIncrement);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customSteerAxis, 5, 5, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref customAccelerationAxis, 1, 1, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].x, ref customLeanAxis, 1, 1, false);
                        WayPointInput(wayPointSystem.movementInstructionSet[Time.frameCount / wayPointSystem.frameIncrement].y, ref rawCustomAccelerationAxis, 1, 1, true);
                        sprint = wayPointSystem.sprintInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
                        bunnyHopInputState = wayPointSystem.bHopInstructionSet[Time.frameCount / wayPointSystem.frameIncrement];
                    }
                }
            }
        }

        //Input Manager Controls  to have always a Value of 1f for vertical accerlation
        float CustomInput(string name, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var r = Input.GetAxisRaw(name);
            return CustomInput(r, ref axis, sensitivity, gravity, isRaw);
        }

        float CustomInput(float r, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

        /* float CustomInput(string name, ref float axis, float sensitivity, float gravity, bool isRaw)
         {
             var r = Input.GetAxisRaw(name);
             var s = sensitivity;
             var g = gravity;
             var t = Time.unscaledDeltaTime;

             if (isRaw)
                 axis = r;
             else
             {
                 if (r != 0)
                     axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                 else
                     axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
             }

             return axis;
         }*/

        float WayPointInput(float instruction, ref float axis, float sensitivity, float gravity, bool isRaw)
        {
            var r = instruction;
            var s = sensitivity;
            var g = gravity;
            var t = Time.unscaledDeltaTime;

            if (isRaw)
                axis = r;
            else
            {
                if (r != 0)
                    axis = Mathf.Clamp(axis + r * s * t, -1f, 1f);
                else
                    axis = Mathf.Clamp01(Mathf.Abs(axis) - g * t) * Mathf.Sign(axis);
            }

            return axis;
        }

        IEnumerator DelayBunnyHop()
        {
            yield return new WaitForSeconds(0.5f);
            isBunnyHopping = false;
            yield return null;
        }

    }
}

