using System;
using UnityEngine;

namespace MyNamespace
{
    public class Player : MonoBehaviour
    {
        private float jumpHeight;
        private float timeToJumpApex;
        
        float moveSpeed;
        float gravity;
        float jumpVelocity;
        Vector3 velocity;
        bool shouldJump; // 添加一个字段来存储跳跃指令

        Controller2D controller;

        void Start()
        {
            controller = GetComponent<Controller2D>();
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
        
            velocity.x = input.x * moveSpeed;
            velocity.y += gravity * Time.fixedDeltaTime; // 注意是使用 Time.fixedDeltaTime
            controller.Move(velocity * Time.fixedDeltaTime);

        }
    }
}