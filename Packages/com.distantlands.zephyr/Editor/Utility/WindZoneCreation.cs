using UnityEditor;
using UnityEngine;

namespace DistantLands.Zephyr.EditorScripts
{
    public class WindZoneCreation
    {

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Manager", false, 5, secondaryPriority = 10)]
        public static void CreateWindManager()
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Wind Manager";
            windZone.AddComponent<ZephyrWind>();

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        //Create Effect Listeners

        [MenuItem("GameObject/Distant Lands/Zephyr/Effect Listeners/Audio", false, 5, secondaryPriority = 1)]
        public static void CreateWindAudio(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Audio";
            windZone.AddComponent<WindAudio>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Effect Listeners/Animator", false, 5, secondaryPriority = 1)]
        public static void CreateWindAnimator(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Animator";
            windZone.AddComponent<WindAnimator>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Effect Listeners/Cloth", false, 5, secondaryPriority = 1)]
        public static void CreateWindCloth(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Cloth";
            windZone.AddComponent<WindCloth>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Effect Listeners/Particles", false, 5, secondaryPriority = 1)]
        public static void CreateWindParticles(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Particles";
            windZone.AddComponent<WindParticles>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Effect Listeners/Visual Effect", false, 5, secondaryPriority = 1)]
        public static void CreateWindVisualEffect(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Zephyr Visual Effect";
            windZone.AddComponent<WindVisualEffect>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        //Create Wind Zones

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Constant Force Zone", false, 5, secondaryPriority = 1)]
        public static void CreateConstantForceZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Constant Force Zone";
            windZone.AddComponent<ConstantForceZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Acceleration Zone", false, 5, secondaryPriority = 1)]
        public static void CreateAccelerationZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Acceleration Zone";
            windZone.AddComponent<AccelerationZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Attraction Zone", false, 5, secondaryPriority = 1)]
        public static void CreateAttractionZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Attraction Zone";
            windZone.AddComponent<AttractionZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Funnel Zone", false, 5, secondaryPriority = 1)]
        public static void CreateFunnelZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Funnel Zone";
            windZone.AddComponent<FunnelZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }

        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Vortex Zone", false, 5, secondaryPriority = 1)]
        public static void CreateVortexZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Vortex Zone";
            windZone.AddComponent<VortexZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }
        [MenuItem("GameObject/Distant Lands/Zephyr/Wind Zones/Aerodynamic Zone", false, 5, secondaryPriority = 1)]
        public static void CreateAerodynamicZone(MenuCommand menuCommand)
        {
            GameObject windZone = new GameObject();
            windZone.name = "Aerodynamic Zone";
            windZone.AddComponent<AerodynamicZone>();

            GameObjectUtility.SetParentAndAlign(windZone, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(windZone, "Create " + windZone.name);
            Selection.activeObject = windZone;
        }
    }
}
