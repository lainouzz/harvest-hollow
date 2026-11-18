#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// Tools > Harvest Hollow > Build Greenhouse Restore FX
/// Builds FX_GreenhouseRestore.prefab (+ 2 materials) next to SmokePuffs.fbx.
/// Built with AI, too speed shit up :)
public static class GreenhouseRestoreFxBuilder
{
    [MenuItem("Tools/Harvest Hollow/Build Greenhouse Restore FX")]
    static void Build()
    {
        string guid = AssetDatabase.FindAssets("SmokePuffs t:Model").FirstOrDefault();
        if (guid == null) { Debug.LogError("SmokePuffs.fbx not found in the project."); return; }
        string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
        string dir = System.IO.Path.GetDirectoryName(fbxPath).Replace('\\', '/');

        // mesh particles need readable meshes
        var imp = (ModelImporter)AssetImporter.GetAtPath(fbxPath);
        if (!imp.isReadable || imp.materialImportMode != ModelImporterMaterialImportMode.None)
        {
            imp.isReadable = true;
            imp.materialImportMode = ModelImporterMaterialImportMode.None;
            imp.SaveAndReimport();
        }
        var meshes = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<Mesh>().ToDictionary(m => m.name);
        Mesh M(string n) => meshes.TryGetValue(n, out var m) ? m : null;

        var smokeMat = SaveMat(dir + "/FX_SmokePuff.mat", Color.white,
            "Universal Render Pipeline/Particles/Simple Lit", "Universal Render Pipeline/Particles/Lit",
            "Particles/Standard Surface", "Standard");
        var sparkMat = SaveMat(dir + "/FX_Sparkle.mat", Color.white,
            "Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit", "Unlit/Color");

        var root = new GameObject("FX_GreenhouseRestore");
        var rps = root.AddComponent<ParticleSystem>();
        var rm = rps.main; rm.playOnAwake = false; rm.loop = false; rm.duration = 3f;
        var re = rps.emission; re.enabled = false;
        root.GetComponent<ParticleSystemRenderer>().enabled = false;

        var puffs = new[] { M("Puff_01"), M("Puff_02"), M("Puff_03"), M("Puff_Cluster") }.Where(m => m).ToArray();
        var smalls = new[] { M("Puff_04"), M("Puff_01") }.Where(m => m).ToArray();
        Color cream = new Color(1f, 0.97f, 0.91f), warmGrey = new Color(0.86f, 0.84f, 0.80f);
        var puffSize = Curve((0, 0.2f), (0.12f, 1f), (0.65f, 0.9f), (1f, 0f));

        // 1) Big cover burst: engulfs the whole greenhouse, peaks ~0.3-0.6 s (swap happens under it)
        {
            var ps = Child(root, "Smoke_Burst", puffs, smokeMat);
            var m = ps.main;
            m.duration = 1.2f; m.startLifetime = new ParticleSystem.MinMaxCurve(1.3f, 2.0f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
            m.startSize = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);
            m.startColor = new ParticleSystem.MinMaxGradient(cream, warmGrey);
            m.gravityModifier = -0.08f; m.maxParticles = 300;
            Burst(ps, (0f, 55), (0.2f, 25));
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box;
            sh.position = new Vector3(0, 1.6f, 0); sh.scale = new Vector3(6.6f, 3.2f, 8.6f);
            sh.randomDirectionAmount = 0.5f;
            Rise(ps, 0.4f, 0.9f); Damp(ps, 0.4f, 0.12f); Size(ps, puffSize); Spin(ps, 1f);
        }
        // 2) Ground ring: small puffs shooting outward along the ground
        {
            var ps = Child(root, "Smoke_Ground", smalls, smokeMat);
            var m = ps.main;
            m.duration = 0.5f; m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            m.startColor = new ParticleSystem.MinMaxGradient(cream, warmGrey);
            m.gravityModifier = -0.05f;
            Burst(ps, (0f, 40));
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle;
            sh.radius = 3.4f; sh.radiusThickness = 0f;
            sh.rotation = new Vector3(90f, 0, 0); sh.scale = new Vector3(1f, 1.35f, 1f);
            sh.position = new Vector3(0, 0.15f, 0);
            Damp(ps, 0.3f, 0.15f); Size(ps, puffSize); Spin(ps, 2f);
        }
        // 3) Lingering wisps around the walls after the reveal
        {
            var ps = Child(root, "Smoke_Linger", puffs, smokeMat);
            var m = ps.main;
            m.duration = 1.5f; m.startDelay = 0.5f;
            m.startLifetime = new ParticleSystem.MinMaxCurve(2f, 3f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
            m.startColor = new ParticleSystem.MinMaxGradient(cream, warmGrey);
            var e = ps.emission; e.rateOverTime = 18f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.BoxShell;
            sh.position = new Vector3(0, 0.3f, 0); sh.scale = new Vector3(7f, 0.3f, 9f);
            Rise(ps, 0.3f, 0.6f); Size(ps, puffSize); Spin(ps, 0.6f);
        }
        // 4) "Restored!" sparkles as the smoke thins
        {
            var ps = Child(root, "Sparkles", new[] { M("Sparkle_Star") }.Where(x => x).ToArray(), sparkMat);
            var m = ps.main;
            m.duration = 0.6f; m.startDelay = 0.45f;
            m.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.2f);
            m.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            m.startRotation3D = false;
            m.startRotation = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            m.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.82f, 0.35f), new Color(1f, 0.95f, 0.7f));
            m.gravityModifier = -0.15f;
            Burst(ps, (0f, 20), (0.3f, 10));
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box;
            sh.position = new Vector3(0, 3f, 0); sh.scale = new Vector3(6f, 1.5f, 8f);
            sh.randomDirectionAmount = 1f;
            Size(ps, Curve((0, 0), (0.2f, 1.2f), (0.5f, 1f), (1f, 0f)));
            var rot = ps.rotationOverLifetime; rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.alignment = ParticleSystemRenderSpace.View;   // if stars look edge-on, try Local
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        string prefabPath = dir + "/FX_GreenhouseRestore.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        Debug.Log("Built " + prefabPath);
    }

    // ------------------------------------------------------------ helpers

    static ParticleSystem Child(GameObject root, string name, Mesh[] meshes, Material mat)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var m = ps.main;
        m.playOnAwake = false; m.loop = false;
        m.simulationSpace = ParticleSystemSimulationSpace.World;
        m.scalingMode = ParticleSystemScalingMode.Hierarchy;
        m.startRotation3D = true;
        m.startRotationX = m.startRotationY = m.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        var e = ps.emission; e.rateOverTime = 0f;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Mesh;
        r.SetMeshes(meshes);
        r.meshDistribution = ParticleSystemMeshDistribution.UniformRandom;
        r.sharedMaterial = mat;
        r.alignment = ParticleSystemRenderSpace.Local;
        r.shadowCastingMode = ShadowCastingMode.On;     // chunky puffs look cozier with soft shadows
        r.receiveShadows = true;
        return ps;
    }

    static void Burst(ParticleSystem ps, params (float t, int n)[] bursts)
    {
        var e = ps.emission;
        e.SetBursts(bursts.Select(b => new ParticleSystem.Burst(b.t, (short)b.n)).ToArray());
    }

    static void Rise(ParticleSystem ps, float min, float max)
    {
        var v = ps.velocityOverLifetime; v.enabled = true;
        v.space = ParticleSystemSimulationSpace.World;
        v.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        v.y = new ParticleSystem.MinMaxCurve(min, max);
        v.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    }

    static void Damp(ParticleSystem ps, float limit, float dampen)
    {
        var l = ps.limitVelocityOverLifetime; l.enabled = true;
        l.limit = limit; l.dampen = dampen;
    }

    static void Size(ParticleSystem ps, AnimationCurve c)
    {
        var s = ps.sizeOverLifetime; s.enabled = true;
        s.size = new ParticleSystem.MinMaxCurve(1f, c);
    }

    static void Spin(ParticleSystem ps, float radPerSec)
    {
        var r = ps.rotationOverLifetime; r.enabled = true; r.separateAxes = true;
        r.x = r.y = r.z = new ParticleSystem.MinMaxCurve(-radPerSec, radPerSec);
    }

    static AnimationCurve Curve(params (float t, float v)[] keys)
    {
        var c = new AnimationCurve(keys.Select(k => new Keyframe(k.t, k.v)).ToArray());
        for (int i = 0; i < c.length; i++) c.SmoothTangents(i, 0f);
        return c;
    }

    static Material SaveMat(string path, Color color, params string[] shaders)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing) return existing;
        var shader = shaders.Select(Shader.Find).FirstOrDefault(s => s);
        var mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
#endif
