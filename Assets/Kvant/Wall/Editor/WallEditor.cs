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

        SerializedProperty _positionNoiseMode;
        SerializedProperty _positionNoiseAmplitude;
        SerializedProperty _positionNoiseFrequency;
        SerializedProperty _positionNoiseSpeed;

        SerializedProperty _rotationNoiseMode;
        SerializedProperty _rotationNoiseAmplitude;
        SerializedProperty _rotationNoiseFrequency;
        SerializedProperty _rotationNoiseSpeed;

        SerializedProperty _scaleNoiseMode;
        SerializedProperty _scaleNoiseAmplitude;
        SerializedProperty _scaleNoiseFrequency;
        SerializedProperty _scaleNoiseSpeed;

        SerializedProperty _noiseOffset;

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

        static GUIContent _textPositionNoise = new GUIContent("Noise To Position");
        static GUIContent _textRotationNoise = new GUIContent("Noise To Rotation");
        static GUIContent _textScaleNoise    = new GUIContent("Noise To Scale");
        static GUIContent _textAmplitude     = new GUIContent("Amplitude");
        static GUIContent _textFrequency     = new GUIContent("Frequency");
        static GUIContent _textSpeed         = new GUIContent("Speed");
        static GUIContent _textScale         = new GUIContent("Scale");
        static GUIContent _textRandomOffset  = new GUIContent("Random Offset");
        static GUIContent _textEmpty         = new GUIContent(" ");
        static GUIContent _textNull          = new GUIContent("");

        void OnEnable()
        {
            _columns = serializedObject.FindProperty("_columns");
            _rows    = serializedObject.FindProperty("_rows");
            _extent  = serializedObject.FindProperty("_extent");

            _positionNoiseMode      = serializedObject.FindProperty("_positionNoiseMode");
            _positionNoiseAmplitude = serializedObject.FindProperty("_positionNoiseAmplitude");
            _positionNoiseFrequency = serializedObject.FindProperty("_positionNoiseFrequency");
            _positionNoiseSpeed     = serializedObject.FindProperty("_positionNoiseSpeed");

            _rotationNoiseMode      = serializedObject.FindProperty("_rotationNoiseMode");
            _rotationNoiseAmplitude = serializedObject.FindProperty("_rotationNoiseAmplitude");
            _rotationNoiseFrequency = serializedObject.FindProperty("_rotationNoiseFrequency");
            _rotationNoiseSpeed     = serializedObject.FindProperty("_rotationNoiseSpeed");

            _scaleNoiseMode      = serializedObject.FindProperty("_scaleNoiseMode");
            _scaleNoiseAmplitude = serializedObject.FindProperty("_scaleNoiseAmplitude");
            _scaleNoiseFrequency = serializedObject.FindProperty("_scaleNoiseFrequency");
            _scaleNoiseSpeed     = serializedObject.FindProperty("_scaleNoiseSpeed");

            _noiseOffset = serializedObject.FindProperty("_noiseOffset");

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

            EditorGUILayout.PropertyField(_positionNoiseMode, _textPositionNoise);
            if (_positionNoiseMode.hasMultipleDifferentValues || _positionNoiseMode.enumValueIndex > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_positionNoiseAmplitude, _textAmplitude);
                EditorGUILayout.PropertyField(_positionNoiseFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_positionNoiseSpeed, _textSpeed);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_rotationNoiseMode, _textRotationNoise);
            if (_rotationNoiseMode.hasMultipleDifferentValues || _rotationNoiseMode.enumValueIndex > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_rotationNoiseAmplitude, _textAmplitude);
                EditorGUILayout.PropertyField(_rotationNoiseFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_rotationNoiseSpeed, _textSpeed);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_scaleNoiseMode, _textScaleNoise);
            if (_scaleNoiseMode.hasMultipleDifferentValues || _scaleNoiseMode.enumValueIndex > 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_scaleNoiseAmplitude, _textAmplitude);
                EditorGUILayout.PropertyField(_scaleNoiseFrequency, _textFrequency);
                EditorGUILayout.PropertyField(_scaleNoiseSpeed, _textSpeed);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_noiseOffset);

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_shapes, true);

            if (EditorGUI.EndChangeCheck())
                targetWall.NotifyConfigChange();

            MinMaxSlider(_textScale, _minScale, _maxScale, 0.01f, 2.0f);

            EditorGUILayout.Space();

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
