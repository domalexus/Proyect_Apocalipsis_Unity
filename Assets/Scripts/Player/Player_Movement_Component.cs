using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement_Component : MonoBehaviour
{
    private InputAction PlayerMoveAction;
    private Vector2 MovementValue;
    private Rigidbody2D RigidBodyComp;
    private Animator AnimatorComp;
    private float LastX;
    private float LastY;
    public bool CanMove = true;
    public float Speed = 5f;

    void Awake()
    {
        //Buscamos los componentes 
        RigidBodyComp = GetComponent<Rigidbody2D>();
        AnimatorComp = GetComponent<Animator>();

        PlayerMoveAction = InputSystem.actions.FindAction("Move");
    }
    void Start()
    {
        
    }

    
    void Update()
    {
        ReadMoveAction();
    }

    void FixedUpdate()
    {
        Move();
    }


    private void ReadMoveAction ()
    {
        MovementValue = PlayerMoveAction.ReadValue<Vector2>();
        AnimatorComp.SetFloat("X", MovementValue.x);
        AnimatorComp.SetFloat("Y", MovementValue.y);
        AnimatorComp.SetFloat("Speed", MovementValue.magnitude);
        UpdateLastXY(MovementValue.x, MovementValue.y);
    }

    private void Move()
    {
        if(!CanMove) return;
        //if(MovementValue == Vector2.zero) return;

        RigidBodyComp.linearVelocity = MovementValue * Speed;
        
        //if(RigidBodyComp.linearVelocityX <)
    }

    private void UpdateLastXY (float X, float Y)
    {
        
        if(X == 0 && Y == 0) return;
        
        float XRounded = Mathf.Round(X * 10) / 10;
        float YRounded = Mathf.Round(Y * 10) / 10;

        //if(XRounded > 0.5) XRounded = 1f;
        //if(YRounded > 0.5) YRounded = 1f;
//
        //if(XRounded < -0.5) XRounded = -1f;
        //if(YRounded < -0.5) YRounded = -1f;

        LastX = XRounded;
        LastY = YRounded;

        AnimatorComp.SetFloat("LastX", XRounded);
        AnimatorComp.SetFloat("LastY", YRounded);
        //print($"X: {XRounded} Y: {YRounded}");
        
        
    }
}   
