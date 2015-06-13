//
// Fade-to-skybox fog effect
//
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Fade To Skybox")]
public class FadeToSkybox : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField] bool _useRadialDistance;
    [SerializeField] float _startDistance;
    [SerializeField] Shader _fogShader;

    #endregion

    #region Public Properties And Functions

    public bool useRadialDistance {
        get { return _useRadialDistance; }
        set { _useRadialDistance = value; }
    }

    public float startDistance {
        get { return _startDistance; }
        set { _startDistance = value; }
    }

    public static bool CheckSkybox()
    {
        var skybox = RenderSettings.skybox;
        return skybox != null &&
               skybox.HasProperty("_Tex") &&
               skybox.HasProperty("_Tint") &&
               skybox.HasProperty("_Exposure") &&
               skybox.HasProperty("_Rotation");
    }

    #endregion

    #region Private Objects

    Material  _fogMaterial;

    #endregion

    #region Private Functions

    void Setup()
    {
        if (_fogMaterial == null) {
            _fogMaterial = new Material(_fogShader);
            _fogMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void SanitizeParameters()
    {
        _startDistance = Mathf.Max(_startDistance, 0.0f);
    }

    #endregion

    #region Monobehaviour Functions

    void Start()
    {
        Setup();
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CheckSkybox())
        {
            // The current skybox isn't cubed one.
            Graphics.Blit(source, destination);
            return;
        }

        SanitizeParameters();

        Setup();

        // Set up fog parameters.
        _fogMaterial.SetFloat("_DistanceOffset", _startDistance);

        var mode = RenderSettings.fogMode;
        if (mode == FogMode.Linear)
        {
            var start = RenderSettings.fogStartDistance;
            var end = RenderSettings.fogEndDistance;
            var invDiff = 1.0f / Mathf.Max(end - start, 1.0e-6f);
            _fogMaterial.SetFloat("_LinearGrad", -invDiff);
            _fogMaterial.SetFloat("_LinearOffs", end * invDiff);
            _fogMaterial.DisableKeyword("FOG_EXP");
            _fogMaterial.DisableKeyword("FOG_EXP2");
        }
        else if (mode == FogMode.Exponential)
        {
            const float coeff = 1.4426950408f; // 1/ln(2)
            var density = RenderSettings.fogDensity;
            _fogMaterial.SetFloat("_Density", coeff * density);
            _fogMaterial.EnableKeyword("FOG_EXP");
            _fogMaterial.DisableKeyword("FOG_EXP2");
        }
        else // FogMode.ExponentialSquared
        {
            const float coeff = 1.2011224087f; // 1/sqrt(ln(2))
            var density = RenderSettings.fogDensity;
            _fogMaterial.SetFloat("_Density", coeff * density);
            _fogMaterial.DisableKeyword("FOG_EXP");
            _fogMaterial.EnableKeyword("FOG_EXP2");
        }

        if (_useRadialDistance)
            _fogMaterial.EnableKeyword("RADIAL_DIST");
        else
            _fogMaterial.DisableKeyword("RADIAL_DIST");

        // Transfer the skybox parameters.
        var skybox = RenderSettings.skybox;
        _fogMaterial.SetTexture("_SkyCubemap", skybox.GetTexture("_Tex"));
        _fogMaterial.SetColor("_SkyTint", skybox.GetColor("_Tint"));
        _fogMaterial.SetFloat("_SkyExposure", skybox.GetFloat("_Exposure"));
        _fogMaterial.SetFloat("_SkyRotation", skybox.GetFloat("_Rotation"));

        // Calculate vectors towards frustum corners.
        var cam = GetComponent<Camera>();
        var camtr = cam.transform;
        var camNear = cam.nearClipPlane;
        var camFar = cam.farClipPlane;

        var tanHalfFov = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2);
        var toRight = camtr.right * camNear * tanHalfFov * cam.aspect;
        var toTop = camtr.up * camNear * tanHalfFov;

        var v_tl = camtr.forward * camNear - toRight + toTop;
        var v_tr = camtr.forward * camNear + toRight + toTop;
        var v_br = camtr.forward * camNear + toRight - toTop;
        var v_bl = camtr.forward * camNear - toRight - toTop;

        var v_s = v_tl.magnitude * camFar / camNear;

        // Draw screen quad.
        _fogMaterial.SetTexture("_MainTex", source);
        _fogMaterial.SetPass(0);

        RenderTexture.active = destination;

        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0, 0);
        GL.MultiTexCoord(1, v_bl.normalized * v_s);
        GL.Vertex3(0, 0, 0.1f);

        GL.MultiTexCoord2(0, 1, 0);
        GL.MultiTexCoord(1, v_br.normalized * v_s);
        GL.Vertex3(1, 0, 0.1f);

        GL.MultiTexCoord2(0, 1, 1);
        GL.MultiTexCoord(1, v_tr.normalized * v_s);
        GL.Vertex3(1, 1, 0.1f);

        GL.MultiTexCoord2(0, 0, 1);
        GL.MultiTexCoord(1, v_tl.normalized * v_s);
        GL.Vertex3(0, 1, 0.1f);

        GL.End();
        GL.PopMatrix();
    }

    #endregion
}
