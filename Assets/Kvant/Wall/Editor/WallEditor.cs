//
// Custom editor class for Wall
//
using UnityEngine;
using UnityEditor;

namespace Kvant
{
    [CustomEditor(typeof(Wall)), CanEditMultipleObjects]
    public class WallEditor : Editor
    {
        SerializedProperty _columns;
        SerializedProperty _rows;
        SerializedProperty _extent;

        SerializedProperty _noiseFrequency;
        SerializedProperty _noiseSpeed;
        SerializedProperty _noiseOffset;

        SerializedProperty _noiseToPosition;
        SerializedProperty _positionMode;

        SerializedProperty _noiseToRotation;
        SerializedProperty _rotationMode;

        SerializedProperty _noiseToScale;
        SerializedProperty _scaleMode;

        SerializedProperty _shapes;
        SerializedProperty _minScale;
        SerializedProperty _maxScale;

        SerializedProperty _colorMode;
        SerializedProperty _color;
        SerializedProperty _color2;
        SerializedProperty _metallic;
        SerializedProperty _smoothness;

        SerializedProperty _albedoMap;
        SerializedProperty _normalMap;
        SerializedProperty _occlusionMap;
        SerializedProperty _occlusionStrength;
        SerializedProperty _textureScale;
        SerializedProperty _textureOffset;
        SerializedProperty _textureRandomOffset;

        SerializedProperty _castShadows;
        SerializedProperty _receiveShadows;

        SerializedProperty _randomSeed;
        SerializedProperty _debug;

        static GUIContent _textFrequency    = new GUIContent("Frequency");
        static GUIContent _textSpeed        = new GUIContent("Speed");
        static GUIContent _textOffset       = new GUIContent("Offset");
        static GUIContent _textScale        = new GUIContent("Scale");
        static GUIContent _textRandomOffset = new GUIContent("Random Offset");
        static GUIContent _textEmpty        = new GUIContent(" ");
        static GUIContent _textNull         = new GUIContent("");

        void OnEnable()
        {
            _columns = serializedObject.FindProperty("_columns");
            _rows    = serializedObject.FindProperty("_rows");
            _extent  = serializedObject.FindProperty("_extent");

            _noiseFrequency = serializedObject.FindProperty("_noiseFrequency");
            _noiseSpeed     = serializedObject.FindProperty("_noiseSpeed");
            _noiseOffset    = serializedObject.FindProperty("_noiseOffset");

            _noiseToPosition = serializedObject.FindProperty("_noiseToPosition");
            _positionMode    = serializedObject.FindProperty("_positionMode");

            _noiseToRotation = serializedObject.FindProperty("_noiseToRotation");
            _rotationMode    = serializedObject.FindProperty("_rotationMode");

            _noiseToScale = serializedObject.FindProperty("_noiseToScale");
            _scaleMode    = serializedObject.FindProperty("_scaleMode");

            _shapes   = serializedObject.FindProperty("_shapes");
            _minScale = serializedObject.FindProperty("_minScale");
            _maxScale = serializedObject.FindProperty("_maxScale");

            _colorMode  = serializedObject.FindProperty("_colorMode");
            _color      = serializedObject.FindProperty("_color");
            _color2     = serializedObject.FindProperty("_color2");
            _metallic   = serializedObject.FindProperty("_metallic");
            _smoothness = serializedObject.FindProperty("_smoothness");

            _albedoMap           = serializedObject.FindProperty("_albedoMap");
            _normalMap           = serializedObject.FindProperty("_normalMap");
            _occlusionMap        = serializedObject.FindProperty("_occlusionMap");
            _occlusionStrength   = serializedObject.FindProperty("_occlusionStrength");
            _textureScale        = serializedObject.FindProperty("_textureScale");
            _textureOffset       = serializedObject.FindProperty("_textureOffset");
            _textureRandomOffset = serializedObject.FindProperty("_textureRandomOffset");

            _castShadows    = serializedObject.FindProperty("_castShadows");
            _receiveShadows = serializedObject.FindProperty("_receiveShadows");

            _randomSeed = serializedObject.FindProperty("_randomSeed");
            _debug      = serializedObject.FindProperty("_debug");
        }

        public override void OnInspectorGUI()
        {
            var targetWall = target as Wall;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_columns);
            EditorGUILayout.PropertyField(_rows);

            if (EditorGUI.EndChangeCheck())
                targetWall.NotifyConfigChange();

            EditorGUILayout.PropertyField(_extent);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Turbulent Noise", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_noiseFrequency, _textFrequency);
            EditorGUILayout.PropertyField(_noiseSpeed, _textSpeed);
            EditorGUILayout.PropertyField(_noiseOffset, _textOffset);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noiseToPosition);
            if (_noiseToPosition.hasMultipleDifferentValues || _noiseToPosition.floatValue > 0)
                EditorGUILayout.PropertyField(_positionMode, _textEmpty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noiseToRotation);
            if (_noiseToRotation.hasMultipleDifferentValues || _noiseToRotation.floatValue > 0)
                EditorGUILayout.PropertyField(_rotationMode, _textEmpty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noiseToScale);
            if (_noiseToScale.hasMultipleDifferentValues || _noiseToScale.floatValue > 0)
                EditorGUILayout.PropertyField(_scaleMode, _textEmpty);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_shapes, true);

            if (EditorGUI.EndChangeCheck())
                targetWall.NotifyConfigChange();

            MinMaxSlider(_textScale, _minScale, _maxScale, 0.01f, 2.0f);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(_colorMode);
            if (_colorMode.hasMultipleDifferentValues || _colorMode.enumValueIndex != 0)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(_textEmpty);
                EditorGUILayout.PropertyField(_color, _textNull);
                EditorGUILayout.PropertyField(_color2, _textNull);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
            }
            else
            {
                EditorGUILayout.PropertyField(_color, _textEmpty);
            }

            EditorGUILayout.PropertyField(_metallic);
            EditorGUILayout.PropertyField(_smoothness);
            EditorGUILayout.PropertyField(_albedoMap);
            EditorGUILayout.PropertyField(_normalMap);
            EditorGUILayout.PropertyField(_occlusionMap);
            if (_occlusionMap.hasMultipleDifferentValues || _occlusionMap.objectReferenceValue)
                EditorGUILayout.PropertyField(_occlusionStrength, _textEmpty);


            if (_albedoMap.hasMultipleDifferentValues || _albedoMap.objectReferenceValue ||
                _normalMap.hasMultipleDifferentValues || _normalMap.objectReferenceValue ||
                _occlusionMap.hasMultipleDifferentValues || _occlusionMap.objectReferenceValue)
            {
                EditorGUILayout.PropertyField(_textureScale);
                EditorGUILayout.PropertyField(_textureOffset);
                EditorGUILayout.PropertyField(_textureRandomOffset, _textRandomOffset);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_castShadows);
            EditorGUILayout.PropertyField(_receiveShadows);

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_randomSeed);
            EditorGUILayout.PropertyField(_debug);

            serializedObject.ApplyModifiedProperties();
        }

        void MinMaxSlider(GUIContent label, SerializedProperty propMin, SerializedProperty propMax, float minLimit, float maxLimit)
        {
            var min = propMin.floatValue;
            var max = propMax.floatValue;

            EditorGUI.BeginChangeCheck();

            // Min-max slider.
            EditorGUILayout.MinMaxSlider(label, ref min, ref max, minLimit, maxLimit);

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Float value boxes.
            var rect = EditorGUILayout.GetControlRect();
            rect.x += EditorGUIUtility.labelWidth;
            rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;

            if (EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.labelWidth = 28;
                min = Mathf.Clamp(EditorGUI.FloatField(rect, "min", min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, "max", max), min, maxLimit);
                EditorGUIUtility.labelWidth = 0;
            }
            else
            {
                min = Mathf.Clamp(EditorGUI.FloatField(rect, min), minLimit, max);
                rect.x += rect.width + 4;
                max = Mathf.Clamp(EditorGUI.FloatField(rect, max), min, maxLimit);
            }

            EditorGUI.indentLevel = prevIndent;

            if (EditorGUI.EndChangeCheck()) {
                propMin.floatValue = min;
                propMax.floatValue = max;
            }
        }
    }
}
