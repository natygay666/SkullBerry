using System;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorEngine
{
    

    public class PlayerMovement : MonoBehaviour, IDeactivateWithActor
    {
        #region Movement Settings
        public interface IPlayerMovementSettings
        {
            public float GetFwdRate(PlayerMovement movement);
            public void GetRotation(PlayerMovement movement, out float sign, out float rate);
        }

        [System.Serializable]
        public class PlayerMovementTankSettings : IPlayerMovementSettings
        {
            [SerializeField] float m_RotationSpeed = 180f;
            [SerializeField] float m_MinInputMovementThreshold = 0.5f;
            [SerializeField] float m_MinInputRotationThreshold = 0.15f;

            public float GetFwdRate(PlayerMovement movement)
            {
                if (Mathf.Abs(movement.m_InputAxis.y) < m_MinInputMovementThreshold)
                    return 0f;

                return movement.m_InputAxis.y;
            }

            public void GetRotation(PlayerMovement movement, out float sign, out float rate)
            {
                if (Mathf.Abs(movement.m_InputAxis.x) < m_MinInputRotationThreshold)
                {
                    sign = 0f;
                    rate = 0f;
                    return;
                }

                sign = movement.m_InputAxis.x;
                rate = m_RotationSpeed;
            }
        }

        [System.Serializable]
        public class PlayerMovementAlternateSettings : IPlayerMovementSettings
        {
            [SerializeField] AnimationCurve m_RotationSpeedOverAngle;
            [SerializeField] float m_MinInputMovementThreshold = 0.5f;
            [SerializeField] float m_MinInputRotationThreshold = 0.15f;
            [SerializeField] float m_MinAngleRotationThreshold;
            [SerializeField] float m_InputUnlockAngleThreshold = 0.1f;

            private Behaviour m_LockedCam;
            private Behaviour m_LastRefreshedCam;
            private Vector2 m_LockedInput;

            public float GetFwdRate(PlayerMovement movement)
            {
                Vector3 moveAxis = movement.m_InputAxis;
                if (moveAxis.magnitude > 1f)
                    moveAxis.Normalize();

                Vector3 movementDir = CalculateDirFromCamera(movement.transform.position, moveAxis);
                Debug.DrawLine(movement.transform.position, movement.transform.position + movementDir * 5f, Color.blue);
                return Vector3.Dot(movement.transform.forward, movementDir);
            }

            public void GetRotation(PlayerMovement movement, out float sign, out float rate)
            {
                sign = 0;
                rate = 0;

                if (movement.m_InputAxis.magnitude < m_MinInputRotationThreshold)
                    return;

                Vector3 movementDir = CalculateDirFromCamera(movement.transform.position, movement.m_InputAxis);
                float signedAngle = Vector3.SignedAngle(movement.transform.forward, movementDir, Vector3.up);
                if (Mathf.Abs(signedAngle) > m_MinAngleRotationThreshold)
                {
                    sign = Mathf.Sign(signedAngle);
                    rate = m_RotationSpeedOverAngle.Evaluate(Mathf.Abs(signedAngle));
                }
            }

            private Vector3 CalculateDirFromCamera(Vector3 playerPos, Vector2 input)
            {
                // Ensure locked input is some direction for angle calculation
                if (m_LockedInput == Vector2.zero)
                    m_LockedInput = Vector2.up;

                bool inDifferentCam = false;
                var cam = CameraSystem.Instance.ActiveCamera;
                // Refresh locked input when entering a different camera to prevent an early change
                if (cam && cam != m_LastRefreshedCam) 
                {
                    m_LockedInput = input;
                    m_LastRefreshedCam = cam;
                    inDifferentCam = true;
                }

                // Update cam if the stick when the input stops or changes too much
                float angle = Vector2.Angle(input, m_LockedInput);
                if (input.sqrMagnitude < m_MinInputMovementThreshold || angle > m_InputUnlockAngleThreshold)
                {
                    m_LockedCam = m_LastRefreshedCam;
                    m_LockedInput = input;
                    inDifferentCam = false;
                }

                // No movement if input is not enough
                if (input.magnitude < m_MinInputMovementThreshold)
                    return Vector3.zero;

                Vector3 fwd = (m_LockedCam ? m_LockedCam.transform.forward : cam.transform.forward);
                fwd.y = 0;
                fwd.Normalize();
                Vector3 right = -Vector3.Cross(fwd, Vector3.up);
                right.Normalize();

                // Use locked input when in different cam to avoid incorrect small rotation
                Vector2 fixedInput = inDifferentCam ? m_LockedInput : input;
                Vector3 movementDir = right * fixedInput.x + fwd * fixedInput.y;

                return movementDir;
            }
        }
        #endregion

        public enum MovementType
        {
            Tank,
            Alternate
        }

        [Flags]
        public enum MovementConstrain
        {
            None = 0,
            Movement = 1,
            Rotation = 2
        }

        public MovementType Type = MovementType.Tank;

        [HideInInspector]
        public MovementConstrain Constrain = 0;

        [Header("Main settings")]
        [SerializeField] float m_MovementSpeed;
        [SerializeField] float m_MovementRunSpeed;
        [SerializeField] float m_MovementBackwardsSpeed;
        [SerializeField] float m_NavMeshCheckDistance;
        [SerializeField] bool m_AnalogRunning;

        [Header("Movement type settings")]
        [SerializeField] PlayerMovementTankSettings m_TankSettings;
        [SerializeField] PlayerMovementAlternateSettings m_AlternateSettings;

        [Header("Health Modifiers")]
        [SerializeField] bool m_ChangeWalkSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);
        [SerializeField] bool m_ChangeRunSpeedBasedOnHealth;
        [SerializeField] AnimationCurve m_NormalizedHealthRunSpeedScalar = AnimationCurve.Linear(0, 1f, 1f, 1f);

        private Rigidbody m_Rigidbody;
        private Vector2 m_InputAxis;
        private GroundDetector m_GroundDetector;
        private IPlayerInput m_Input;
        private bool m_Running;
        private Health m_Health;
        
        public Vector3 IntendedMovement { get; private set; }

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Input = GetComponent<IPlayerInput>();
            m_Health = GetComponent<Health>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_GroundDetector = GetComponent<GroundDetector>();
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            if (!m_AnalogRunning)
                m_Running = m_Input.IsRunHeld();

            m_InputAxis = m_Input.GetPrimaryAxis();

        }

        // --------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!Constrain.HasFlag(MovementConstrain.Movement))
                UpdateMovement();

            if (!Constrain.HasFlag(MovementConstrain.Rotation))
                UpdateRotation();
        }

        // --------------------------------------------------------------------

        private void UpdateRotation()
        {
            IPlayerMovementSettings settings;
            if (Type == MovementType.Tank || Constrain.HasFlag(MovementConstrain.Movement))
            {
                settings = m_TankSettings;
            }
            else
            {
                settings = m_AlternateSettings;
                
            }

            settings.GetRotation(this, out float sign, out float rate);
            if (rate > 0)
                Rotate(sign, rate);
        }

        // --------------------------------------------------------------------

        public void Rotate(float dir, float speed)
        {
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * Quaternion.Euler(Vector3.up * dir * Time.deltaTime * speed));
        }

        // --------------------------------------------------------------------

        private void UpdateMovement()
        {
            float fwd = 0f, absFwd = 0f;

            IPlayerMovementSettings settings = (Type == MovementType.Tank) ? m_TankSettings : m_AlternateSettings;

            fwd = settings.GetFwdRate(this);
            absFwd = Mathf.Abs(fwd);

            float speed = 0f;
            if (m_AnalogRunning)
            {
                if (fwd > Mathf.Epsilon)
                    speed = Mathf.Lerp(m_MovementSpeed * absFwd, m_MovementRunSpeed, absFwd);
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed * absFwd;
            }
            else
            {
                if (fwd > Mathf.Epsilon)
                    speed = m_Running ? m_MovementRunSpeed : m_MovementSpeed;
                else if (fwd < -Mathf.Epsilon)
                    speed = m_MovementBackwardsSpeed;
            }

            if (speed >= m_MovementRunSpeed && m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthRunSpeedScalar.Evaluate(m_Health.Normalized);
            else if (m_ChangeRunSpeedBasedOnHealth)
                speed *= m_NormalizedHealthSpeedScalar.Evaluate(m_Health.Normalized);

            Vector3 prevPos = m_Rigidbody.position;
            Vector3 newPos = prevPos + transform.forward * Time.deltaTime * speed * Mathf.Sign(fwd);

            // Correct the movement to check the ground slope
            if (m_GroundDetector.Detect(newPos))
            {
                Vector3 dir = (m_GroundDetector.Position - prevPos).normalized;
                
                IntendedMovement = dir * (m_AnalogRunning ? absFwd : 1f) * Time.deltaTime * speed;
                newPos = prevPos + IntendedMovement;
            }

            // Navmesh sliding
            if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, m_NavMeshCheckDistance, NavMesh.AllAreas))
            {
                Vector3 navMeshNewPos;
                if (m_GroundDetector.Detect(hit.position))
                {
                    navMeshNewPos = m_GroundDetector.Position;
                }
                else
                {
                    navMeshNewPos = hit.position;
                }

                //Check sliding direction
                Vector3 dirToNewPos = (newPos - prevPos).normalized;
                Vector3 dirToNavMeshNewPos = (navMeshNewPos - prevPos).normalized;
                float dot = Vector3.Dot(dirToNewPos, dirToNavMeshNewPos);
                if (dot > 0)
                {
                    newPos = prevPos + dirToNavMeshNewPos * Vector3.Distance(newPos, prevPos) * dot; // Scale using the dot to reduce speed at a perpendicular angle
                }
                else
                {
                    newPos = prevPos; // No sliding since it was going to be a backward movement
                }

            }
            
            m_Rigidbody.MovePosition(newPos);
        }


        // --------------------------------------------------------------------

        void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position - transform.right, transform.position + transform.right);
            Gizmos.DrawLine(transform.position - transform.forward, transform.position + transform.forward);
        }


        public void AddConstrain(MovementConstrain constrain) { Constrain |= constrain; }
        public void RemoveConstrain(MovementConstrain constrain) { Constrain &= ~constrain; }
    }
}