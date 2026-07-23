#if UNITY_EDITOR
// Compatibility shim: Object.GetInstanceID() is error-level obsolete on Unity 6000.5+.
namespace HierarchyDesigner
{
    internal static class HD_InstanceIdCompat
    {
        public static int CompatInstanceId(this UnityEngine.Object obj)
        {
#if UNITY_6000_3_OR_NEWER
            return obj.GetEntityId().GetHashCode();
#else
            return obj.GetInstanceID();
#endif
        }
    }
}
#endif
