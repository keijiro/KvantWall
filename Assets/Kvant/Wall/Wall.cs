//
// Wall - mesh object array
//
using UnityEngine;
using UnityEngine.Rendering;

namespace Kvant
{
    [ExecuteInEditMode, AddComponentMenu("Kvant/Wall")]
    public partial class Wall : MonoBehaviour
    {
        #region Public Type Definitions

        public enum PositionMode { ZOnly, XYZ, Random }
        public enum RotationMode { XAxis, YAxis, ZAxis, Random }
        public enum ScaleMode { Uniform, XYZ }
        public enum ColorMode { Single, Random, Animation }

        #endregion

        #region Parameters Exposed To Editor

        [SerializeField]
        int _columns = 80;

        [SerializeField]
        int _rows = 80;

        [SerializeField]
        Vector2 _extent = new Vector2(100, 100);

        [SerializeField]
        float _noiseFrequency = 0.2f;

        [SerializeField]
        float _noiseSpeed = 0.2f;

        [SerializeField]
        Vector2 _noiseOffset = Vector2.zero;

        [SerializeField, Range(0, 8)]
        float _noiseToPosition = 1.0f;

        [SerializeField]
        PositionMode _positionMode = PositionMode.ZOnly;

        [SerializeField, Range(0, 180)]
        float _noiseToRotation = 0.0f;

        [SerializeField]
        RotationMode _rotationMode = RotationMode.Random;

        [SerializeField, Range(0, 1)]
        float _noiseToScale = 0.0f;

        [SerializeField]
        ScaleMode _scaleMode = ScaleMode.Uniform;

        [SerializeField]
        Mesh[] _shapes = new Mesh[1];

        [SerializeField]
        float _minScale = 0.8f;

        [SerializeField]
        float _maxScale = 1.2f;

        [SerializeField] ColorMode _colorMode;

        [SerializeField]
        Color _color = Color.white;

        [SerializeField]
        Color _color2 = Color.red;

        [SerializeField, Range(0, 1)]
        float _metallic = 0.5f;

        [SerializeField, Range(0, 1)]
        float _smoothness = 0.5f;

        [SerializeField]
        Texture2D _albedoMap;

        [SerializeField]
        Texture2D _normalMap;

        [SerializeField]
        Texture2D _occlusionMap;

        [SerializeField, Range(0, 1)]
        float _occlusionStrength;

        [SerializeField]
        Vector2 _textureScale = Vector2.one;

        [SerializeField]
        Vector2 _textureOffset = Vector2.zero;

        [SerializeField]
        bool _textureRandomOffset = false;

        [SerializeField]
        ShadowCastingMode _castShadows;

        [SerializeField]
        bool _receiveShadows = false;

        [SerializeField]
        int _randomSeed = 0;

        [SerializeField]
        bool _debug;

        #endregion

        #region Shader And Materials

        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _displayShader;
        [SerializeField] Shader _debugShader;

        Material _kernelMaterial;
        Material _displayMaterial;
        Material _debugMaterial;

        #endregion

        #region Private Variables And Objects

        RenderTexture _positionBuffer;
        RenderTexture _rotationBuffer;
        RenderTexture _scaleBuffer;
        BulkMesh _bulkMesh;
        bool _needsReset = true;

        #endregion

        #region Resource Management

        public void NotifyConfigChange()
        {
            _needsReset = true;
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var buffer = new RenderTexture(_columns, _rows, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;

            m.SetVector("_Extent", _extent);

            var np = new Vector3(_noiseOffset.x, _noiseOffset.y, _noiseFrequency);
            m.SetVector("_NoiseParams", np);

            var ni = new Vector3(_noiseToPosition, Mathf.Deg2Rad * _noiseToRotation, _noiseToScale);
            m.SetVector("_NoiseInfluence", ni);

            m.SetVector("_ScaleParams", new Vector2(_minScale, _maxScale));

            m.SetVector("_Config", new Vector2(_randomSeed, Time.time));

            if (_positionMode == PositionMode.ZOnly)
            {
                m.DisableKeyword("POSITION_XYZ");
                m.DisableKeyword("POSITION_RANDOM");
            }
            else if (_positionMode == PositionMode.XYZ) 
            {
                m.EnableKeyword("POSITION_XYZ");
                m.DisableKeyword("POSITION_RANDOM");
            }
            else // PositionMode.Random
            {
                m.DisableKeyword("POSITION_XYZ");
                m.EnableKeyword("POSITION_RANDOM");
            }

            if (_rotationMode == RotationMode.Random)
            {
                m.EnableKeyword("ROTATION_RANDOM");
            }
            else
            {
                m.DisableKeyword("ROTATION_RANDOM");
                if (_rotationMode == RotationMode.XAxis)
                    m.SetVector("_RotationAxis", Vector3.right);
                else if (_rotationMode == RotationMode.YAxis)
                    m.SetVector("_RotationAxis", Vector3.up);
                else
                    m.SetVector("_RotationAxis", Vector3.forward);
            }

            if (_scaleMode == ScaleMode.Uniform)
            {
                m.DisableKeyword("SCALE_XYZ");
            }
            else // ScaleMode.XYZ
            {
                m.EnableKeyword("SCALE_XYZ");
            }
        }

        void UpdateDisplayShader()
        {
            var m = _displayMaterial;

            m.SetTexture("_PositionTex", _positionBuffer);
            m.SetTexture("_RotationTex", _rotationBuffer);
            m.SetTexture("_ScaleTex", _scaleBuffer);

            if (_colorMode == ColorMode.Random)
            {
                m.SetColor("_Color", _color);
                m.SetColor("_Color2", _color2);
                m.EnableKeyword("COLOR_RANDOM");
            }
            else
            {
                m.SetColor("_Color", _color);
                m.SetColor("_Color2", _colorMode == ColorMode.Single ? _color : _color2);
                m.DisableKeyword("COLOR_RANDOM");
            }

            m.SetVector("_PbrParams", new Vector2(_metallic, _smoothness));

            m.mainTexture = _albedoMap;
            m.SetTexture("_BumpMap", _normalMap);
            m.SetTexture("_OcclusionMap", _occlusionMap);
            m.SetFloat("_OcclusionStrength", _occlusionStrength);

            if (_occlusionMap)
            {
                m.DisableKeyword("ALBEDO_ONLY");
                m.DisableKeyword("ALBEDO_NORMAL");
                m.EnableKeyword("ALBEDO_NORMAL_OCCLUSION");
            }
            else if (_normalMap)
            {
                m.DisableKeyword("ALBEDO_ONLY");
                m.EnableKeyword("ALBEDO_NORMAL");
                m.DisableKeyword("ALBEDO_NORMAL_OCCLUSION");
            }
            else if (_albedoMap)
            {
                m.EnableKeyword("ALBEDO_ONLY");
                m.DisableKeyword("ALBEDO_NORMAL");
                m.DisableKeyword("ALBEDO_NORMAL_OCCLUSION");
            }
            else
            {
                m.DisableKeyword("ALBEDO_ONLY");
                m.DisableKeyword("ALBEDO_NORMAL");
                m.DisableKeyword("ALBEDO_NORMAL_OCCLUSION");
            }

            m.mainTextureScale = _textureScale;
            m.mainTextureOffset = _textureOffset;

            if (_textureRandomOffset)
                m.EnableKeyword("UV_RANDOM");
            else
                m.DisableKeyword("UV_RANDOM");
        }

        void ResetResources()
        {
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_shapes, _columns);
            else
                _bulkMesh.Rebuild(_shapes, _columns);

            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer) DestroyImmediate(_scaleBuffer);

            _positionBuffer = CreateBuffer();
            _rotationBuffer = CreateBuffer();
            _scaleBuffer = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);
            if (!_displayMaterial) _displayMaterial = CreateMaterial(_displayShader);
            if (!_debugMaterial) _debugMaterial = CreateMaterial(_debugShader);

            _needsReset = false;
        }

        #endregion

        #region MonoBehaviour Functions

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_bulkMesh != null) _bulkMesh.Release();
            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer) DestroyImmediate(_scaleBuffer);
            if (_kernelMaterial) DestroyImmediate(_kernelMaterial);
            if (_displayMaterial) DestroyImmediate(_displayMaterial);
            if (_debugMaterial) DestroyImmediate(_debugMaterial);
        }

        void Update()
        {
            if (_needsReset) ResetResources();

            UpdateKernelShader();

            Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
            Graphics.Blit(null, _rotationBuffer, _kernelMaterial, 1);
            Graphics.Blit(null, _scaleBuffer, _kernelMaterial, 2);

            UpdateDisplayShader();

            var p = transform.position;
            var r = transform.rotation;
            var uv = new Vector2(0.5f / _positionBuffer.width, 0);
            var offs = new MaterialPropertyBlock();

            for (var i = 0; i < _positionBuffer.height; i++)
            {
                uv.y = (0.5f + i) / _positionBuffer.height;
                offs.AddVector("_BufferOffset", uv);
                Graphics.DrawMesh(_bulkMesh.mesh, p, r, _displayMaterial, 0, null, 0, offs, _castShadows, _receiveShadows);
            }
        }

        void OnGUI()
        {
            if (_debug && Event.current.type.Equals(EventType.Repaint) && _debugMaterial)
            {
                var r1 = new Rect(0, 0, _columns, _rows);
                var r2 = new Rect(0, _rows, _columns, _rows);
                var r3 = new Rect(0, _rows * 2, _columns, _rows);
                if (_positionBuffer) Graphics.DrawTexture(r1, _positionBuffer, _debugMaterial);
                if (_rotationBuffer) Graphics.DrawTexture(r2, _rotationBuffer, _debugMaterial);
                if (_scaleBuffer) Graphics.DrawTexture(r3, _scaleBuffer, _debugMaterial);
            }
        }

        #endregion
    }
}
