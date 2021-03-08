using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainScan : MonoBehaviour
{
    public Transform scannerOrigin;
    public Material effectMaterial;
    public float scanDistance;
    public float scanSpeed;

    private Camera _camera;

    private bool _scanning = false;
    
    private void Update()
    {
        Debug.Log(scannerOrigin.position);
        
        if (_scanning)
        {
            scanDistance += Time.deltaTime * scanSpeed;
            //Debug.Log(scanDistance);
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _scanning = true;
            scanDistance = 0;
        }
        
    }

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        
        // Generate a depth texture.
        // Will generate a screen-space depth texture as seen from this camera.
        // Will be set as _CameraDepthTexture global shader property.
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    // Any Image Effect with this attribute will be rendered after opaque geometry but before transparent geometry.

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        effectMaterial.SetVector("_WorldSpaceScannerPos", scannerOrigin.position);
        effectMaterial.SetFloat("_ScanDistance", scanDistance);
        
        RaycastCornerBlit(src, dest, effectMaterial);
    }

    // a Blit copies source texture into destination render texture with a shader.
    void RaycastCornerBlit(RenderTexture source, RenderTexture desti, Material mat)
    {
        // Hier ga ik een custom Frustum maken. Een Frustum is een uitsnede van een solide object. Meestal een piramide.
         
        float camFar = _camera.farClipPlane;
        float camFov = _camera.fieldOfView;
        float camAspect = _camera.aspect;

        float fovWHalf = camFov * 0.5f;

        // Hier bereken ik het rechter/top punt van het scherm (op deze manier kan het voor elk soort scherm gebruikt worden)
        Vector3 toRight = _camera.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = _camera.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        
        // Berekenen van alle 4 de hoeken op de farplane.
        // Door toRight en toTop te gebruiken kan ik makkelijk de vier hoeken vinden
        Vector3 topLeft = (_camera.transform.forward - toRight + toTop);
        
                    
        //pas na de eerste hoek kan ik de camScale bepalen.
        float camScale = topLeft.magnitude * camFar;
        
        topLeft.Normalize();
        topLeft *= camScale;
        
        Vector3 topRight = (_camera.transform.forward + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (_camera.transform.forward + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (_camera.transform.forward - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;
        
        // <CUSTOM BLIT> De Frustum hoeken gaan nu encoded worden als "Additional Texture Coordinates"
        // Ik geef het als TEX CORDS mee zodat de frag shader het dan via geinterpolate krijgt van de vertex shader
        RenderTexture.active = desti;
        
        mat.SetTexture("_MainTex", source);
        
        GL.PopMatrix();
        GL.LoadOrtho();

        mat.SetPass(0);

        GL.Begin(GL.QUADS);
        
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.MultiTexCoord(1, bottomLeft);
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.MultiTexCoord(1, bottomRight);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.MultiTexCoord(1, topRight);
        GL.Vertex3(1.0f, 1.0f, 0.0f);

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.MultiTexCoord(1, topLeft);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        
        GL.End();
        GL.PopMatrix();
    }
}
