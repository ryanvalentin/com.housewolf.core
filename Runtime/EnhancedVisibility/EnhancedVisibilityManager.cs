using Housewolf.EntitySystem;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Housewolf
{
    public class EnhancedVisibilityManager : BaseEntityManager<EnhancedVisibilityEntity>, IEntityManager
    {
        private ScaleObjectsSerfossJob _scalesJob;

        private JobHandle _scalesHandle;

        private TransformAccessArray _transforms;

        private List<Vector3> _originalScales;

        private NativeArray<float3> _originalScalesNA;

        private NativeArray<bool> _enabledObjects;

        public IEntityManager Dependency => null;

        public override void HandleInit()
        {
            base.HandleInit();

            _transforms = new TransformAccessArray(1, -1);
            _originalScales = new List<Vector3>();
        }

        public override int Register(EnhancedVisibilityEntity entity)
        {
            int index = base.Register(entity);

            var entityTransform = entity.transform;

            _transforms.capacity = _transforms.length + 1;
            _transforms.Add(entityTransform);

            _originalScales.Add(entityTransform.localScale);

            return index;
        }

        public override void HandleUpdate()
        {
            base.HandleUpdate();

            float3 cameraPosition = Camera.main.transform.position;

            _originalScalesNA = new NativeArray<float3>(_originalScales.Count, Allocator.TempJob);
            _enabledObjects = new NativeArray<bool>(EntityCount, Allocator.TempJob);

            ForEachEntity((entity, index) =>
            {
                _originalScalesNA[index] = _originalScales[index];
                _enabledObjects[index] = entity.isActiveAndEnabled;
            });

            _scalesJob = new ScaleObjectsSerfossJob()
            {
                CameraPosition = cameraPosition,
                OriginalScales = _originalScalesNA,
                EnabledObjects = _enabledObjects,
            };

            _scalesHandle = _scalesJob.Schedule(_transforms);
        }

        public override void HandleLateUpdate()
        {
            base.HandleLateUpdate();

            _scalesHandle.Complete();

            _originalScalesNA.Dispose();
            _enabledObjects.Dispose();
        }

        protected override void DisposeAll()
        {
            base.DisposeAll();

            try
            {
                _transforms.Dispose();
                _originalScalesNA.Dispose();
                _enabledObjects.Dispose();
            }
            catch (System.ObjectDisposedException)
            {
                // Likely called when called from OnDestroy when they were already disposed in update loop.
                // We can ignore.
            }            
        }
    }
}
