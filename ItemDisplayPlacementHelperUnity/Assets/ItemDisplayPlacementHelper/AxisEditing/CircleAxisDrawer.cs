using System;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class CircleAxisDrawer : MonoBehaviour
    {
        public Material lineMaterial;
        public Color color;
        [Range(0, 1000)]
        public int lineCount = 50;
        public float radius = 1;

        [Space]
        public Vector3 movementStart;
        public float movementAngle;
        [Range(0, 1)]
        public float movementColorAlpha = 0.1F;

        private void OnEnable()
        {
            CameraPostprocessEventHandler.onPostRender += DrawLine;
        }

        private void OnDisable()
        {
            CameraPostprocessEventHandler.onPostRender -= DrawLine;
        }

        private void DrawLine(object source, EventArgs eventArgs)
        {
            if (!lineMaterial)
            {
                return;
            }

            lineMaterial.SetPass(0);

            var cameraDirection = Camera.main.transform.forward.normalized;

            var normalVector = transform.forward.normalized;

            var drawFullCircle = Mathf.Abs(Vector3.Dot(normalVector, cameraDirection)) > 0.98;

            var orthogonalVector = (drawFullCircle ? CalculateOrthogonalVector(normalVector) : Vector3.Cross(normalVector, cameraDirection)).normalized;
            var circleStartVector = Vector3.Cross(normalVector, orthogonalVector).normalized;

            //GL.PushMatrix();
            //GL.MultMatrix(transform.localToWorldMatrix);

            #region Main axis circle
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color);

            var drawLineCount = lineCount / (drawFullCircle ? 1 : 2);
            for (var i = 0; i < drawLineCount; i++)
            {
                var angle = i / (float)drawLineCount * Mathf.PI * (drawFullCircle ? 2 : 1);

                var vertex = CalculatePointOnCircle(angle);

                GL.Vertex(vertex);
            }
            
            GL.Vertex(CalculatePointOnCircle(drawFullCircle ? 0 : Mathf.PI));

            GL.End();
            #endregion

            #region Movement axis circle
            if (movementStart != default)
            {
                orthogonalVector = (transform.rotation * movementStart).normalized;
                circleStartVector = Vector3.Cross(normalVector, orthogonalVector).normalized;

                GL.Begin(GL.TRIANGLES);
                GL.Color(new Color(color.r, color.g, color.b, movementColorAlpha));

                var i = 0;
                var angle = 0F;
                var angleStep = 1 / (lineCount * Mathf.PI * 2);
                var sign = Mathf.Sign(movementAngle);
                var absClampedMovementAngle = Mathf.Clamp(Mathf.Abs(movementAngle), 0, Mathf.PI * 6);

                if (angle == absClampedMovementAngle)
                {
                    GL.Vertex(transform.position);
                    GL.Vertex(CalculatePointOnCircle(0));
                    GL.Vertex(CalculatePointOnCircle(0));
                }

                while (angle < absClampedMovementAngle)
                {
                    GL.Vertex(transform.position);

                    angle = i * angleStep;
                    GL.Vertex(CalculatePointOnCircle(angle * sign));
                    i++;

                    angle = Math.Min(i * angleStep, absClampedMovementAngle);
                    GL.Vertex(CalculatePointOnCircle(angle * sign));
                }

                GL.End();
            }
            #endregion

            //GL.PopMatrix();

            Vector3 CalculatePointOnCircle(float angle)
            {
                var pointVector = orthogonalVector * Mathf.Cos(angle) * radius + circleStartVector * Mathf.Sin(angle) * radius;
                pointVector.x *= transform.lossyScale.x;
                pointVector.y *= transform.lossyScale.y;
                pointVector.z *= transform.lossyScale.z;

                return transform.position + pointVector;
            }
        }

        private void OnDrawGizmos()
        {
            DrawLine(this, null);
        }

        private Vector3 CalculateOrthogonalVector(Vector3 vector)
        {
            if (vector.x != 0)
            {
                return new Vector3(vector.y, -vector.x, 0);
            }
            if (vector.y != 0)
            {
                return new Vector3(-vector.y, vector.x, 0);
            }
            if (vector.z != 0)
            {
                return new Vector3(-vector.z, 0, vector.x);
            }

            return Vector3.zero;
        }

        //private Vector3 CalculatePointOnCircle(Vector3 normalVector, Vector3 orthogonalVector, float angle, float radius)
        //{
        //    var v = orthogonalVector;
        //    var u = Vector3.Cross(normalVector, v);
        //
        //    return v * Mathf.Cos(angle) * radius + u * Mathf.Sin(angle) * radius;
        //}

        //private Vector3 CalculateOrthogonalVector(Vector3 vector)
        //{
        //    var xMagnitude = Mathf.Abs(vector.x);
        //    var yMagnitude = Mathf.Abs(vector.y);
        //    var zMagnitude = Mathf.Abs(vector.z);
        //
        //    if (xMagnitude >= yMagnitude && yMagnitude >= zMagnitude)
        //    {
        //        return new Vector3(vector.y, -vector.x, 0);
        //    }
        //    if (xMagnitude >= zMagnitude && zMagnitude >= yMagnitude)
        //    {
        //        return new Vector3(vector.z, 0, -vector.x);
        //    }
        //
        //    if (yMagnitude >= xMagnitude && xMagnitude >= zMagnitude)
        //    {
        //        return new Vector3(0, vector.z, -vector.y);
        //    }
        //    if (yMagnitude >= zMagnitude && zMagnitude >= xMagnitude)
        //    {
        //        return new Vector3(-vector.y, vector.x, 0);
        //    }
        //
        //    if (zMagnitude >= xMagnitude && xMagnitude >= yMagnitude)
        //    {
        //        return new Vector3(-vector.z, 0, vector.x);
        //    }
        //    if (zMagnitude >= yMagnitude && yMagnitude >= xMagnitude)
        //    {
        //        return new Vector3(0, -vector.z, vector.y);
        //    }
        //
        //    return Vector3.zero;
        //}
    }
}
