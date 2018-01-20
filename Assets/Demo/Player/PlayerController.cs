/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers;
using DG.Tweening;
using Player.BehaviourTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    public int hp = 100;
    [SerializeField] PlayerTree _tree;

    [SerializeField] Transform _endingPoint;
    [SerializeField] Transform _stopPoint;
    [SerializeField] Transform _drinkPos;

    [SerializeField] GameObject _deadNote;
    [SerializeField] GameObject _resumeTip;

    PlayerMovement _mover;

    #region Status
    PlayerPosture _posture = PlayerPosture.Empty;
    bool _isStatusDirty = true;

    [ReadOnly, SerializeField]
    int _currentHp;
    int CurrentHp
    {
        get
        {
            return _currentHp;
        }
        set
        {
            bool changed = _currentHp != value;
            _currentHp = value;
            if (changed)
            {
                Messenger.Broadcast(Msg.PlayerStatusChanged, CurrentHpRatio);
            }
        }
    }
    float CurrentHpRatio
    {
        get { return (float)CurrentHp / hp; }
    }
    public bool IsDead
    {
        get
        {
            return CurrentHp <= 0;
        }
    }

    public bool IsOnGround
    {
        get { return _tree.IsOnGround; }
        set
        {
            if (value != _tree.IsOnGround)
            {
                SendBehaviourRequest(new PlayerBlackboard { isOnGround = value });
            }
        }
    }

    public bool IsGravityEnabled
    {
        get { return _tree.IsGravityEnabled; }
    }
    #endregion

    #region Init
    private void Awake()
    {
        _mover = GetComponent<PlayerMovement>();
        _mover.Init(this);
        Messenger.AddListener<PlayerOrder>(Msg.Player.Order, OnReceiveOrder);
    }

    private void Start()
    {
        Spawn();
    }
    #endregion

    private void Update()
    {
        UpdatePosture();
        UpdateInput();
        if (_endingPoint != null && transform.position.x >= _endingPoint.position.x)
        {
            Camera.main.GetComponent<CameraController>().target = null;
        }
        if (_stopPoint != null && transform.position.x >= _stopPoint.position.x)
        {
            Stop();
        }
    }

    #region Control
    void UpdateInput()
    {
        // Reset
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Spawn();
            return;
        }
        if (IsDead)
        {
            return;
        }
        // Action
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TossPotion(0);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Damage(1);
        }
        else if (Input.GetKey(KeyCode.B))
        {
            Damage(99999);
        }
        // Move
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Run();
        }
        else if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            Stop();
        }
    }
    #endregion

    #region Requests
    void SendBehaviourRequest(PlayerBlackboard request, bool immediateAction = false)
    {
        if (request == null || request.IsEmpty) return;
        _tree.WriteOnBlackboard(request);
        if (request.action != null && immediateAction)
        {
            StopPlayingAction();
        }
    }

    [See]
    void Spawn()
    {
        transform.position = Vector3.zero;
        _currentHp = hp;
        StopPlayingAction();
        Stop();
    }

    void Run()
    {
        SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Running });
    }

    void Stop()
    {
        SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Idle });
    }

    void Damage(int dmg)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Max(0, CurrentHp - dmg);
        if (CurrentHp <= 0)
        {
            Die();
        }
        else
        {
            LoseHp();
        }
    }

    public void Die()
    {
        StopPlayingAction();
        SendBehaviourRequest(new PlayerBlackboard
        {
            posture = PlayerPosture.Dead,
        }, true);
    }

    public void LoseHp()
    {
        StopPlayingAction();
        SendBehaviourRequest(new PlayerBlackboard
        {
            action = new PlayerAction(
                PlayerActionType.Hit,
                OnAction(() => _mover.HitBackCo()),
                null,
                () => _actionCoroutine != null
            )
        }, true);
    }

    void TossPotion(int id)
    {
        SendBehaviourRequest(new PlayerBlackboard
        {
            action = new PlayerAction(
                PlayerActionType.Cast,
                OnAction(() => _mover.CastCo(id)),
                null,
                () => _actionCoroutine != null
            )
        });
    }

    #endregion

    #region Orders

    void OnReceiveOrder(PlayerOrder order)
    {
        if (order.posture.HasValue)
        {
            OnPostureChanged(order.posture.Value);
        }
        if (order.actionCallback != null)
            order.actionCallback();
    }

    void OnPostureChanged(PlayerPosture newPosture)
    {
        if (_posture == newPosture) return;
        _posture = newPosture; // Status will be updated in the next frame
        _isStatusDirty = true;
    }
    void UpdatePosture()
    {
        if (_actionCoroutine != null) return;
        if (!_isStatusDirty) return;
        _isStatusDirty = false;
        switch (_posture)
        {
            case PlayerPosture.Idle:
                _mover.Stop();
                break;
            case PlayerPosture.Running:
                _mover.Run();
                break;
            case PlayerPosture.Dead:
                OnAction(() => _mover.DieCo())();
                break;
            case PlayerPosture.FallingAlive:
                _mover.Fall(true);
                break;
            case PlayerPosture.FallingDead:
                _mover.Fall(false);
                break;
            default:
                _mover.Reset();
                break;
        }
    }

    Coroutine _actionCoroutine;

    void StopPlayingAction()
    {
        if (_actionCoroutine != null)
        {
            StopCoroutine(_actionCoroutine);
            _actionCoroutine = null;
            _isStatusDirty = true;
        }
    }

    Action OnAction(Func<IEnumerator> coroutine)
    {
        return () =>
        {
            _actionCoroutine = StartCoroutine(OnActionCo(coroutine));
        };
    }

    IEnumerator OnActionCo(Func<IEnumerator> coroutine)
    {
        if (coroutine != null)
        {
            yield return coroutine();
        }
        StopPlayingAction();
    }

    #endregion

    #region Status

    #endregion

    #region Actions

    #endregion

}
