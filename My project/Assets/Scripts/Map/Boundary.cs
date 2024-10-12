using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Map 
{
    public class Boundary : MonoBehaviour
    {
        public static Boundary instance { get; private set; }

        private List<Vector2> boundaryPoints = new List<Vector2>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
                // Make this object's transform a root transform
                transform.SetParent(null);
                
                DontDestroyOnLoad(gameObject);
                InitializeBoundaryPoints();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeBoundaryPoints()
        {
            boundaryPoints.Clear();
            Transform boundaryObject = GameObject.Find("Boundary").transform;
            for (int i = 1; i <= boundaryObject.childCount; i++)
            {
                Transform point = boundaryObject.Find(i.ToString());
                if (point != null)
                {
                    Vector2 position = new Vector2(point.position.x, point.position.z);
                    boundaryPoints.Add(position);
                }
                else 
                {
                    break;
                }
            }
        }

        public bool IsWithinBoundary(Vector2 position)
        {
            int vertexCount = boundaryPoints.Count;
            if (vertexCount < 3)
                return false;

            bool isInside = false;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
            {
                Vector2 vi = boundaryPoints[i];
                Vector2 vj = boundaryPoints[j];

                if (((vi.y > position.y) != (vj.y > position.y)) &&
                    (position.x < (vj.x - vi.x) * (position.y - vi.y) / (vj.y - vi.y) + vi.x))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        private Vector2 GetClosestPointOnLineSegment(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            line.Normalize();

            Vector2 lineStartToPoint = point - lineStart;
            float dotProduct = Vector2.Dot(lineStartToPoint, line);

            if (dotProduct <= 0)
                return lineStart;
            else if (dotProduct >= lineLength)
                return lineEnd;
            else
                return lineStart + line * dotProduct;
        }

        private Vector2 GetNearestPointOnBoundary(Vector2 position)
        {
            if (boundaryPoints.Count < 3)
                return position;

            Vector2 nearestPoint = Vector2.zero;
            float minDistance = float.MaxValue;

            for(int i = 0; i < boundaryPoints.Count; i++)
            {
                int nextIndex = (i + 1) % boundaryPoints.Count;
                Vector2 start = boundaryPoints[i];
                Vector2 end = boundaryPoints[nextIndex];

                Vector2 closestPoint = GetClosestPointOnLineSegment(start, end, position);
                float distance = Vector2.Distance(position, closestPoint);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPoint = closestPoint;
                }
            }

            return nearestPoint;
        }

        public Vector3 ClampPositionToBoundary(Vector3 position)
        {
            Vector2 positionXZ = new Vector2(position.x, position.z);
            
            if (!IsWithinBoundary(positionXZ))
            {
                // Log message
                Vector2 clampedXZ = GetNearestPointOnBoundary(positionXZ);
                return new Vector3(clampedXZ.x, position.y, clampedXZ.y);
            }
            
            return position;
        }
    }
}
