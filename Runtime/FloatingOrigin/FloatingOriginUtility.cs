// FloatingOrigin.cs
// Written by Peter Stirling
// 11 November 2010
// Uploaded to Unify Community Wiki on 11 November 2010
// Updated to Unity 5.x particle system by Tony Lovell 14 January, 2016
// fix to ensure ALL particles get moved by Tony Lovell 8 September, 2016
// URL: http://wiki.unity3d.com/index.php/Floating_Origin
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.SceneManagement;

/// <summary>
/// TODO: Convert the heavy lifting of this to a Burst Job.
/// </summary>
public class FloatingOriginUtility : MonoBehaviour
{
    private static FloatingOriginUtility _instance = null;
    public static FloatingOriginUtility Instance => _instance;

    [Header("Properties")]
    [Tooltip("Use a power of 2 to avoid pops in ocean surface geometry."), SerializeField]
    private float _threshold = 1024;

    [Tooltip("Set to zero to disable."), SerializeField]
    private float _physicsThreshold = 1000.0f;

    [SerializeField]
    private float _defaultSleepThreshold = 0.14f;

#if EXPANSE
    [Header("References")]
    [Tooltip("Instance of the Expanse planet volume for setting origin offset."), SerializeField]
    private Expanse.Planet _expansePlanet;
#endif

    [Tooltip("The Transform to follow for ensuring updates. Defaults to main camera.")]
    public Transform Target;

    [Tooltip("Optionally provide a list of transforms to avoid doing a FindObjectsOfType() call."), SerializeField]
    private Transform[] _overrideTransformList = null;

    [Tooltip("Optionally provide a list of particle systems to avoid doing a FindObjectsOfType() call."), SerializeField]
    private ParticleSystem[] _overrideParticleSystemList = null;

    [Tooltip("Optionally provide a list of rigidbodies to avoid doing a FindObjectsOfType() call."), SerializeField]
    private Rigidbody[] _overrideRigidbodyList = null;

#if CREST
    [Tooltip("Optionally provide a list of Gerstner components to avoid doing a FindObjectsOfType() call."), SerializeField]
    private Crest.ShapeGerstnerBatched[] _overrideGerstnerList = null;
#endif

    private Dictionary<int, Transform> _transformMap = null;
    private Dictionary<int, ParticleSystem> _particleSystemMap = null;
    private Dictionary<int, Rigidbody> _rigidbodyMap = null;

    private TransformAccessArray _transformAccess;
    private MoveTransformsOriginJob _moveTransformsJob;
    private JobHandle _moveTransformsHandle;

    /// <summary>
    /// Fired when origin resets and includes the current position.
    /// </summary>
    /// <returns></returns>
    [Tooltip("Fired when origin resets and includes the current position.")]
    public ResetOriginEvent OnResetOrigin = new ResetOriginEvent();

    public static Vector3 CurrentPosition
    {
        get;
        private set;
    }

    public static Vector3 CurrentOffset
    {
        get;
        private set;
    }

    public static bool HasResetThisFrame
    {
        get;
        private set;
    }

    private ParticleSystem.Particle[] _particleBuffer = null;

    [System.Serializable]
    public class ResetOriginEvent : UnityEvent<Vector3> {}

    public void Register(UnityAction<Vector3> action)
    {
        OnResetOrigin.AddListener(action);
    }

    public void Register(Transform transform)
    {
        if (_transformMap == null)
            _transformMap = new Dictionary<int, Transform>();

        int instId = transform.GetInstanceID();
        if (!_transformMap.ContainsKey(instId))
            _transformMap.Add(instId, transform);
    }

    public void Register(ParticleSystem system)
    {
        if (_particleSystemMap == null)
            _particleSystemMap = new Dictionary<int, ParticleSystem>();

        int instId = system.GetInstanceID();
        if (!_particleSystemMap.ContainsKey(instId))
            _particleSystemMap.Add(instId, system);
    }

    public void Register(Rigidbody rb)
    {
        if (_rigidbodyMap == null)
            _rigidbodyMap = new Dictionary<int, Rigidbody>();

        int instId = rb.GetInstanceID();
        if (!_rigidbodyMap.ContainsKey(instId))
            _rigidbodyMap.Add(instId, rb);
    }

    public void Unregister(UnityAction<Vector3> action)
    {
        OnResetOrigin.RemoveListener(action);
    }

    public void Unregister(Transform transform)
    {
        if (_transformMap == null)
            _transformMap = new Dictionary<int, Transform>();

        int instId = transform.GetInstanceID();
        if (_transformMap.ContainsKey(instId))
            _transformMap.Remove(instId);
    }

    public void Unregister(ParticleSystem system)
    {
        if (_particleSystemMap == null)
            _particleSystemMap = new Dictionary<int, ParticleSystem>();

        int instId = system.GetInstanceID();
        if (_particleSystemMap.ContainsKey(instId))
            _particleSystemMap.Remove(instId);
    }

    public void Unregister(Rigidbody rb)
    {
        if (_rigidbodyMap == null)
            _rigidbodyMap = new Dictionary<int, Rigidbody>();

        int instId = rb.GetInstanceID();
        if (_rigidbodyMap.ContainsKey(instId))
            _rigidbodyMap.Remove(instId);
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (Target == default)
            Target = Camera.main.transform;

        for (int i = 0; i < _overrideTransformList.Length; i++)
        {
            Register(_overrideTransformList[i]);
        }

        for (int i = 0; i < _overrideParticleSystemList.Length; i++)
        {
            Register(_overrideParticleSystemList[i]);
        }

        for (int i = 0; i < _overrideRigidbodyList.Length; i++)
        {
            Register(_overrideRigidbodyList[i]);
        }
    }

    private void Update()
    {
        var newOrigin = Vector3.zero;
        if (Mathf.Abs(Target.position.x) > _threshold)
            newOrigin.x += Target.position.x;
        if (Mathf.Abs(Target.position.y) > _threshold)
            newOrigin.y += Target.position.y;
        if (Mathf.Abs(Target.position.z) > _threshold)
            newOrigin.z += Target.position.z;

        if (newOrigin != Vector3.zero)
        {
            HasResetThisFrame = true;
            CurrentPosition = newOrigin;
            CurrentOffset += newOrigin;

            StartMoveOriginTransforms(CurrentPosition);
            MoveOriginParticles(CurrentPosition);
            MoveOriginOcean(CurrentPosition);
            MoveOriginMicrosplatShader(CurrentPosition);
            MoveOriginDisablePhysics();
            MoveOriginClouds(CurrentPosition);
            CompleteMoveOriginTransforms();

            // Update event listeners.
            OnResetOrigin?.Invoke(CurrentPosition);
        }
        else
        {
            HasResetThisFrame = false;
        }
    }

    private void StartMoveOriginTransforms(Vector3 newOrigin)
    {
        _transformAccess = new TransformAccessArray(_transformMap.Values.ToArray(), 1);
        _moveTransformsJob = new MoveTransformsOriginJob() { NewOrigin = newOrigin };
        _moveTransformsHandle = _moveTransformsJob.Schedule(_transformAccess);
    }

    private void CompleteMoveOriginTransforms()
    {
        _moveTransformsHandle.Complete();
        _transformAccess.Dispose();
    }

    /// <summary>
    /// Move transforms to recenter around new origin.
    /// </summary>
    [Obsolete("Use Jobs system with StartMoveOriginTransforms() and CompleteMoveOriginTransforms().")]
    private void MoveOriginTransformsSync(Vector3 newOrigin)
    {
        bool hasOverrideList = _transformMap != null && _transformMap.Count > 0;
        var transforms = hasOverrideList ? _transformMap.Values.ToArray() : FindObjectsOfType<Transform>();

        foreach (var t in transforms)
        {
            if (t.parent == null)
            {
                t.position -= newOrigin;
            }
        }
    }

    /// <summary>
    /// Move all particles that are simulated in world space
    /// </summary>
    private void MoveOriginParticles(Vector3 newOrigin)
    {
        bool hasOverrideList = _particleSystemMap != null && _particleSystemMap.Count > 0;
        var particleSystems = hasOverrideList ? _particleSystemMap.Values.ToArray() : FindObjectsOfType<ParticleSystem>();
        foreach (var sys in particleSystems)
        {
            if (sys.main.simulationSpace != ParticleSystemSimulationSpace.World)continue;

            var particlesNeeded = sys.main.maxParticles;
            if (particlesNeeded <= 0)continue;

            var wasPaused = sys.isPaused;
            var wasPlaying = sys.isPlaying;

            if (!wasPaused)
            {
                sys.Pause();
            }

            // Ensure a sufficiently large array in which to store the particles
            if (_particleBuffer == null || _particleBuffer.Length < particlesNeeded)
            {
                _particleBuffer = new ParticleSystem.Particle[particlesNeeded];
            }

            // Update the particles
            var num = sys.GetParticles(_particleBuffer);
            for (var i = 0; i < num; i++)
            {
                _particleBuffer[i].position -= newOrigin;
            }
            sys.SetParticles(_particleBuffer, num);

            if (wasPlaying)
            {
                sys.Play();
            }
        }
    }

    /// <summary>
    /// Notify ocean of origin shift
    /// </summary>
    private void MoveOriginOcean(Vector3 newOrigin)
    {
#if CREST
        if (OceanRenderer.Instance)
        {
            OceanRenderer.Instance._lodTransform.SetOrigin(newOrigin);

            var fos = OceanRenderer.Instance.GetComponentsInChildren<IFloatingOrigin>();
            foreach (var fo in fos)
            {
                fo.SetOrigin(newOrigin);
            }

            // Gerstner components
            var gerstners = _overrideGerstnerList != null && _overrideGerstnerList.Length > 0 ? _overrideGerstnerList : FindObjectsOfType<ShapeGerstnerBatched>();
            foreach (var gerstner in gerstners)
            {
                gerstner.SetOrigin(newOrigin);
            }
        }
#endif
    }

    /// <summary>
    /// Shifts the origin used by the MicroSplat shaders in world space UVs.
    /// </summary>
    private void MoveOriginMicrosplatShader(Vector3 newOrigin)
    {
#if MICROSPLAT
        Shader.SetGlobalMatrix("_GlobalOriginMTX", Matrix4x4.TRS(newOrigin, Quaternion.identity, Vector3.one));
#endif
    }

    /// <summary>
    /// Disable physics outside radius
    /// </summary>
    private void MoveOriginDisablePhysics()
    {
        if (_physicsThreshold > 0f)
        {
            bool hasOverrideList = _rigidbodyMap != null && _rigidbodyMap.Count > 0;

            var physicsThreshold2 = _physicsThreshold * _physicsThreshold;
            var rbs = hasOverrideList ? _rigidbodyMap.Values.ToArray() : FindObjectsOfType<Rigidbody>();
            foreach (var rb in rbs)
            {
                if (rb.gameObject.transform.position.sqrMagnitude > physicsThreshold2)
                {
                    rb.sleepThreshold = float.MaxValue;
                }
                else
                {
                    rb.sleepThreshold = _defaultSleepThreshold;
                }
            }
        }
    }

    private void MoveOriginClouds(Vector3 newOrigin)
    {
#if EXPANSE
        _expansePlanet.m_originOffset -= newOrigin;
#endif
    }

    private void OnDestroy()
    {
        try
        {
            _transformAccess.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Job complete.
        }
    }

    [BurstCompile]
    private struct MoveTransformsOriginJob : IJobParallelForTransform
    {
        public Vector3 NewOrigin;

        public void Execute(int index, TransformAccess transform)
        {
            transform.position -= NewOrigin;
        }
    }
}

public static class VirtualOriginExtensions
{
    public static Vector3 ToVirtualPosition(this Transform transform)
    {
        return ToVirtualPosition(transform.position);
    }

    public static Vector3 ToVirtualPosition(this Vector3 position)
    {
        return position + FloatingOriginUtility.CurrentOffset;
    }

    public static Vector3 ToTruePosition(this Vector3 position)
    {
        return position - FloatingOriginUtility.CurrentOffset;
    }
}
