/*****************************************************
/* Created by Wizcas Chen (http://wizcas.me)
/* Please contact me if you have any question
/* E-mail: chen@wizcas.me
/* 2017 © All copyrights reserved by Wizcas Zhuo Chen
*****************************************************/

using Cheers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[PrettyLog.Provider("SpriteExploder", ModuleColor = "orange")]
                           public class SpriteExploder : MonoBehaviour
{
    public enum ExplodeType
    {
        Foe,
        Potion
    }

    public ExplodeType type = ExplodeType.Foe;
    public ParticleSystem fxPrefab;
    public bool tintByPixels;
    public bool overrideEmitPosition;
    [Help("Works with position overriding")]
    public Vector2 scatteringDensity = Vector2.one;
    public bool overrideEmitVelocity;
    [Help("Works with velocity overriding")]
    public float blowAngleVariant = 30f;

    [Range(1, 256)]
    public int chunkSize = 4;
    public float chunkMass = 1;
    public Vector2 offset = Vector2.zero;

    [SerializeField] bool _showDebugInfo;
    [SerializeField] bool _fakeExplodeForTest;

    static Dictionary<int, Color32[]> SpritePixelColors = new Dictionary<int, Color32[]>();

    string GetPoolName(ExplodeType type)
    {
        switch (type)
        {
            case ExplodeType.Foe:
                return PoolName.ExplodeFX;
            case ExplodeType.Potion:
                return PoolName.PotionBreakFX;
            default:
                return null;
        }
    }

    ParticleSystem MakeFx(Transform spawnTransform)
    {
        var pos = spawnTransform.position + offset.ToVector3();
        if (fxPrefab != null)
        {
            return Instantiate(fxPrefab, pos, Quaternion.identity);
        }
        return null;
    }

    Color32[] ExtractColorsFromTex(Sprite sprite)
    {
        var spriteRect = sprite.rect;
        var spriteId = sprite.GetInstanceID();
        var tex = sprite.texture;
        Color32[] ret;
        if (!SpritePixelColors.TryGetValue(spriteId, out ret))
        {
            if (_showDebugInfo)
            {
                PrettyLog.Log("making tex... id: {0} name: {1}", spriteId, tex.name);
            }
            var w = (int)spriteRect.width;
            var h = (int)spriteRect.height;
            var colors = new List<Color32>();
            for (int xOffset = 0; xOffset < w; xOffset++)
            {
                var x = xOffset + (int)spriteRect.x;
                for (int yOffset = 0; yOffset < h; yOffset++)
                {
                    var y = yOffset + (int)spriteRect.y;
                    var pixelColor = tintByPixels ? tex.GetPixel(x, y) : Color.white;
                    if (Mathf.Approximately(pixelColor.a, 0))
                    {
                        continue;
                    }
                    colors.Add(pixelColor);
                }
            }
            ret = colors.ToArray();
            SpritePixelColors[spriteId] = ret;
        }
        return ret;
    }

    Color32[] MakePieceColors(SpriteRenderer rdr)
    {
        var colors = ExtractColorsFromTex(rdr.sprite);
        var pieceCount = Mathf.RoundToInt(colors.Length / (chunkSize * chunkSize));
        if(_showDebugInfo)
            PrettyLog.Log("pixels: {0}, chunk size: {1}, piece count: {2}", colors.Length, chunkSize, pieceCount);
        var ret = new Color32[pieceCount];
        for (int colorIndex = 0, pieceIndex = 0; pieceIndex < pieceCount; colorIndex += chunkSize, pieceIndex++)
        {
            ret[pieceIndex] = colors[colorIndex];
        }
        return ret;
    }

    public void Explode(SpriteRenderer rdr, Vector2? explodePos)
    {
        if (!_fakeExplodeForTest)
            rdr.gameObject.SetActive(false);
        if (rdr == null)
        {
            Debug.LogError("Sprite renderer is null", this);
            return;
        }
        var fx = MakeFx(rdr.transform);
        var colors = MakePieceColors(rdr);
        var particleCount = Mathf.Min(colors.Length, fx.main.maxParticles);
        fx.Emit(particleCount);
        var particles = new ParticleSystem.Particle[particleCount];
        int num = fx.GetParticles(particles);

        // 用于计算粒子飞行向量
        System.Func<ParticleSystem.Particle, Vector2, Vector2> ComputeFlyVec = (particle, blowPos) =>
        {
            var dir = Quaternion.AngleAxis(Random.Range(-1f, 1f) * 30 + blowAngleVariant, Vector3.forward) * (particle.position.ToVector2() - blowPos).normalized;
            var force = fx.main.startSpeedMultiplier * (1f / (particle.startSize * chunkMass)) * .7f;
            return dir * force;
        };

        for (int i = 0; i < num; i++)
        {
            MakeParticle(ref particles[i], explodePos, colors[i], rdr, fx, ComputeFlyVec);
        }
        fx.SetParticles(particles, num);
        if (_showDebugInfo)
        {
            PrettyLog.Log("{1} explodes into {0} particles", fx.particleCount, rdr.name);
        }
    }

    void MakeParticle(
        ref ParticleSystem.Particle particle,
        Vector2? explodeWorldPos, Color32 sampledColor, SpriteRenderer rdr,
        ParticleSystem fx,
        System.Func<ParticleSystem.Particle, Vector2, Vector2> funcComputeVec
        )
    {
        if (tintByPixels)
        {
            particle.startColor = sampledColor;
        }
        if (overrideEmitPosition)
        {
            var center = TransformWorldPosToSimSpace(rdr.bounds.center, fx);
            var scatteredPos = center + new Vector2(
                (Random.value - .5f) * rdr.bounds.size.x * scatteringDensity.x,
                (Random.value - .5f) * rdr.bounds.size.y * scatteringDensity.y
                );
            particle.position = scatteredPos;
        }
        if (explodeWorldPos.HasValue && overrideEmitVelocity)
        {
            var blowPos = TransformWorldPosToSimSpace(explodeWorldPos.Value, fx);
            particle.velocity = funcComputeVec(particle, blowPos);
        }
        if (_showDebugInfo)
        {
            Debug.DrawRay(TransformSimPosToWorldSpace(particle.position, fx), particle.velocity.normalized, Color.red, 1f);
        }
    }

    Transform GetSimTransform(ParticleSystem fx)
    {
        switch (fx.main.simulationSpace)
        {
            case ParticleSystemSimulationSpace.Local:
                return fx.transform;
            case ParticleSystemSimulationSpace.Custom:
                return fx.main.customSimulationSpace;
            default:
                return null;
        }
    }

    Vector2 TransformWorldPosToSimSpace(Vector3 worldPos, ParticleSystem fx)
    {
        Transform simTransform = GetSimTransform(fx);
        if (simTransform != null)
        {
            worldPos = simTransform.InverseTransformPoint(worldPos);
        }
        return worldPos.ToVector2();
    }

    Vector2 TransformSimPosToWorldSpace(Vector3 simPos, ParticleSystem fx)
    {
        Transform simTransform = GetSimTransform(fx);
        if (simTransform != null)
        {
            simPos = simTransform.TransformPoint(simPos);
        }
        return simPos.ToVector2();
    }

    public static void Explode(MonoBehaviour sender)
    {
        if (sender == null)
            return;
        var exploders = sender.GetComponentsInAllChildren<SpriteExploder>(true);
        if (exploders.Length == 0)
        {
            exploders = new[] { sender.gameObject.AddComponent<SpriteExploder>() };
        }
        exploders.ForEach(ex => ex.Explode(sender.GetComponent<SpriteRenderer>(), null));
    }

    public void Play(MonoBehaviour sender, Vector2 hitPos)
    {
        Explode(sender.GetComponent<SpriteRenderer>(), hitPos);
    }
}
