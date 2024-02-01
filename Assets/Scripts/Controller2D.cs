using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Controller2D : MonoBehaviour
    {
        public LayerMask collisionMask;

        const float skinWidth = .015f;
        public int horizontalRayCount = 4;
        public int verticalRayCount = 4;

        private float maxClimbAngle = 80;
        private float maxDescendAngle = 75;
        
        float horizontalRaySpacing;
        float verticalRaySpacing;

        BoxCollider2D collider;
        RaycastOrigins raycastOrigins;
        public CollectionInfo collisions;

        void Start()
        {
            collider = GetComponent<BoxCollider2D>();
            CalculateRaySpacing();
        }

        // 1. 检测水平方向的碰撞
        private void HorizontalCollisions(ref Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;
            // 从下往上依次检测
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= maxClimbAngle)
                    {
                        if (collisions.DescendingSlope)
                        {
                            //防止一个斜坡到另一个反向斜坡时，出现停止一帧的情况
                            collisions.DescendingSlope = false;
                            velocity = collisions.velocityOld;
                        }
                        float distanceToSlopeStart = 0;
                        // 且斜坡角度发生变化，需要调整皮肤宽度
                        if(slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - skinWidth;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }
                        
                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.ClimbingSlope || slopeAngle > maxClimbAngle)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;
                    
                        if(collisions.ClimbingSlope)
                        {//在斜坡上碰撞到左右墙壁时，把通过斜坡爬坡得到的y轴速度去掉，
                            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }
                        
                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }
            }
        }
        
        // 2. 检测垂直方向的碰撞
        private void VerticalCollisions(ref Vector3 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;
            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
                Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2,
                    Color.red);

                if (hit)
                {
                     velocity.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    if (collisions.ClimbingSlope)
                    {//在斜坡上碰撞到上下墙壁时，x轴速度清零
                        velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) *
                                     Mathf.Sign(velocity.x);
                    }

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }
            }
            
            if (collisions.ClimbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                rayLength = Mathf.Abs(velocity.x) + skinWidth;
                Vector2 rayOrigin = (directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) +
                                    Vector2.up * velocity.y;
                
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
                
                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    // 如果斜坡角度发生变化，需要调整速度
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }
        }

        // 3. 移动
        public void Move(Vector3 velocity)
        {
            UpdateRaycastOrigins();
            collisions.Reset();
            collisions.velocityOld = velocity;

            if (velocity.y < 0)
            {
                DescendSlope(ref velocity);
            }
            
            if (velocity.x != 0)
            {
                HorizontalCollisions(ref velocity);
            }

            if (velocity.y != 0)
            {
                VerticalCollisions(ref velocity);
            }
            transform.Translate(velocity);
        }

        // 4. 爬坡, 同时速度以velocity.x为准
        void ClimbSlope(ref Vector3 velocity, float slopeAngle)
        {
            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
            
            // 如果velocity.y大于爬坡速度，证明在跳跃，不需要爬坡
            if (velocity.y <= climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisions.below = true;
                collisions.ClimbingSlope = true;
                collisions.slopeAngle = slopeAngle;
            }
        }
        
        void DescendSlope(ref Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomRight:raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                        {
                            float moveDistance = Mathf.Abs(velocity.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendVelocityY;
                            
                            collisions.slopeAngle = slopeAngle;
                            collisions.DescendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }
            }
        }
        
        // 5. 更新射线原点
        void UpdateRaycastOrigins()
        {
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        // 6. 计算射线间隔
        void CalculateRaySpacing()
        {
            Bounds bounds = collider.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }

        // 7. 射线原点
        struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }
        
        // 8. 碰撞信息
        public struct CollectionInfo
        {
            public bool above, below;
            public bool left, right;
            public Vector3 velocityOld;
            public bool ClimbingSlope { get; set; }
            public bool DescendingSlope { get; set; }
            public float slopeAngle , slopeAngleOld;

            public void Reset()
            {
                above = below = false;
                left = right = false;
                ClimbingSlope = false;
                DescendingSlope = false;
                velocityOld = Vector3.zero;
                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
            }
        }
    }
}