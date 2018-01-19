/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[PrettyLog.Provider("Player", "movement", "green", "blue")]
[RequireComponent(typeof(BoxCollider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField]
    float speed = 3;
    [SerializeField] Vector2 drag = new Vector2(2, 4);
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float speedModifier = 0;
    bool _isMovingRight = false;
    [See]
    float Speed
    {
        get { return Mathf.Max(speed + speedModifier - _xDragModifier, 0) * (_isMovingRight ? 1 : -1); }
    }
    [See]
    public Vector2 FrameDeltaPos { get; private set; }
    [See]
    bool IsStopped
    {
        get { return Speed == 0; }
    }
    #endregion

    #region Action Settings
    [SerializeField] float _jumpStrength = 5f;
    [SerializeField] Transform _tossPos;
    [SerializeField] float _hitJumpSpeed = 3f;
    [SerializeField] ParticleSystem _dustFx;

    PlayerController _controller;
    bool _isPlayingActionAnim;
    #endregion

    #region Detectors
    [Header("Terrain Detecting Settings")]
    [SerializeField, Range(0, 90)]
    float maxClimbAngle = 60f;
    [SerializeField, Range(0, -90)] float minDropAngle = -60f;
    [SerializeField, Range(2, 6)] int verticalRayCount = 3;
    [SerializeField, Range(2, 6)] int horizontalRayCount = 3;
    /// <summary>
    /// Head space for wall detection - Ceiling higher than this value will be ignore and accessible
    /// </summary>
    [SerializeField] float upperRayMargin = .1f;
    /// <summary>
    /// Foot space for wall detection - Wall lower than this value will be ignored and accessible
    /// </summary>
    [SerializeField] float lowerRayMargin = .3f;

    BoxCollider2D __selfCollider;
    BoxCollider2D SelfCollider
    {
        get
        {
            if (__selfCollider == null)
            {
                __selfCollider = GetComponent<BoxCollider2D>();
            }
            return __selfCollider;
        }
    }

    Rect SelfColliderRect
    {
        get
        {
            var b = SelfCollider.bounds;
            return new Rect(b.min, b.size);
        }
    }

    IEnumerable<Vector2> VerticalRayOrigins(bool isBottom)
    {
        var rect = SelfColliderRect;
        var delta = (rect.xMax - rect.xMin) / (verticalRayCount - 1);
        for (int i = 0; i < verticalRayCount; i++)
        {
            var x = rect.xMin + i * delta;
            yield return new Vector2(x, isBottom ? rect.yMin : rect.yMax);
        }
    }

    IEnumerable<Vector2> HorizontalRayOrigins(bool isRight)
    {
        var rect = SelfColliderRect;
        var yMin = rect.yMin + lowerRayMargin;
        var yMax = rect.yMax - upperRayMargin;
        var delta = (yMax - yMin) / (horizontalRayCount - 1);
        for (int i = 0; i < horizontalRayCount; i++)
        {
            var y = yMin + i * delta;
            yield return new Vector2(isRight ? rect.xMax : rect.xMin, y);
        }
    }
    #endregion

    #region General Settings
    public bool showDebugInfo;
    #endregion

    #region Components

    Animator __anim;
    Animator Anim
    {
        get
        {
            if (__anim == null)
                __anim = GetComponent<Animator>();
            return __anim;
        }
    }
    #endregion

    #region Init
    public void Init(PlayerController controller)
    {
        _controller = controller;
    }

    public void Reset()
    {
        speedModifier = -speed;
        _isMovingRight = true;
        PutOnGround();
    }

    void PutOnGround()
    {
        var scr = SelfColliderRect;
        var offsetY2ObjectCenter = scr.center - transform.position.ToVector2();
        var bottomOrigin = new Vector2(scr.center.x, scr.yMin);
        var groundPos = StageUtility.FindGroundPosition(bottomOrigin);
        if (!VectorEx.IsNaN(groundPos))
        {
            var standPos = groundPos + Vector2.up * scr.height * .5f - offsetY2ObjectCenter;
            transform.position = standPos;
        }
    }
    #endregion

    #region Message Listener

    [See]
    public void Stop()
    {
        speedModifier = -speed;
        Anim.Play("PlayerIdle");
    }

    [See]
    public void Run()
    {
        speedModifier = 0;
        Anim.Play("PlayerRun");
    }

    [See]
    void Jump()
    {
        Jump(_jumpStrength);
    }

    void Jump(float speed)
    {
        _jumpSpeed = speed;
        _isJumping = true;
    }

    [See]
    void HitBack()
    {
        Jump(_hitJumpSpeed);
        Anim.Play("PlayerHit");
        _isMovingRight = false;
    }

    public IEnumerator HitBackCo()
    {
        HitBack();
        while (!_isMovingRight)
            yield return null;
        yield return null;
    }

    void LandOnGround()
    {
        _isMovingRight = true;
        _jumpSpeed = 0f;
        _isJumping = false;
        if (!_controller.IsDead)
        {
            if (Speed == 0)
                Stop();
            else
                Run();
        }
    }

    public void Fall(bool isAlive)
    {
        if (isAlive)
        {
            if (_isMovingRight)
            {
                Anim.Play("PlayerFallAlive");
            }
        }
        else
        {
            Anim.Play("PlayerFallDead");
        }
    }

    public void ChangeSpeed(float modifier)
    {
        speedModifier = modifier;
    }

    public IEnumerator DieCo()
    {
        yield return PlayActionAnim("PlayerDie", null);
    }

    public IEnumerator CastCo(int id)
    {
        //var potion = Potion.Make(potionData, Vector2.up * 2, false);
        //potion.transform.SetParent(_tossPos, false);
        //potion.gameObject.SetActive(true);
        //potion.transform.localPosition = Vector2.zero;
        yield return PlayActionAnim("PlayerCast", (normalizedTime, length) =>
        {
            //if (_castingPotion == null || _castingPotion != potion)
            //{
            //    _castingPotion = potion;
            //}
        });
    }

    public void OnCast()
    {
        //if (_castingPotion == null) return;
        //_castingPotion.transform.SetParent(null, true);
        //_castingPotion.Toss();
        //_castingPotion = null;
    }

    public void StopActionAnim()
    {
        _isPlayingActionAnim = false;
    }

    IEnumerator PlayActionAnim(string stateName, AnimationFrameCallback frameUpdate)
    {
        Anim.Play(stateName);
        yield return null;
        float nt = 0f;
        _isPlayingActionAnim = true;
        while (nt <= 1f && _isPlayingActionAnim)
        {
            var state = Anim.GetCurrentAnimatorStateInfo(0);
            nt = state.normalizedTime;
            if (frameUpdate != null)
            {
                frameUpdate(nt, state.length);
            }
            yield return null;
        }
        _isPlayingActionAnim = false;
    }

    #endregion

    #region Updating
    private void Update()
    {
        //UpdatePosition();
        UpdatePositionBetter();
        if (_controller.IsOnGround && Speed != 0)
        {
            if (!_dustFx.isPlaying) _dustFx.Play();
        }
        else if (_dustFx.isPlaying)
        {
            _dustFx.Stop();
        }
    }

    float _fallTime = 0f;
    float _xDragModifier = 0f;
    float _jumpSpeed = 0f;
    bool _isJumping;

    void UpdatePositionBetter()
    {
        var selfPos = transform.position;
        #region Vertical Test
        // Test if player is on ground
        var isOnGround = false;
        RaycastHit2D groundHit = new RaycastHit2D();
        Vector2 selfGroundPos = Vector2.zero;
        foreach (var bo in VerticalRayOrigins(true))
        {
            var hit = StageUtility.FindGroundSurface(bo);
            var groundCheck = hit.transform != null && hit.point.y >= bo.y;
            if (isOnGround && !groundCheck) break; // ignore the last mid-in-air rays
            isOnGround = groundCheck;
            if (isOnGround)
            {
                groundHit = hit;
                selfGroundPos = bo;
            }
        }
        float slopeAngle = 0f;
        float foot2Ground = 0f;

        if (isOnGround)
        {
            slopeAngle = Vector2.SignedAngle(Vector2.up, groundHit.normal);
            _fallTime = 0f;
            _xDragModifier = 0f;
            // adjust value to put player's feet on the ground (on the direction of surface normal)
            foot2Ground = Vector2.Dot(Vector2.up * (groundHit.point.y - selfGroundPos.y), groundHit.normal);
            if (!_controller.IsOnGround)
            {
                LandOnGround();
            }
        }
        else if (_controller.IsGravityEnabled)
        {
            _fallTime += Time.deltaTime;
            slopeAngle = -90f;
            _xDragModifier += drag.x * Time.deltaTime;
            if (_jumpSpeed > 0)
            {
                _jumpSpeed = Mathf.Max(_jumpSpeed - drag.y * Time.deltaTime, 0);
            }
        }
        var gSpeed = -gravity * _fallTime + _jumpSpeed; // the grav falling speed, v = gt (t is the accumulated falling time)
        #endregion

        #region Horizontal Test
        bool isWallBlocked = false;
        RaycastHit2D wallHit = new RaycastHit2D();
        foreach (var ro in HorizontalRayOrigins(_isMovingRight))
        {
            var hit = StageUtility.FindWallSurface(ro, _isMovingRight);
            if (hit.transform != null)
            {
                wallHit = hit;
                break;
            }
        }
        if (wallHit.transform != null)
        {
            var wallAngle = Vector2.Angle(Vector2.up, wallHit.normal);
            isWallBlocked = wallAngle >= maxClimbAngle;
        }
        #endregion

        var xSpeed = isWallBlocked ? 0f : Speed;
        Vector2 moveVec = Quaternion.Euler(0, 0, slopeAngle) * new Vector2(xSpeed, foot2Ground); // move speed along groud (if is on the ground)
        moveVec += Vector2.up * _jumpSpeed;
        Vector2 fallVec = new Vector2(xSpeed, gSpeed); // falling vector in the next second
        float fallAngle = Vector2.SignedAngle(Vector2.right, fallVec.normalized);
        Vector2 finalVec = (slopeAngle >= minDropAngle || slopeAngle > fallAngle) && moveVec.sqrMagnitude > 0 && !_isJumping ? moveVec : fallVec;
        if (showDebugInfo)
        {
            Debug.DrawRay(selfPos, moveVec.normalized, Color.blue);
            Debug.DrawRay(selfPos, fallVec.normalized, Color.red);
            Debug.DrawRay(selfPos, finalVec.normalized, Color.green);
        }
        var newPos = selfPos + finalVec.ToVector3() * Time.deltaTime;
        FrameDeltaPos = newPos - selfPos;
        transform.position = newPos;
        _controller.IsOnGround = isOnGround;
    }
    #endregion

    public delegate void AnimationFrameCallback(float normalizedTime, float length);
}
