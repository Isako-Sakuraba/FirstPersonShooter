using System;
using UnityEditor;
using UnityEngine;

namespace SplineCollider
{
    [CustomEditor(typeof(SplineCollider))]
    public class SplineColliderEditor : Editor
    {
        private const string POST_PROCESSING_WARNING = "Try post-processing instead.";
        private const string DISTANCE_WARNING = "Small distance may create many colliders.";
        private const string COUNT_WARNING = "High segments count may create many colliders.";

        private SplineCollider _collider;

        private SerializedProperty _segmentShapeProp;
        private SerializedProperty _samplingModeProp;
        private SerializedProperty _postProcessingProp;

        private SerializedProperty _colliderRootProp;
        private SerializedProperty _segmentLengthProp;
        private SerializedProperty _segmentCountProp;

        private SerializedProperty _minBendAngleProp;
        private SerializedProperty _maxSubdivisionDepthProp;

        private SerializedProperty _radiusProp;
        private SerializedProperty _isTriggerProp;

        private bool _needsRebake = true;
        private bool _baked = false;

        private void OnEnable()
        {
            _collider = target as SplineCollider;
            _baked = _collider.HasBakedColliders;

            _colliderRootProp = serializedObject.FindProperty("_colliderRoot");

            _segmentShapeProp = serializedObject.FindProperty("_segmentShape");
            _samplingModeProp = serializedObject.FindProperty("_samplingMode");
            _segmentLengthProp = serializedObject.FindProperty("_segmentLength");
            _segmentCountProp = serializedObject.FindProperty("_segmentCount");

            _postProcessingProp = serializedObject.FindProperty("_postProcessing");
            _minBendAngleProp = serializedObject.FindProperty("_minBendAngle");
            _maxSubdivisionDepthProp = serializedObject.FindProperty("_maxSubdivisionDepth");

            _radiusProp = serializedObject.FindProperty("_radius");
            _isTriggerProp = serializedObject.FindProperty("_isTrigger");

            
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            _baked = _collider.HasBakedColliders;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Container Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_colliderRootProp, new GUIContent("Collider Root"));
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Sampling settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_samplingModeProp, new GUIContent("Sample by"));

            SamplingMode samplingMode = (SamplingMode)_samplingModeProp.enumValueIndex;

            switch (samplingMode)
            {
                case SamplingMode.Distance:
                    EditorGUILayout.PropertyField(_segmentLengthProp, new GUIContent("Distance"));
                    break;
                case SamplingMode.Count:
                    EditorGUILayout.PropertyField(_segmentCountProp, new GUIContent("Segments"));
                    break;
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Post-processing Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_postProcessingProp, new GUIContent("Post-processing actions"));

            EditorGUILayout.PropertyField(_minBendAngleProp, new GUIContent("Min Bend Angle"));

            var postProcessFlags = (ColliderPostProcess)_postProcessingProp.intValue;
            if ((postProcessFlags & ColliderPostProcess.SubdivideSharpBends) != 0)
                EditorGUILayout.PropertyField(_maxSubdivisionDepthProp, new GUIContent("Max Subdivision Depth"));

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Collider Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_segmentShapeProp, new GUIContent("Shape"));
            EditorGUILayout.PropertyField(_radiusProp, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(_isTriggerProp, new GUIContent("Is Trigger"));
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            if (changed)
                _needsRebake = true;

            EditorGUILayout.BeginVertical(GUI.skin.box);

            var defaultColor = GUI.backgroundColor;
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            GUI.backgroundColor = Color.limeGreen;
            if (GUILayout.Button("Bake", GUILayout.Height(25)))
                OnBake();

            GUI.backgroundColor = Color.orange;
            if (GUILayout.Button("Clear", GUILayout.Height(25)))
                OnClear();
            GUI.backgroundColor = defaultColor;

            EditorGUILayout.EndVertical();


            EditorGUILayout.Space();


            // Message conditions
            bool bakeRequired = !_baked;
            bool rebakeRequired = _baked && _needsRebake;
            bool noPostProcessing = postProcessFlags == ColliderPostProcess.None;

            bool showDensityWarning =
                noPostProcessing &&
                ((samplingMode == SamplingMode.Distance && _segmentLengthProp.floatValue < 0.5f) ||
                 (samplingMode == SamplingMode.Count && _segmentCountProp.intValue >= 50));

            bool hasAnyMessage = bakeRequired || rebakeRequired || showDensityWarning;

            if (hasAnyMessage)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Messages", EditorStyles.boldLabel);

                // Density warning (only when no post-processing)
                if (showDensityWarning)
                {
                    string warningText =
                        samplingMode == SamplingMode.Distance
                            ? $"{DISTANCE_WARNING} {POST_PROCESSING_WARNING}"
                            : $"{COUNT_WARNING} {POST_PROCESSING_WARNING}";

                    EditorGUILayout.HelpBox(warningText, MessageType.Warning);
                }

                // Bake / rebake messaging (priority: bake required > rebake)
                if (bakeRequired)
                    EditorGUILayout.HelpBox("Bake required: colliders are not baked.", MessageType.Error);
                else if (rebakeRequired)
                    EditorGUILayout.HelpBox("Rebake might be required: settings might have been modified.", MessageType.Info);

                EditorGUILayout.EndVertical();
            }
        }

        private void OnBake()
        {
            _collider.Bake();
            _needsRebake = false;
        }

        private void OnClear()
        {
            _collider.ClearBakedSegments();
            _needsRebake = false;
        }
    }
}