using UnityEngine;

[ExecuteInEditMode]
public class Light2DRenderer : MonoBehaviour
{
#if UNITY_EDITOR
    public bool raycast = true;
#endif

    public float sdfBorder = 0;

    public int samples = 128;
    public float maxBrightness = 1;

    public Color ambientColor;

    private Shader uvShader;
    private Shader jfShader;
    private Shader sdfShader;

    private Material uvMaterial;
    private Material jfMaterial;
    private Material rayMaterial;

    private RenderTexture colorTexture;
    private RenderTexture jfTextureA;
    private RenderTexture jfTextureB;

    private void InitTextures()
    {
        colorTexture = new RenderTexture(Screen.width, Screen.height, 0);
        jfTextureA = new RenderTexture(Screen.width, Screen.height, 0);
        jfTextureB = new RenderTexture(Screen.width, Screen.height, 0);
        colorTexture.filterMode = FilterMode.Point;
        jfTextureA.filterMode = FilterMode.Point;
        jfTextureB.filterMode = FilterMode.Point;
    }

    private void Start()
    {
        uvShader = Shader.Find("Hidden/UV Encoding");
        jfShader = Shader.Find("Hidden/Jump Flood");
        sdfShader = Shader.Find("Hidden/Raytracer");

        uvMaterial = new Material(uvShader);
        jfMaterial = new Material(jfShader);
        rayMaterial = new Material(sdfShader);

        InitTextures();
    }

    public void Update()
    {
        if (colorTexture == null || colorTexture.width != Screen.width || colorTexture.height != Screen.height)
            InitTextures();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int iterations = Mathf.CeilToInt(Mathf.Log(Screen.width * Screen.height));

        Vector2 stepSize = new Vector2(Screen.width, Screen.height);

        Graphics.Blit(source, colorTexture);
        jfMaterial.SetVector("stepSize", stepSize);
        Graphics.Blit(colorTexture, jfTextureA, uvMaterial);

        for (int i = 0; i < iterations; i++)
        {
            stepSize /= 2;

            jfMaterial.SetVector("stepSize", stepSize);

            if (i % 2 == 0)
                Graphics.Blit(jfTextureA, jfTextureB, jfMaterial);
            else
                Graphics.Blit(jfTextureB, jfTextureA, jfMaterial);
        }

        rayMaterial.SetTexture("_ColorTex", colorTexture);
        rayMaterial.SetFloat("samples", samples);
        rayMaterial.SetFloat("sdfBorder", sdfBorder);
        rayMaterial.SetFloat("maxBrightness", maxBrightness);
        rayMaterial.SetColor("ambient", ambientColor);

#if UNITY_EDITOR
        if (!raycast)
        {
            if (iterations % 2 != 0)
                Graphics.Blit(jfTextureA, destination);
            else
                Graphics.Blit(jfTextureB, destination);
            return;
        }
#endif
        if (iterations % 2 != 0)
            Graphics.Blit(jfTextureA, destination, rayMaterial);
        else
            Graphics.Blit(jfTextureB, destination, rayMaterial);
    }
}
