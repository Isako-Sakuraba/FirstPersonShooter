#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.EditorTools
{
    public static class AudioMixerExposedParamsDebug
    {
        [MenuItem("Tools/Audio/Print Exposed Params")]
        private static void PrintExposedParams()
        {
            Object selected = Selection.activeObject;
            if (selected is not AudioMixer mixer)
            {
                Debug.LogWarning("Select an AudioMixer asset in Project first.");
                return;
            }

            SerializedObject serializedMixer = new SerializedObject(mixer);
            SerializedProperty exposedArray = serializedMixer.FindProperty("m_ExposedParameters");
            if (exposedArray == null || !exposedArray.isArray)
            {
                Debug.LogWarning($"Could not read exposed parameters for '{mixer.name}'.");
                return;
            }

            Debug.Log($"Exposed parameters in '{mixer.name}' ({exposedArray.arraySize}):");

            for (int i = 0; i < exposedArray.arraySize; i++)
            {
                SerializedProperty item = exposedArray.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = item.FindPropertyRelative("name");
                string paramName = nameProp != null ? nameProp.stringValue : "<unknown>";
                Debug.Log($"[{i}] {paramName}");
            }
        }
    }
}
#endif
