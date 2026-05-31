
using ProceduralToolkit;
using ProceduralToolkit.ClipperLib;
using System.Collections.Generic;
using UnityEngine;

namespace Exoa.Designer
{
    /// <summary>
    /// Class representing a procedural space for generating an outdoor area.
    /// </summary>
    public class ProceduralOutside : ProceduralSpace
    {
        /// <summary>
        /// The MeshFilter component used to hold the generated floor mesh.
        /// </summary>
        public MeshFilter floor;

        /// <summary>
        /// Clears the generated mesh by setting the floor's mesh to null.
        /// </summary>
        override public void GenerateEmpty()
        {
            floor.mesh = null;
        }

        /// <summary>
        /// Generates the outdoor area floor based on the provided list of world positions.
        /// </summary>
        /// <param name="worldPosList">A list of world positions representing the outline of the outdoor area.</param>
        override public void Generate(List<Vector3> worldPosList)
        {
            normalRoomPoints = MathUtils.GetClockwise(worldPosList, MathUtils.PlanType.XZ);
            GenerateFloor();
            AddMeshColliders();
        }

        /// <summary>
        /// Adds a MeshCollider to the floor if the flag for adding colliders is set and there isn't one already attached.
        /// </summary>
        protected void AddMeshColliders()
        {
            if (addMeshColliders)
            {
                if (floor != null && floor.GetComponent<MeshCollider>() == null)
                    floor.gameObject.AddComponent<MeshCollider>();
            }
        }

        /// <summary>
        /// Generates the floor mesh based on the normal room points, setting their Y coordinates to zero 
        /// and tessellating to create the final mesh geometry.
        /// </summary>
        protected void GenerateFloor()
        {
            //print("GenerateFloor points:" + normalRoomPoints.Dump());
            for (int i = 0; i < normalRoomPoints.Count; i++)
                normalRoomPoints[i] = normalRoomPoints[i].SetY(0);
            //print("GenerateFloor points:" + normalRoomPoints.Dump());

            Tessellator tessellator = new Tessellator();
            tessellator.AddContour(normalRoomPoints);
            tessellator.Tessellate(normal: Vector3.up);
            MeshDraft md = tessellator.ToMeshDraft();
            md.uv = MathUtils.GenerateUVs(md.vertices, AppController.Instance.wallsHeight,
                AppController.Instance.wallsHeight, MathUtils.PlanType.XZ);
            md.Paint(SpaceVertexColor);
            floor.mesh = md.ToMesh();
        }

    }
}
