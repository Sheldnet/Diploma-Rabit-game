using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float jumpAngle = 25f;

    private FrameInputs _inputs;

    [Header("Jumping")]
    [SerializeField] private float _wallJumpMovementLerp = 20;
    private bool _isJumpingForward;
    private float jumpAngleRad;
    public float jumpForceMagnitude = 10f;
    public float jumpAngleIncrement = 5f; // Шаг изменения угла прыжка
    private Vector3 jumpForceVector;
    private Vector3 savedJumpForceVector;
    //public float maxJumpAngle; // Максимальный угол прыжка в градусах

    [SerializeField] private TrajectoryRenderer _traj;

    private bool _isJumping = false;
    private bool _hasJumped;


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {

        GatheringInputs();
        HandlerGrounding();

        if (isGrounded)
        {
            _traj.ShowTrajectory(transform.position, CalculateJumpForce(jumpAngle));

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded)
                {
                    jumpForceVector = CalculateJumpForce(jumpAngle);
                    savedJumpForceVector = jumpForceVector;
                    _rb.AddForce(jumpForceVector, ForceMode.Impulse); ;
                    isGrounded = false;
                    _isJumping = true;
                    _traj.ShowTrajectory(transform.position, jumpForceVector);
                }
                else
                {
                    savedJumpForceVector = Vector3.zero; // Сбросить сохраненный вектор прыжка
                }
            }

            HandleWalking();
        }
        else
        {
            HandleAirMovement();
        }
    }

    #region Walking

    [Header("Walking")]
    [SerializeField] private float _walkSpeed = 2;
    [SerializeField] private float _acceleration = 2;
    [SerializeField] private float _maxWalkingPenalty = 0.5f;
    [SerializeField] private float _currentMovementLerpSpeed = 100;
    private float _currentWalkingPenalty;
    private Vector3 walkDirection;
    private bool _isWalking;

    private Vector3 _dir;

    private void HandleWalking()
    {
        _isWalking = _inputs.X != 0f || _inputs.Z != 0f;

        _currentMovementLerpSpeed = Mathf.MoveTowards(_currentMovementLerpSpeed, 100, _wallJumpMovementLerp * Time.deltaTime);

        var normalizedDir = walkDirection.normalized;

        if (_dir != Vector3.zero)
            _currentWalkingPenalty += _acceleration * Time.deltaTime;
        else
            _currentWalkingPenalty -= _acceleration * Time.deltaTime;

        _currentWalkingPenalty = Mathf.Clamp(_currentWalkingPenalty, _maxWalkingPenalty, 1);

        var targetVel = new Vector3(walkDirection.x, _rb.velocity.y, walkDirection.z) * _currentWalkingPenalty * _walkSpeed;

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

        _dir = new Vector3(_inputs.X, 0, _inputs.Z);
        walkDirection = new Vector3(_inputs.X, 0f, _inputs.Z);

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            // Увеличиваем угол прыжка
            jumpAngle += jumpAngleIncrement;
            // Обновляем вектор прыжка
            jumpForceVector = CalculateJumpForce(jumpAngle);
            Debug.Log(jumpAngle);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            // Уменьшаем угол прыжка
            jumpAngle -= jumpAngleIncrement;
            // Обновляем вектор прыжка
            jumpForceVector = CalculateJumpForce(jumpAngle);
            Debug.Log(jumpAngle);
        }

        if (!_isJumping && _inputs.Z > 0)
        {
            // Если мы не прыгаем и вводим положительное значение по оси Z,
            // то прыжок происходит вперед
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

        //TODO: Анимация
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
        // Рассчитываем вектор прыжка с учетом угла и магнитуды
        Vector3 jumpDirection = Quaternion.AngleAxis(jumpAngle, transform.right) * characterDirection;
        Vector3 jumpForce = jumpDirection * jumpForceMagnitude;


        return jumpForce;
    }

    private void JumpVectorSaver()
    {

    }
    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем, столкнулись ли с землей после прыжка
        if (collision.gameObject.CompareTag("Ground"))
        {
            savedJumpForceVector = Vector3.zero; // Сбрасываем сохраненный вектор прыжка
        }
    }
    private struct FrameInputs
    {
        public float X, Z;
        public int RawX, RawZ;
    }

}