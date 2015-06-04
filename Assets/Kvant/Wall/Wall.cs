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
        #region Parameters Exposed To Editor

        [SerializeField]
        int _columns = 50;

        [SerializeField]
        int _rows = 50;

        [SerializeField]
        Vector2 _size = new Vector2(10, 10);

        [SerializeField]
        float _noiseFrequency = 0.2f;

        [SerializeField]
        float _noiseAmplitude = 5.0f;

        [SerializeField]
        float _noiseAnimation = 1.0f;

        [SerializeField]
        Mesh[] _shapes = new Mesh[1];

        [SerializeField, Range(0, 1)]
        float _metallic = 0.5f;

        [SerializeField, Range(0, 1)]
        float _smoothness = 0.5f;

        [SerializeField]
        ShadowCastingMode _castShadows;

        [SerializeField]
        bool _receiveShadows = false;

        public enum ColorMode { Single, Random, Animation }

        [SerializeField] ColorMode _colorMode;

        [SerializeField, ColorUsage(true, true, 0, 8, 0.125f, 3)]
        Color _color = Color.white;

        [SerializeField, ColorUsage(true, true, 0, 8, 0.125f, 3)]
        Color _color2 = Color.red;

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
            var np = new Vector3(_noiseFrequency, _noiseAmplitude, _noiseAnimation);
            m.SetVector("_Size", _size);
            m.SetVector("_NoiseParams", np);
            m.SetVector("_Config", new Vector4(_randomSeed, Time.time, 0, 0));
        }

        void UpdateDisplayShader()
        {
            var m = _displayMaterial;
            m.SetTexture("_PositionTex", _positionBuffer);
            m.SetTexture("_RotationTex", _rotationBuffer);
            m.SetTexture("_ScaleTex", _scaleBuffer);
            m.SetVector("_PbrParams", new Vector2(_metallic, _smoothness));
            m.SetColor("_Color", _color);
            m.SetColor("_Color2", _color2);
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
