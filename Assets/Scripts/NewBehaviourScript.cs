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
        private State state;

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private TrajectoryRenderer _traj;
        [SerializeField] private Transform _playerTrans;
        [SerializeField] private Transform _camTrans;
        [SerializeField] private Animator _anim;
        [SerializeField] private float jumpAngle = 25f;

        private FrameInputs _inputs;

        [Header("Jumping")]
        [SerializeField] private float _wallJumpMovementLerp = 20;
        private bool _isJumpingForward;
        private float jumpAngleRad;
        public float jumpForceMagnitude = 10f;
        public float jumpAngleIncrement = 5f; // ��� ��������� ���� ������
        private Vector3 jumpForceVector;
        private Vector3 savedJumpForceVector;
        public float maxPlusJumpAngle; // ������������ ���� ������ � ��������
        public float maxMinusJumpAngle; // ������������ ���� ������ � ��������



        private bool _isJumping = false;
        private bool _hasJumped;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            state = State.Normal;
        }

        private void Update()
        {
            switch (state)
            {
                case State.Normal:

                    GatheringInputs();
                    HandlerGrounding();

                    if (isGrounded)
                    {
                        _traj.ShowTrajectory(transform.position, CalculateJumpForce(jumpAngle) + _dir + _dir);

                        HandleJumpInput();
                        HandleWalking();
                    }
                    else
                    {
                        HandleAirMovement();
                    }
                    break;

                case State.Jumping:
                    break;
            }
        }




        private void HandleJumpInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded)
                {
                    jumpForceVector = CalculateJumpForce(jumpAngle) + _dir + _dir;
                    _rb.AddForce(jumpForceVector, ForceMode.Impulse); ;
                    isGrounded = false;
                    _isJumping = true;
                    _traj.ShowTrajectory(transform.position, jumpForceVector + (_dir / 2 ));
                    state = State.Jumping;
                }
                else
                {
                    savedJumpForceVector = Vector3.zero; // �������� ����������� ������ ������
                }
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
        private Vector3 walkDirection;
        private bool _isWalking;

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

                _isWalking = _inputs.X != 0f || _inputs.Z != 0f;

                _currentMovementLerpSpeed = Mathf.MoveTowards(_currentMovementLerpSpeed, 100, _wallJumpMovementLerp * Time.deltaTime);

                if (_dir != Vector3.zero)
                    _currentWalkingPenalty += _acceleration * Time.deltaTime;
                else
                    _currentWalkingPenalty -= _acceleration * Time.deltaTime;

                _currentWalkingPenalty = Mathf.Clamp(_currentWalkingPenalty, _maxWalkingPenalty, 1);

                var targetVel = moveDir * _currentWalkingPenalty * _walkSpeed;

                if (!_isJumping)
                {
                    targetVel += savedJumpForceVector;
                    savedJumpForceVector = Vector3.zero;
                }

                var idealVel = new Vector3(targetVel.x, _rb.velocity.y, targetVel.z);
                _rb.velocity = Vector3.MoveTowards(_rb.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);

                if (_isJumping)
                {
                    savedJumpForceVector = Vector3.zero;
                    _isJumping = false;
                }
            }
        }



        private void HandleAirMovement()
        {
            if (_isWalking)
            {
                var airVelocity = new Vector3(_inputs.X * _walkSpeed, _rb.velocity.y, _inputs.Z * _walkSpeed);
                _rb.velocity = Vector3.Lerp(_rb.velocity, airVelocity, _currentMovementLerpSpeed * Time.deltaTime);
            }
        }

        #endregion

        #region Inputs



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
                // ����������� ���� ������
                jumpAngle += jumpAngleIncrement;
                // ��������� ������ ������
                jumpForceVector = CalculateJumpForce(jumpAngle);
                Debug.Log(jumpAngle);
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && jumpAngle > maxMinusJumpAngle)
            {
                // ��������� ���� ������
                jumpAngle -= jumpAngleIncrement;
                // ��������� ������ ������
                jumpForceVector = CalculateJumpForce(jumpAngle);
                Debug.Log(jumpAngle);
            }

            if (!_isJumping && _inputs.Z > 0)
            {
                // ���� �� �� ������� � ������ ������������� �������� �� ��� Z,
                // �� ������ ���������� ������
                _isJumpingForward = true;
            }
            else
            {
                _isJumpingForward = false;
            }
            if (!isGrounded)
            {
                walkDirection = Vector3.zero;
            }

            //TODO: ��������
        }
        #endregion

        #region Detection


        [Header("Detection")][SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _grounderOffset = -1, _grounderRadius = 0.2f;
        public bool isGrounded = true;

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
                state = State.Normal;
            }
            else if (isGrounded && !grounded)
            {
                isGrounded = false;
                transform.SetParent(null);
            }
        }
        #endregion

        private Vector3 CalculateJumpForce(float jumpAngle)
        {
            jumpAngleRad = jumpAngle * Mathf.Deg2Rad;

            Vector3 characterDirection = transform.forward;
            // ������������ ������ ������ � ������ ���� � ���������
            Vector3 jumpDirection = Quaternion.AngleAxis(jumpAngle, transform.right) * characterDirection;
            Vector3 jumpForce = jumpDirection * jumpForceMagnitude;


            return jumpForce;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // ���������, ����������� �� � ������ ����� ������
            if (collision.gameObject.CompareTag("Ground"))
            {
                savedJumpForceVector = Vector3.zero; // ���������� ����������� ������ ������
                state = State.Normal;
            }
        }
        private struct FrameInputs
        {
            public float X, Z;
            public int RawX, RawZ;
        }
    }
}