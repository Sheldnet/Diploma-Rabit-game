using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]

    public class NewBehaviourScript : MonoBehaviour
    {
        private enum State
        {
            Normal,
            Jumping,
        }

        [SerializeField] private State _state;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private TrajectoryRenderer _traj;
        [SerializeField] private Transform _playerTrans;
        [SerializeField] private Transform _camTrans;
        [SerializeField] private Camera _cam;
        [SerializeField] private Animator _anim;
        [SerializeField] private float jumpAngle = 60f;

        private FrameInputs _inputs;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            _state = State.Normal;
        }

        private void Update()
        {

            switch (_state)
            {
                case State.Normal:

                    HandlerGrounding();
                    GatheringInputs();

                    if (isGrounded)
                    {
                        _traj.ShowTrajectory(transform.position, CalculateJumpForce(jumpAngle) + _rb.velocity.normalized);
                        HandleJumpInput();
                        HandleWalking();

                    }
                    break;

                case State.Jumping:
                    HandleAirMovementInput();
                    break;
            }
        }

        #region Walking

        [Header("Walking")]
        [SerializeField] private float _walkSpeed = 2;
        [SerializeField] private float _acceleration = 2;
        [SerializeField] private float _maxWalkingPenalty = 0.5f;
        [SerializeField] private float _currentMovementLerpSpeed = 100;
        //[SerializeField] private float _rotateSpeed = 0.5f;
        private float _currentWalkingPenalty;

        private float rotationSpeed;
        private float turnSmoothTime = 0.1f;

        private Vector3 _dir;

        private void HandleWalking()
        {
            if (_dir.magnitude > 0)
            {
                float targetAngle = Mathf.Atan2(_inputs.X, _inputs.Z) * Mathf.Rad2Deg + _camTrans.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;
                _currentMovementLerpSpeed = Mathf.MoveTowards(_currentMovementLerpSpeed, 100, _wallJumpMovementLerp * Time.deltaTime);
                _currentWalkingPenalty = Mathf.Clamp(_currentWalkingPenalty, _maxWalkingPenalty, 1);

                var targetVel = moveDir * _currentWalkingPenalty * _walkSpeed;
                var idealVel = new Vector3(targetVel.x, _rb.velocity.y, targetVel.z);
                _rb.velocity = Vector3.MoveTowards(_rb.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);
            }
        }


        private Vector3 CalculateJumpForce(float jumpAngle)
        {
            jumpAngleRad = jumpAngle * Mathf.Deg2Rad;

            Vector3 characterDirection = transform.forward;
            // Рассчитываем вектор прыжка с учетом угла и магнитуды
            Vector3 jumpDirection = Quaternion.AngleAxis(jumpAngle, transform.right) * characterDirection;
            Vector3 jumpForce = jumpDirection * jumpForceMagnitude;


            return jumpForce;
        }

        //private void HandleAirMovement()
        //{
        //    var airVelocity = new Vector3(_inputs.X * _walkSpeed, _rb.velocity.y, _inputs.Z * _walkSpeed);
        //    _rb.velocity = Vector3.Lerp(_rb.velocity, airVelocity, _currentMovementLerpSpeed * Time.deltaTime);
        //}

        #endregion

        #region Inputs

        [SerializeField] private float _airControlInterpolationFactor = 0.8f;

        private void GatheringInputs()
        {
            _inputs.RawX = (int)Input.GetAxisRaw("Horizontal");
            _inputs.X = Input.GetAxis("Horizontal");
            _inputs.RawZ = (int)Input.GetAxisRaw("Vertical");
            _inputs.Z = Input.GetAxis("Vertical");

            _dir = transform.TransformDirection(new Vector3(_inputs.X, 0, _inputs.Z));


            //if (_dir != Vector3.zero ) _anim.transform.forward = _dir;
            //_anim.SetInteger("RawZ", _inputs.RawZ);

            if (Input.GetAxis("Mouse ScrollWheel") > 0 && jumpAngle < maxPlusJumpAngle)
            {
                // Увеличиваем угол прыжка
                jumpAngle += jumpAngleIncrement;
                // Обновляем вектор прыжка
                jumpForceVector = CalculateJumpForce(jumpAngle);
                Debug.Log(jumpAngle);
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && jumpAngle > maxMinusJumpAngle)
            {
                // Уменьшаем угол прыжка
                jumpAngle -= jumpAngleIncrement;
                // Обновляем вектор прыжка
                jumpForceVector = CalculateJumpForce(jumpAngle);
                Debug.Log(jumpAngle);
            }

            //TODO: Анимация
        }

        #endregion

        #region Jumping

        [Header("Jumping")]
        [SerializeField] private float _wallJumpMovementLerp = 20;
        [SerializeField] private float _airControlFactor;
        [SerializeField] private float _jumpVelocityFalloff = 8;
        [SerializeField] private float _fallMultiplier = 7;

        private float jumpAngleRad;
        public float jumpForceMagnitude = 10f;
        public float jumpAngleIncrement = 5f; // Шаг изменения угла прыжка
        private Vector3 jumpForceVector;
        public float maxPlusJumpAngle; // Максимальный угол прыжка в градусах
        public float maxMinusJumpAngle; // Максимальный угол прыжка в градусах
        private Vector3 airMovementInput;

        private bool _isJumping = false;
        private bool _hasJumped;

        private void HandleAirMovementInput()
        {
            float airMovementInputX = Input.GetAxis("Horizontal");
            float airMovementInputZ = Input.GetAxis("Vertical");

            // Применить коррекцию к вектору движения в воздухе
            airMovementInput = _playerTrans.TransformDirection(new Vector3(airMovementInputX, 0f, airMovementInputZ));
            Vector3 targetVelocity = _rb.velocity + airMovementInput * _walkSpeed;
            _rb.velocity = Vector3.Lerp(_rb.velocity, targetVelocity, _airControlInterpolationFactor * Time.deltaTime);

            //if(!exitedCollision && !isGrounded)
            //{
            //    _state = State.Normal;
            //}
        }

        private void HandleJumpInput()
        {
            if (Input.GetButton("Fire1"))
            {
                if (isGrounded && !_hasJumped) // Добавляем проверку наличия столкновения с землей и флага _hasJumped
                {
                    jumpForceVector = CalculateJumpForce(jumpAngle);
                    _rb.AddForce(jumpForceVector, ForceMode.Impulse);
                    isGrounded = false;
                    _isJumping = true;
                    _traj.ShowTrajectory(transform.position, CalculateJumpForce(jumpAngle) + _rb.velocity);
                    _state = State.Jumping;

                    _hasJumped = true; // Устанавливаем флаг _hasJumped в true после выполнения прыжк
                }
            }
        }
        #endregion

        #region Detection

        [Header("Detection")][SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
        public bool isGrounded = true;
        private bool exitedCollision = false;


        public static event Action OnTouchedGround;

        private readonly Collider[] _ground = new Collider[1];

        private void HandlerGrounding()
        {
            var grounded = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, _grounderOffset), _grounderRadius, _ground, _groundMask) > 0;

            if (!isGrounded && grounded)
            {
                isGrounded = true;
                _hasJumped = false;
                OnTouchedGround?.Invoke();
                _traj.ClearTrajectory();
                jumpForceVector = Vector3.zero;
                _state = State.Normal;
            }
            else if (isGrounded && !grounded) // Добавляем условие проверки предыдущего значения isGrounded
            {
                isGrounded = false;
                transform.SetParent(null);
            }


        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            // Grounder
            Gizmos.DrawWireSphere(transform.position + new Vector3(0, _grounderOffset), _grounderRadius);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Проверяем, столкнулись ли с землей после прыжка
            if (collision.gameObject.CompareTag("Ground"))
            {
                _state = State.Normal;
                exitedCollision = false;

            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                exitedCollision = true;
            }
        }
        //private void OnCollisionStay(Collision collision)
        //{
        //    if (collision.gameObject.CompareTag("Ground"))
        //    {
        //        _state = State.Normal;
        //    }
        //}

        private struct FrameInputs
        {
            public float X, Z;
            public int RawX, RawZ;
        }

        #endregion


    }
}