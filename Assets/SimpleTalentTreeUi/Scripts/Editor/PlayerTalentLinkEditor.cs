#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SimpleTalentTreeUi
{
    /// <summary>
    /// Custom inspector for SimpleTalentTreeUi.PlayerTalentLink.
    /// Uses SerializedProperty and type name reflection so it does not
    /// need a direct compile-time reference to PlayerTalentLink.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class PlayerTalentLinkEditor : Editor
    {
        private SerializedProperty treeAssetProp;
        private SerializedProperty treeIdProp;
        private SerializedProperty linkAutomaticallyProp;
        private SerializedProperty bindingsProp;

        private TalentTreeSO cachedTree;
        private bool showBindings = true;
        private Vector2 bindingsScroll;

        /// <summary>
        /// Returns true if the current target is SimpleTalentTreeUi.PlayerTalentLink.
        /// </summary>
        private bool IsPlayerTalentLink =>
            target != null && target.GetType().FullName == "SimpleTalentTreeUi.PlayerTalentLink";

        private void OnEnable()
        {
            if (!IsPlayerTalentLink)
                return;

            treeAssetProp = serializedObject.FindProperty("treeAsset");
            treeIdProp = serializedObject.FindProperty("treeId");
            linkAutomaticallyProp = serializedObject.FindProperty("linkAutomatically");
            bindingsProp = serializedObject.FindProperty("bindings");

            cachedTree = treeAssetProp.objectReferenceValue as TalentTreeSO;

            if (cachedTree == null)
            {
                RefreshTreeAssetFromId();

                if (cachedTree != null)
                {
                    treeAssetProp.objectReferenceValue = cachedTree;
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        /// <summary>
        /// Tries to find the TalentTreeSO asset using the stored treeId string.
        /// </summary>
        private void RefreshTreeAssetFromId()
        {
            cachedTree = null;
            if (treeIdProp == null)
                return;

            string id = treeIdProp.stringValue;
            if (string.IsNullOrEmpty(id))
                return;

            string[] guids = AssetDatabase.FindAssets("t:TalentTreeSO");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var tree = AssetDatabase.LoadAssetAtPath<TalentTreeSO>(path);
                if (tree != null && tree.ID == id)
                {
                    cachedTree = tree;
                    break;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            // If the target is not PlayerTalentLink, fall back to the default inspector
            if (!IsPlayerTalentLink)
            {
                base.OnInspectorGUI();
                return;
            }

            if (treeAssetProp == null ||
                treeIdProp == null ||
                linkAutomaticallyProp == null ||
                bindingsProp == null)
            {
                // Ensure properties are initialized correctly
                OnEnable();
            }

            serializedObject.Update();

            EditorGUILayout.LabelField("Player Talent Link", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(linkAutomaticallyProp);

            EditorGUILayout.Space();
            DrawTreeSelection();
            EditorGUILayout.Space();
            DrawBindingsList();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the section where the TalentTreeSO is selected and
        /// the tree ID is displayed.
        /// </summary>
        private void DrawTreeSelection()
        {
            EditorGUILayout.LabelField("Talent Tree", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                treeAssetProp,
                new GUIContent("Tree Asset", "Select the TalentTreeSO used by this player"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                cachedTree = treeAssetProp.objectReferenceValue as TalentTreeSO;

                if (cachedTree != null)
                {
                    treeIdProp.stringValue = cachedTree.ID;
                    RegenerateBindingsFromTree(cachedTree);
                }
                else
                {
                    treeIdProp.stringValue = string.Empty;
                    if (bindingsProp != null)
                        bindingsProp.ClearArray();
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(treeIdProp, new GUIContent("Tree Id"));
            }

            if (cachedTree == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a TalentTreeSO to fill Tree Id and generate the bindings list.",
                    MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Rebuild bindings from tree"))
                {
                    RegenerateBindingsFromTree(cachedTree);
                }
            }
        }

        /// <summary>
        /// Rebuilds the bindings list from the TalentTreeSO.
        /// NOTE: this resets the UnityEvents for each binding (On Points Changed).
        /// </summary>
        private void RegenerateBindingsFromTree(TalentTreeSO treeSo)
        {
            if (treeSo == null || bindingsProp == null)
                return;

            bindingsProp.ClearArray();

            for (int i = 0; i < treeSo.talentNodes.Count; i++)
            {
                var node = treeSo.talentNodes[i];
                if (node == null)
                    continue;

                int newIndex = bindingsProp.arraySize;
                bindingsProp.InsertArrayElementAtIndex(newIndex);
                var element = bindingsProp.GetArrayElementAtIndex(newIndex);

                var idProp = element.FindPropertyRelative("talentId");
                var titleProp = element.FindPropertyRelative("title");
                var iconProp = element.FindPropertyRelative("icon");
                var eventProp = element.FindPropertyRelative("onPointsChanged");

                if (idProp != null) idProp.intValue = i;
                if (titleProp != null) titleProp.stringValue = node.title;
                if (iconProp != null) iconProp.objectReferenceValue = node.GetNodeImageAsSprite();

                // eventProp remains with the default UnityEvent (no listeners).
                // The user can configure callbacks after generating or regenerating the list.
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// Draws the scrollable list of talent bindings and their events.
        /// </summary>
        private void DrawBindingsList()
        {
            EditorGUILayout.Space();
            showBindings = EditorGUILayout.Foldout(
                showBindings,
                $"Talent Bindings ({(bindingsProp != null ? bindingsProp.arraySize : 0)})",
                true);

            if (!showBindings || bindingsProp == null)
                return;

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            bindingsScroll = EditorGUILayout.BeginScrollView(
                bindingsScroll,
                GUILayout.MaxHeight(260));

            for (int i = 0; i < bindingsProp.arraySize; i++)
            {
                var element = bindingsProp.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative("talentId");
                var titleProp = element.FindPropertyRelative("title");
                var iconProp = element.FindPropertyRelative("icon");
                var eventProp = element.FindPropertyRelative("onPointsChanged");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                Texture2D tex = null;
                var iconSprite = iconProp != null ? iconProp.objectReferenceValue as Sprite : null;
                if (iconSprite != null)
                {
                    tex = iconSprite.texture;
                }
                else if (cachedTree != null &&
                         idProp != null &&
                         idProp.intValue >= 0 &&
                         idProp.intValue < cachedTree.talentNodes.Count)
                {
                    var node = cachedTree.talentNodes[idProp.intValue];
                    if (node != null && node.image != null)
                        tex = node.image;
                }

                if (tex != null)
                    GUILayout.Label(tex, GUILayout.Width(32), GUILayout.Height(32));
                else
                    GUILayout.Box(GUIContent.none, GUILayout.Width(32), GUILayout.Height(32));

                EditorGUILayout.BeginVertical();
                if (titleProp != null)
                    EditorGUILayout.LabelField(titleProp.stringValue, EditorStyles.boldLabel);
                if (idProp != null)
                    EditorGUILayout.LabelField($"ID: {idProp.intValue}");
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                if (eventProp != null)
                    EditorGUILayout.PropertyField(eventProp, new GUIContent("On Points Changed"));

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }
    }
}
#endif
