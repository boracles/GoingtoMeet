# Documentation

1. Installation: Install LitSmoke to your Unity project from the package manager.

2. Switching Render Pipeline (if you're using URP): If you're using URP, double-click ZeroingVFX/LitSmoke_URP to switch to the URP render pipeline.

3. You can find the effect prefab in the ZeroingVFX/LitSmoke/Prefab folder.

4. If you want to create your own smoke effect, you can directly modify the particle system based on the original prefab.

5. If you want to create a smoke effect from scratch , follow the steps below : Create a Particle System,  Set the Renderer Mode to Mesh, and use vfx_NormalQuad01 or vfx_NormalQuad02 (These two models are specifically designed for Lit Smoke ). Create a new material and use the vfxVertexLit shader. Assign this material to your particle system , and now you have a particle system that can interact with lighting. 

Next, you need to understand the purpose of each property in the shader.

---
//vfxVertexLit Shader Explanation:

_Texture : main texture of the material

_Color: main color of the material

_Transmittance: adjusts the material's transmittance

_MinDark: increases the brightness of the material's dark areas

_MaxLit (URP only) :  prevent overexposure when light sources are very close

_DepthFade: adjusts the transparency fade between objects

_UseVertexLights(URP only): when enabled, uses vertex rendering for lighting to save performance , but when turned off, lighting is rendered on every pixel.

_LightCount : adjusts the maximum number of light sources affecting the material

_AlphaGamma: adjusts the transparency falloff, with 0 resulting in no
change and 1 resulting in transparency being squared

_BlackToAlpha: uses texture's grayscale as transparency values

_CullMode: adjusts whether to display the model's front face, back face, or both

---

