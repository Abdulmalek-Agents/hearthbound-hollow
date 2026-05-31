
using Exoa.Events;
using ProceduralToolkit;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static Exoa.Designer.AppController;

namespace Exoa.Designer
{
    /// <summary>
    /// Represents a procedural roof top generator within the Unity environment.
    /// This class is responsible for generating and rendering the roof on top of a building.
    /// </summary>
    public class ProceduralRoofTop : MonoBehaviour
    {
        private AppController.RoofConfig config;
        private AppController appController;
        public MeshFilter roofMf;
        public MeshRenderer roofRd;
        private List<MeshDraft> drafts;
        public List<Vector2> polygon;

        /// <summary>
        /// Cleans up event subscriptions when the object is destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEditorEvents.OnRequestClearAll -= Clear;
            Debug.Log("GameEditorEvents.OnRequestClearAll -= Clear;");
        }

        /// <summary>
        /// Initializes the roof generator, setting up event subscriptions based on the current application state.
        /// </summary>
        void Awake()
        {
            appController = GetComponentInParent<AppController>();
            if (appController == null)
            {
                appController = GameObject.FindObjectOfType<AppController>();
            }
            Debug.Log("appController.currentState:" + appController.currentState);
            if (appController.currentState != AppController.States.PlayMode)
            {
                GameEditorEvents.OnRequestClearAll += Clear;
                Debug.Log("GameEditorEvents.OnRequestClearAll += Clear;");
            }
        }

        /// <summary>
        /// Generates the roof based on the specified polygon and configuration.
        /// </summary>
        /// <param name="polygon">A list of Vector2 points defining the roof's polygon shape.</param>
        /// <param name="config">The roof configuration parameters.</param>
        /// <returns>True if generation was successful; otherwise, false.</returns>
        public bool Generate(List<Vector2> polygon, AppController.RoofConfig config)
        {
            //print("Roof Generate polygon:" + polygon.Count);

            this.config = config;
            this.polygon = polygon;
            return Generate();
        }

        /// <summary>
        /// Clears any previously generated drafts.
        /// </summary>
        public void ClearPreviousDrafts()
        {
            drafts = new List<MeshDraft>();
        }

        /// <summary>
        /// Clears the roof and optionally additional UI elements based on the parameters.
        /// </summary>
        /// <param name="clearFloorsUI">Whether to clear the floors UI.</param>
        /// <param name="clearFloorMapUI">Whether to clear the floor map UI.</param>
        /// <param name="clearScene">Whether to clear the scene elements.</param>
        public void Clear(bool clearFloorsUI = false, bool clearFloorMapUI = false, bool clearScene = false)
        {
            HDLogger.Log("Procedural RoofTop Clear clearScene:" + clearScene, HDLogger.LogCategory.Building);

            if (!clearScene)
                return;

            roofMf.mesh = null;
            ClearPreviousDrafts();
        }

        /// <summary>
        /// Generates the roof draft based on the stored configuration and polygon.
        /// </summary>
        /// <returns>True if generation was successful; otherwise, false.</returns>
        public bool Generate()
        {
            if (drafts == null) drafts = new List<MeshDraft>();

            IConstructible<MeshDraft> roofConstructible = Plan(polygon, config);
            try
            {
                MeshDraft draft = roofConstructible.Construct(Vector2.zero);
                if (config.type != RoofType.Flat)
                    draft.uv = MathUtils.GenerateUVs2(draft.vertices, draft.normals, 5, AppController.Instance.wallsHeight);

                //Debug.Log("Roof Generate:" + drafts.Count);

                drafts.Add(draft);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Roof : " + e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Renders the generated roof by combining all drafts into a single mesh.
        /// </summary>
        /// <returns>The GameObject representing this roof.</returns>
        public GameObject Render()
        {
            if (drafts == null || drafts.Count == 0)
                return gameObject;

            //Debug.Log("Roof Render:" + drafts.Count);

            MeshDraft md = new MeshDraft() { name = "MergedRoofs" };
            for (int i = 0; i < drafts.Count; i++)
            {
                md.Add(drafts[i]);
            }
            roofMf.mesh = md.ToMesh();
            MeshCollider mc = roofMf.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.sharedMesh = roofMf.sharedMesh;
            }
            roofMf.transform.localPosition = Vector3.up * AppController.Instance.wallsHeight;
            return gameObject;
        }

        /// <summary>
        /// Determines and returns a roof construction strategy based on the roof type specified in the configuration.
        /// </summary>
        /// <param name="foundationPolygon">The polygon defining the base of the roof.</param>
        /// <param name="config">The roof configuration parameters containing the roof type and properties.</param>
        /// <returns>An IConstructible object for constructing the specified type of roof.</returns>
        private IConstructible<MeshDraft> Plan(List<Vector2> foundationPolygon, AppController.RoofConfig config)
        {
            switch (config.type)
            {
                case AppController.RoofType.Flat:
                    return new ProceduralFlatRoof(foundationPolygon, config, config.color);
                case AppController.RoofType.Hipped:
                    return new ProceduralHippedRoof(foundationPolygon, config, config.color);
                case AppController.RoofType.Gabled:
                    return new ProceduralGabledRoof(foundationPolygon, config, config.color);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Generates roofs based on the provided walls and rooms.
        /// </summary>
        /// <param name="walls">A list of lists containing wall points.</param>
        /// <param name="rooms">A list of lists containing room points.</param>
        /// <param name="flatRoot">Indicates whether a flat roof should be used.</param>
        public void GenerateFromWalls(List<List<Vector2>> walls, List<List<Vector2>> rooms, bool flatRoot)
        {
            RoofConfig flatRoofConfig = new RoofConfig(RoofType.Flat, .001f, 0);
            HDLogger.Log("[Roof] GenerateFromWalls walls:" + walls.Count, HDLogger.LogCategory.Building);

            bool hasErrors = false;
            for (int j = 0; j < walls.Count; j++)
            {
                List<Vector2> outerPoints = new List<Vector2>(walls[j]);

                outerPoints = MathUtils.GetClockwise(outerPoints);
                //HDLogger.Log("[Roof] Dump1:" + outerPoints.Dump(), HDLogger.LogCategory.Building);
                if (!Generate(outerPoints, flatRoot ? flatRoofConfig : AppController.Instance.roof))
                    hasErrors = true;
            }
            if (hasErrors)
            {
                ClearPreviousDrafts();
                HDLogger.Log("[Roof] GenerateFromWalls rooms:" + rooms.Count, HDLogger.LogCategory.Building);

                for (int i = 0; i < rooms.Count; i++)
                {
                    List<Vector2> outerPoints = new List<Vector2>(rooms[i]);
                    outerPoints = MathUtils.GetClockwise(rooms[i]);
                    //HDLogger.Log("[Roof]  room(" + i + "):" + outerPoints.Count, HDLogger.LogCategory.Building);
                    //HDLogger.Log("[Roof] Dump2:" + outerPoints.Dump(), HDLogger.LogCategory.Building);
                    Generate(outerPoints, flatRoot ? flatRoofConfig : AppController.Instance.roof);
                }
            }
        }

        /// <summary>
        /// Disables the mesh collider attached to the roof mesh filter, if present.
        /// </summary>
        public void DisableCollider()
        {
            MeshCollider mc = roofMf.GetComponent<MeshCollider>();
            if (mc != null)
            {
                //mc.enabled = false;
                Destroy(mc);
            }
        }
    }
}
