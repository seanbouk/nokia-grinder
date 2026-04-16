using UnityEngine;

[ExecuteInEditMode]
public class Nokia1BitPostProcess : MonoBehaviour
{
    public Material EffectMaterial;
    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (EffectMaterial != null)
        {
            Graphics.Blit(src, dest, EffectMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
