using System;
using UnityEngine;

namespace ItemDisplayPlacementHelper.AxisEditing
{
    public class LineAxisDrawer : MonoBehaviour
    {
        //https://docs.unity3d.com/ScriptReference/GL.html
        private static Material lineMaterial;
        private static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetInt("_ZTest", 0);
            }
        }

        public Color color;
        public Axis axis;

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
            CreateLineMaterial();
            lineMaterial.SetPass(0);

            var endPoint = Vector3.zero;
            switch (axis)
            {
                case Axis.X:
                    endPoint = Vector3.right;
                    break;
                case Axis.Y:
                    endPoint = Vector3.up;
                    break;
                case Axis.Z:
                    endPoint = Vector3.forward;
                    break;
            }

            GL.PushMatrix();

            GL.MultMatrix(transform.localToWorldMatrix);
            
            GL.Begin(GL.LINES);
            GL.Color(color);

            GL.Vertex(Vector3.zero);
            GL.Vertex(endPoint);

            GL.End();

            GL.PopMatrix();
        }

        private void OnDrawGizmos()
        {
            DrawLine(this, null);
        }
    }
}
