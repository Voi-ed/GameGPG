using UnityEngine;
using CartoonFX;

public static class VFX_ColorTint
{
    /// <summary>
    /// Traverses all visual components in the root object (Particles, Sprites, Trails, Lights, CFXR)
    /// and recolors them to targetColor while preserving alpha transparency and lifetime animations.
    /// </summary>
    public static void Apply(GameObject root, Color tintColor)
    {
        if (root == null) return;

        // 1. Particle Systems
        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            var startColor = main.startColor;

            if (startColor.mode == ParticleSystemGradientMode.TwoColors)
            {
                Color cMin = tintColor; cMin.a = startColor.colorMin.a;
                Color cMax = tintColor; cMax.a = startColor.colorMax.a;
                main.startColor = new ParticleSystem.MinMaxGradient(cMin, cMax);
            }
            else if (startColor.mode == ParticleSystemGradientMode.Gradient && startColor.gradient != null)
            {
                main.startColor = TintGradient(startColor.gradient, tintColor);
            }
            else
            {
                Color c = tintColor; c.a = startColor.color.a;
                main.startColor = c;
            }

            var colModule = ps.colorOverLifetime;
            if (colModule.enabled && colModule.color.gradient != null)
            {
                colModule.color = TintGradient(colModule.color.gradient, tintColor);
            }

            var trails = ps.trails;
            if (trails.enabled && trails.colorOverLifetime.gradient != null)
            {
                trails.colorOverLifetime = TintGradient(trails.colorOverLifetime.gradient, tintColor);
            }
        }

        // 2. SpriteRenderers
        SpriteRenderer[] spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in spriteRenderers)
        {
            Color c = tintColor;
            c.a = sr.color.a;
            sr.color = c;
        }

        // 3. TrailRenderers
        TrailRenderer[] trailRenderers = root.GetComponentsInChildren<TrailRenderer>(true);
        foreach (var tr in trailRenderers)
        {
            Color c1 = tintColor; c1.a = tr.startColor.a;
            Color c2 = tintColor; c2.a = tr.endColor.a;
            tr.startColor = c1;
            tr.endColor = c2;
        }

        // 4. Unity Lights
        Light[] lights = root.GetComponentsInChildren<Light>(true);
        foreach (var light in lights)
        {
            light.color = tintColor;
        }

        // 5. CartoonFX Animated Lights
        CFXR_Effect cfxr = root.GetComponentInChildren<CFXR_Effect>(true);
        if (cfxr != null && cfxr.animatedLights != null)
        {
            foreach (var al in cfxr.animatedLights)
            {
                if (al != null && al.light != null)
                {
                    al.light.color = tintColor;
                    if (al.colorGradient != null)
                    {
                        al.colorGradient = TintGradient(al.colorGradient, tintColor);
                    }
                }
            }
        }
    }

    private static Gradient TintGradient(Gradient source, Color tintColor)
    {
        if (source == null) return null;

        Gradient g = new Gradient();

        GradientColorKey[] colKeys;
        if (source.colorKeys != null && source.colorKeys.Length > 0)
        {
            colKeys = new GradientColorKey[source.colorKeys.Length];
            for (int i = 0; i < source.colorKeys.Length; i++)
            {
                colKeys[i] = new GradientColorKey(tintColor, source.colorKeys[i].time);
            }
        }
        else
        {
            colKeys = new GradientColorKey[]
            {
                new GradientColorKey(tintColor, 0f),
                new GradientColorKey(tintColor, 1f)
            };
        }

        GradientAlphaKey[] alphaKeys = (source.alphaKeys != null && source.alphaKeys.Length > 0)
            ? source.alphaKeys
            : new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

        g.SetKeys(colKeys, alphaKeys);
        return g;
    }
}
