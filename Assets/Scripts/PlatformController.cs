using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class PlatformController : RaycastController
    {
        public LayerMask passengerMask;
        public Vector3 move;
        
        public override void Start()
        {
            base.Start();
        }
        
        void Update()
        {
            Vector3 velocity = move * Time.deltaTime;
            transform.Translate(velocity);
        }
        
        void MovePassenger(Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            float directionY = Mathf.Sign(velocity.y);
            
            if(velocity.y != 0)
            {
                float rayLength = Mathf.Abs(velocity.y) + skinWidth;
                for (int i = 0; i < verticalRayCount; i++)
                {
                    Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft:raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                    if (hit)
                    {
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;
                    }
                }
            }
        }
    }
}