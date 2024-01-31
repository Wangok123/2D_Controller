using System;
using UnityEngine;
using UnityEngine.WSA;

namespace MyNamespace
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float jumpHeight = 4;
        [SerializeField] private float timeToJumpApex = .4f;
        [SerializeField] private float accelerationTimeAirborne = .2f;
        [SerializeField] private float accelerationTimeGrounded = .1f;
        float moveSpeed = 6;
        
        float gravity;
        float jumpVelocity;
        Vector3 velocity;
        bool shouldJump; // 添加一个字段来存储跳跃指令
        float velocityXSmoothing;

        Controller2D controller;

        void Start()
        {
            controller = GetComponent<Controller2D>();
            
            gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            print("Gravity: " + gravity + "  Jump Velocity: " + jumpVelocity);
        }

        void Update()
        {
            // 检测并存储是否应当跳跃的指令
            if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
            {
                shouldJump = true;
            }
        }
  
        void FixedUpdate()
        {
            if (controller.collisions.above || controller.collisions.below)
            {
                velocity.y = 0;
            }

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
            if (shouldJump)
            {
                velocity.y = jumpVelocity;
                shouldJump = false; // 重置跳跃指令
            }
        
            float targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collisions.below ? accelerationTimeGrounded:accelerationTimeAirborne);
            velocity.y += gravity * Time.fixedDeltaTime; // 注意是使用 Time.fixedDeltaTime
            controller.Move(velocity * Time.fixedDeltaTime);
 
        }
    }
}