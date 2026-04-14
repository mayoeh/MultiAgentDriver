// using Fusion;
// using UnityEngine;

// public class PlayerMovement : NetworkBehaviour
// {
//     private CharacterController _controller;
//     public float PlayerSpeed = 2f;
//     public float JumpForce = 5f;
//     public float GravityValue = -9.81f;
//     private Vector3 _velocity;
//     private bool _jumpPressed;

//     private void Awake() {
//         _controller = GetComponent<CharacterController>();
//     }

//     void Update()
//     {
//         if (Input.GetButtonDown("Jump"))
//         {
//             _jumpPressed = true;
//         }
//     }

//     public override void FixedUpdateNetwork()
//     {
//         // Only move own player and not every other player. Each player controls its own player object.
//         if (HasStateAuthority == false)
//         {
//             return;
//         }

//         if (_controller.isGrounded)
//         {
//             _velocity = new Vector3(0, -1, 0);
//         }

//         Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;


//         _velocity.y += GravityValue * Runner.DeltaTime;
//         if (_jumpPressed && _controller.isGrounded)
//         {
//             _velocity.y += JumpForce;
//         }
//         _controller.Move(move + _velocity * Runner.DeltaTime);

//         if (move != Vector3.zero)
//         {
//             gameObject.transform.forward = move;
//         }

//         _jumpPressed = false;
//     }
// }
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterController _controller;

    public float PlayerSpeed = 5f;
    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    private Vector3 _velocity;
    private bool _jumpPressed;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Only local player reads input
        if (!HasStateAuthority)
            return;

        if (Input.GetButtonDown("Jump"))
        {
            _jumpPressed = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Safety check (prevents null errors)
        if (!HasStateAuthority || _controller == null)
            return;

        // Ground handling
        if (_controller.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        // Horizontal movement
        _controller.Move(move * PlayerSpeed * Runner.DeltaTime);

        // Jump
        if (_jumpPressed && _controller.isGrounded)
        {
            _velocity.y = JumpForce;
        }

        // Gravity
        _velocity.y += GravityValue * Runner.DeltaTime;

        // Vertical movement
        _controller.Move(_velocity * Runner.DeltaTime);

        // Rotation
        if (move.sqrMagnitude > 0.001f)
        {
            transform.forward = move;
        }

        // Reset jump
        _jumpPressed = false;
    }
}