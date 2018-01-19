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
    PlayerPosture _status = PlayerPosture.Empty;
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
        if (_deadNote != null)
        {
            _deadNote.gameObject.SetActive(false);
        }
        if (_resumeTip != null)
        {
            _resumeTip.gameObject.SetActive(true);
        }
    }
    #endregion

    private void Update()
    {
        UpdateStatus();
        if (!IsDead)
        {
            UpdateInput();
        }
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //AlchemyLab.Instance.UseCurrentPotion();
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
            _tree.StopPlayingAction(request.action);
        }
    }

    void Spawn()
    {
        _currentHp = hp;
        _mover.Reset();
        _isStatusDirty = true;
        Run();
    }

    void Run()
    {
        SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Running });
    }

    void Stop()
    {
        SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Idle });
    }

    void Damage(int dmg, DamageType type)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Max(0, CurrentHp - dmg);
        if (CurrentHp <= 0)
        {
            Die();
        }
        else
        {
            LoseHp(dmg, type);
        }
    }

    public void Die()
    {
        SendBehaviourRequest(new PlayerBlackboard
        {
            posture = PlayerPosture.Empty,
            action = new PlayerAction(
                PlayerActionType.Die,
                OnAction(
                    () => _mover.DieCo(),
                    () => SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Dead })
                    )
                )
        }, true);
    }

    public void LoseHp(int dmg, DamageType type)
    {
        switch (type)
        {
            case DamageType.Normal:
                SendBehaviourRequest(new PlayerBlackboard
                {
                    //status = PlayerStatus.Empty,
                    action = new PlayerAction(
                        PlayerActionType.Hit,
                        OnAction(
                            () => _mover.HitBackCo(),
                            () => SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Running })
                        )
                    )
                }, true);
                break;
        }
    }

    void TossPotion(int id)
    {
        SendBehaviourRequest(new PlayerBlackboard
        {
            action = new PlayerAction(
                PlayerActionType.Cast,
                OnAction(
                    () => _mover.CastCo(id),
                    () => SendBehaviourRequest(new PlayerBlackboard { posture = PlayerPosture.Running })
                )
            )
        });
    }

    public void OnDead()
    {
        
    }

    #endregion

    #region Orders

    void OnReceiveOrder(PlayerOrder order)
    {
        if (order.posture.HasValue)
        {
            OnStatusChanged(order.posture.Value);
        }
    }

    void OnStatusChanged(PlayerPosture status)
    {
        //ConsoleProDebug.Watch("Status -> ", status.ToString());
        if (_status == status) return;
        //PrettyLog.Log("Change status: {0} => {1}", _status, status);
        _status = status; // Status will be updated in the next frame
        _isStatusDirty = true;
    }
    void UpdateStatus()
    {
        if (!_isStatusDirty) return;
        _isStatusDirty = false;
        switch (_status)
        {
            case PlayerPosture.Idle:
                _mover.Stop();
                break;
            case PlayerPosture.Running:
                _mover.Run();
                break;
            case PlayerPosture.Dead:
                _mover.Reset();
                //OnDead();
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
        }
    }

    Action OnAction(Func<IEnumerator> coroutine, Action afterAction)
    {
        return () => StartCoroutine(OnActionCo(
                    coroutine,
                    afterAction
                    ));
    }

    IEnumerator OnActionCo(Func<IEnumerator> coroutine, Action afterAction)
    {
        if (coroutine != null)
        {
            yield return coroutine();
        }
        _tree.StopPlayingAction(null);
        if (afterAction != null)
        {
            afterAction();
        }
    }

    #endregion

    #region Status

    #endregion

    #region Actions

    #endregion

}
