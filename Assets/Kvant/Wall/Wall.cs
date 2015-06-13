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

        public enum PositionNoiseMode { Disabled, ZOnly, XYZ, Random }
        public enum RotationNoiseMode { Disabled, XAxis, YAxis, ZAxis, Random }
        public enum ScaleNoiseMode { Disabled, Uniform, XYZ }

        #endregion

        #region Parameters Exposed To Editor

        [SerializeField]
        int _columns = 80;

        [SerializeField]
        int _rows = 80;

        [SerializeField]
        Vector2 _extent = new Vector2(100, 100);

        [SerializeField]
        Vector2 _offset = Vector2.zero;

        // position noise

        [SerializeField]
        PositionNoiseMode _positionNoiseMode = PositionNoiseMode.ZOnly;

        [SerializeField]
        float _positionNoiseAmplitude = 1.0f;

        [SerializeField]
        float _positionNoiseFrequency = 0.2f;

        [SerializeField]
        float _positionNoiseSpeed = 0.2f;

        // rotation noise

        [SerializeField]
        RotationNoiseMode _rotationNoiseMode = RotationNoiseMode.Disabled;

        [SerializeField]
        float _rotationNoiseAmplitude = 0.0f;

        [SerializeField]
        float _rotationNoiseFrequency = 0.2f;

        [SerializeField]
        float _rotationNoiseSpeed = 0.2f;

        // scale noise

        [SerializeField]
        ScaleNoiseMode _scaleNoiseMode = ScaleNoiseMode.Disabled;

        [SerializeField, Range(0, 1)]
        float _scaleNoiseAmplitude = 0.0f;

        [SerializeField]
        float _scaleNoiseFrequency = 0.2f;

        [SerializeField]
        float _scaleNoiseSpeed = 0.2f;

        // render settings

        [SerializeField]
        Mesh _defaultShape;

        [SerializeField]
        Mesh[] _shapes;

        [SerializeField]
        Vector3 _baseScale = Vector3.one;

        [SerializeField]
        float _minRandomScale = 0.8f;

        [SerializeField]
        float _maxRandomScale = 1.2f;

        [SerializeField]
        Material _defaultMaterial;

        [SerializeField]
        Material _material;

        [SerializeField]
        ShadowCastingMode _castShadows;

        [SerializeField]
        bool _receiveShadows = false;

        // etc.

        [SerializeField]
        int _randomSeed = 0;

        [SerializeField]
        bool _debug;

        #endregion

        #region Public Properties

        public int columns {
            get { return _columns; }
        }

        public int rows {
            get { return _rows; }
        }

        public Vector2 extent {
            get { return _extent; }
        }

        public Vector2 offset {
            get { return _offset; }
            set { _offset = value; }
        }

        #endregion

        #region Shader And Materials

        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _debugShader;

        Material _kernelMaterial;
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

        Mesh[] sourceShapes {
            get {
                if (_shapes != null)
                    foreach (var m in _shapes)
                        if (m != null) return _shapes;
                return new Mesh[]{ _defaultShape };
            }
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
            var nv = new Vector4(_offset.x / _extent.x, _offset.y / _extent.y, 0, 0);
            var ni = Vector3.zero;

            m.SetVector("_Extent", _extent);
            m.SetVector("_BaseScale", _baseScale);
            m.SetVector("_RandomScale", new Vector2(_minRandomScale, _maxRandomScale));
            m.SetVector("_Config", new Vector2(_randomSeed, Time.time));
            m.SetVector("_RandomParams", new Vector4(_offset.x / _extent.x, _offset.y / _extent.y, _columns, _rows));

            if (_positionNoiseMode == PositionNoiseMode.Disabled)
            {
                m.DisableKeyword("POSITION_XYZ");
                m.DisableKeyword("POSITION_RANDOM");
            }
            else
            {
                nv.z = _positionNoiseSpeed * Time.time;
                nv.w = _positionNoiseFrequency;
                m.SetVector("_PositionNoise", nv);

                ni.x = _positionNoiseAmplitude;

                if (_positionNoiseMode == PositionNoiseMode.ZOnly)
                {
                    m.DisableKeyword("POSITION_XYZ");
                    m.DisableKeyword("POSITION_RANDOM");
                }
                else if (_positionNoiseMode == PositionNoiseMode.XYZ) 
                {
                    m.EnableKeyword("POSITION_XYZ");
                    m.DisableKeyword("POSITION_RANDOM");
                }
                else // Random
                {
                    m.DisableKeyword("POSITION_XYZ");
                    m.EnableKeyword("POSITION_RANDOM");
                }
            }

            if (_rotationNoiseMode == RotationNoiseMode.Disabled)
            {
                m.DisableKeyword("ROTATION_RANDOM");
                m.SetVector("_RotationAxis", Vector3.right); // not in use
            }
            else
            {
                nv.z = _rotationNoiseSpeed * Time.time;
                nv.w = _rotationNoiseFrequency;
                m.SetVector("_RotationNoise", nv);

                ni.y = Mathf.Deg2Rad * _rotationNoiseAmplitude;

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
                    else // ZAxis
                        m.SetVector("_RotationAxis", Vector3.forward);
                }
            }

            if (_scaleNoiseMode == ScaleNoiseMode.Disabled)
            {
                m.DisableKeyword("SCALE_XYZ");
            }
            else
            {
                nv.z = _scaleNoiseSpeed * Time.time;
                nv.w = _scaleNoiseFrequency;
                m.SetVector("_ScaleNoise", nv);

                ni.z = _scaleNoiseAmplitude;

                if (_scaleNoiseMode == ScaleNoiseMode.Uniform)
                {
                    m.DisableKeyword("SCALE_XYZ");
                }
                else // XYZ
                {
                    m.EnableKeyword("SCALE_XYZ");
                }
            }

            m.SetVector("_NoiseInfluence", ni);
        }

        void ResetResources()
        {
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(sourceShapes, _columns);
            else
                _bulkMesh.Rebuild(sourceShapes, _columns);

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
            block.AddVector("_RandomParams", _kernelMaterial.GetVector("_RandomParams"));

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
