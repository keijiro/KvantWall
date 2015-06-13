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
        #region Basic Properties

        [SerializeField]
        int _columns = 50;

        public int columns {
            get { return _columns; }
            set { _columns = value; }
        }

        [SerializeField]
        int _rows = 50;

        public int rows {
            get { return _rows; }
            set { _rows = value; }
        }

        [SerializeField]
        Vector2 _extent = new Vector2(100, 100);

        public Vector2 extent {
            get { return _extent; }
            set { _extent = value; }
        }

        [SerializeField]
        Vector2 _offset = Vector2.zero;

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        #endregion

        #region Noise-To-Position Parameters

        public enum PositionNoiseMode { Disabled, ZOnly, XYZ, Random }

        [SerializeField]
        PositionNoiseMode _positionNoiseMode = PositionNoiseMode.ZOnly;

        public PositionNoiseMode positionNoiseMode {
            get { return _positionNoiseMode; }
            set { _positionNoiseMode = value; }
        }

        [SerializeField]
        float _positionNoiseAmplitude = 5.0f;

        public float positionNoiseAmplitude {
            get { return _positionNoiseAmplitude; }
            set { _positionNoiseAmplitude = value; }
        }

        [SerializeField]
        float _positionNoiseFrequency = 1.0f;

        public float positionNoiseFrequency {
            get { return _positionNoiseFrequency; }
            set { _positionNoiseFrequency = value; }
        }

        [SerializeField]
        float _positionNoiseSpeed = 0.2f;

        public float positionNoiseSpeed {
            get { return _positionNoiseSpeed; }
            set { _positionNoiseSpeed = value; }
        }

        #endregion

        #region Noise-To-Rotation Parameters

        public enum RotationNoiseMode { Disabled, XAxis, YAxis, ZAxis, Random }

        [SerializeField]
        RotationNoiseMode _rotationNoiseMode = RotationNoiseMode.Disabled;

        public RotationNoiseMode rotationNoiseMode {
            get { return _rotationNoiseMode; }
            set { _rotationNoiseMode = value; }
        }

        [SerializeField]
        float _rotationNoiseAmplitude = 45.0f;

        public float rotationNoiseAmplitude {
            get { return _rotationNoiseAmplitude; }
            set { _rotationNoiseAmplitude = value; }
        }

        [SerializeField]
        float _rotationNoiseFrequency = 1.0f;

        public float rotationNoiseFrequency {
            get { return _rotationNoiseFrequency; }
            set { _rotationNoiseFrequency = value; }
        }

        [SerializeField]
        float _rotationNoiseSpeed = 0.2f;

        public float rotationNoiseSpeed {
            get { return _rotationNoiseSpeed; }
            set { _rotationNoiseSpeed = value; }
        }

        #endregion

        #region Noise-To-Scale Parameters

        public enum ScaleNoiseMode { Disabled, Uniform, XYZ }

        [SerializeField]
        ScaleNoiseMode _scaleNoiseMode = ScaleNoiseMode.Disabled;

        public ScaleNoiseMode scaleNoiseMode {
            get { return _scaleNoiseMode; }
            set { _scaleNoiseMode = value; }
        }

        [SerializeField, Range(0, 1)]
        float _scaleNoiseAmplitude = 0.5f;

        public float scaleNoiseAmplitude {
            get { return _scaleNoiseAmplitude; }
            set { _scaleNoiseAmplitude = value; }
        }

        [SerializeField]
        float _scaleNoiseFrequency = 1.0f;

        public float scaleNoiseFrequency {
            get { return _scaleNoiseFrequency; }
            set { _scaleNoiseFrequency = value; }
        }

        [SerializeField]
        float _scaleNoiseSpeed = 0.2f;

        public float scaleNoiseSpeed {
            get { return _scaleNoiseSpeed; }
            set { _scaleNoiseSpeed = value; }
        }

        #endregion

        #region Render Settings

        [SerializeField]
        Mesh[] _shapes;

        [SerializeField]
        Vector3 _baseScale = Vector3.one;

        public Vector3 baseScale {
            get { return _baseScale; }
            set { _baseScale = value; }
        }

        [SerializeField]
        float _minRandomScale = 0.8f;

        public float minRandomScale {
            get { return _minRandomScale; }
            set { _minRandomScale = value; }
        }

        [SerializeField]
        float _maxRandomScale = 1.0f;

        public float maxRandomScale {
            get { return _maxRandomScale; }
            set { _maxRandomScale = value; }
        }

        [SerializeField]
        Material _material;

        public Material sharedMaterial {
            get { return _material; }
            set { _material = value; }
        }

        [SerializeField]
        ShadowCastingMode _castShadows;

        public ShadowCastingMode castShadows {
            get { return _castShadows; }
            set { _castShadows = value; }
        }

        [SerializeField]
        bool _receiveShadows = false;

        public bool receiveShadows {
            get { return _receiveShadows; }
            set { _receiveShadows = value; }
        }

        #endregion

        #region Editor Properties

        [SerializeField]
        bool _debug;

        #endregion

        #region Built-in Default Resources

        [SerializeField] Mesh _defaultShape;
        [SerializeField] Material _defaultMaterial;
        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _debugShader;

        #endregion

        #region Private Variables

        RenderTexture _positionBuffer;
        RenderTexture _rotationBuffer;
        RenderTexture _scaleBuffer;
        BulkMesh _bulkMesh;
        Material _kernelMaterial;
        Material _debugMaterial;
        bool _needsReset = true;

        #endregion

        #region Local Properties

        Mesh[] SourceShapes {
            get {
                if (_shapes != null)
                    foreach (var m in _shapes)
                        if (m != null) return _shapes;
                return new Mesh[]{ _defaultShape };
            }
        }

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

            // Shader uniforms

            m.SetVector("_ColumnRow", new Vector2(_columns, _rows));
            m.SetVector("_Extent", _extent);
            m.SetVector("_UVOffset", new Vector2(_offset.x / _extent.x, _offset.y / _extent.y));
            m.SetVector("_BaseScale", _baseScale);
            m.SetVector("_RandomScale", new Vector2(_minRandomScale, _maxRandomScale));

            var nf = new Vector3(
                _positionNoiseFrequency,
                _rotationNoiseFrequency,
                _scaleNoiseFrequency);
            m.SetVector("_NoiseFrequency", nf);

            var ns = new Vector3(
                _positionNoiseSpeed,
                _rotationNoiseSpeed,
                _scaleNoiseSpeed);
            m.SetVector("_NoiseTime", ns * Time.time);

            var no_position = (_positionNoiseMode == PositionNoiseMode.Disabled);
            var no_rotation = (_rotationNoiseMode == RotationNoiseMode.Disabled);
            var no_scale    = (_scaleNoiseMode    == ScaleNoiseMode.Disabled);
            var na = new Vector3(
                no_position ? 0.0f : _positionNoiseAmplitude,
                no_rotation ? 0.0f : _rotationNoiseAmplitude * Mathf.Deg2Rad,
                no_scale    ? 0.0f : _scaleNoiseAmplitude);
            m.SetVector("_NoiseAmplitude", na);

            // Shader keywords

            if (_positionNoiseMode == PositionNoiseMode.XYZ)
            {
                m.EnableKeyword("POSITION_XYZ");
                m.DisableKeyword("POSITION_RANDOM");
            }
            else if (_positionNoiseMode == PositionNoiseMode.Random)
            {
                m.DisableKeyword("POSITION_XYZ");
                m.EnableKeyword("POSITION_RANDOM");
            }
            else
            {
                m.DisableKeyword("POSITION_XYZ");
                m.DisableKeyword("POSITION_RANDOM");
            }

            if (_rotationNoiseMode == RotationNoiseMode.Random)
            {
                m.EnableKeyword("ROTATION_RANDOM");
            }
            else
            {
                m.DisableKeyword("ROTATION_RANDOM");
                if (_rotationNoiseMode == RotationNoiseMode.XAxis)
                    m.SetVector("_RotationAxis", Vector3.right);
                else if (_rotationNoiseMode == RotationNoiseMode.YAxis)
                    m.SetVector("_RotationAxis", Vector3.up);
                else // ZAxis or Disabled
                    m.SetVector("_RotationAxis", Vector3.forward);
            }

            if (_scaleNoiseMode == ScaleNoiseMode.XYZ)
                m.EnableKeyword("SCALE_XYZ");
            else
                m.DisableKeyword("SCALE_XYZ");
        }

        void ResetResources()
        {
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(SourceShapes, _columns);
            else
                _bulkMesh.Rebuild(SourceShapes, _columns);

            if (_positionBuffer) DestroyImmediate(_positionBuffer);
            if (_rotationBuffer) DestroyImmediate(_rotationBuffer);
            if (_scaleBuffer) DestroyImmediate(_scaleBuffer);

            _positionBuffer = CreateBuffer();
            _rotationBuffer = CreateBuffer();
            _scaleBuffer = CreateBuffer();

            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);
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
            if (_debugMaterial) DestroyImmediate(_debugMaterial);
        }

        void Update()
        {
            if (_needsReset) ResetResources();

            UpdateKernelShader();

            Graphics.Blit(null, _positionBuffer, _kernelMaterial, 0);
            Graphics.Blit(null, _rotationBuffer, _kernelMaterial, 1);
            Graphics.Blit(null, _scaleBuffer, _kernelMaterial, 2);

            var p = transform.position;
            var r = transform.rotation;
            var uv = new Vector2(0.5f / _positionBuffer.width, 0);
            var m = _material ? _material : _defaultMaterial;
            var block = new MaterialPropertyBlock();

            block.AddTexture("_PositionTex", _positionBuffer);
            block.AddTexture("_RotationTex", _rotationBuffer);
            block.AddTexture("_ScaleTex", _scaleBuffer);
            block.SetVector("_ColumnRow", new Vector2(_columns, _rows));
            block.SetVector("_UVOffset", new Vector2(_offset.x / _extent.x, _offset.y / _extent.y));

            for (var i = 0; i < _positionBuffer.height; i++)
            {
                uv.y = (0.5f + i) / _positionBuffer.height;
                block.AddVector("_BufferOffset", uv);
                Graphics.DrawMesh(_bulkMesh.mesh, p, r, m, 0, null, 0, block, _castShadows, _receiveShadows);
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
