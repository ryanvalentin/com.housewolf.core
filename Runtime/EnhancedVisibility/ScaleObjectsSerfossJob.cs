using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace Housewolf
{
    [BurstCompile]
    public struct ScaleObjectsSerfossJob : IJobParallelForTransform
    {
        public const float SERFOSS_RESOLUTION_FACTOR = 0.09226f;

        public const float SERFOSS_FOV_FACTOR = 0.00148f;

        public const float SERFOSS_RANGE_DOWNSAMPLE_FACTOR = 1000f;

        [ReadOnly]
        public float3 CameraPosition;

        [ReadOnly]
        public NativeArray<float3> OriginalScales;

        [ReadOnly]
        public NativeArray<bool> EnabledObjects;

        public void Execute(int index, TransformAccess transform)
        {
            if (!EnabledObjects[index])
                return;

            ScaleRealDistance(index, transform);
            //ScaleDistanceSq(index, transform);
        }

        /* This is a less dramatic version if desired, create a new job for it.
        private void ScaleDistanceSq(int index, TransformAccess transform)
        {
            float mftConversionSq = ConversionRatios.M_FT_CONVERSION * ConversionRatios.M_FT_CONVERSION;
            float sfResolutionFactorSq = SERFOSS_RESOLUTION_FACTOR * SERFOSS_RESOLUTION_FACTOR;
            float sfRangeDownsampleFactorSq = SERFOSS_RANGE_DOWNSAMPLE_FACTOR * SERFOSS_RANGE_DOWNSAMPLE_FACTOR;
            float _serfossFovFactorSq = SERFOSS_FOV_FACTOR * SERFOSS_FOV_FACTOR;

            float distanceSq = math.distancesq(transform.position, CameraPosition) * mftConversionSq;
            distanceSq /= sfRangeDownsampleFactorSq;
            float scaleAmount = sfResolutionFactorSq * distanceSq - _serfossFovFactorSq * math.sqrt(distanceSq);

            transform.localScale = OriginalScales[index] + scaleAmount;
        }
        */

        private void ScaleRealDistance(int index, TransformAccess transform)
        {
            float distance = math.distance(transform.position, CameraPosition) * ConversionRatios.M_FT_CONVERSION;
            distance /= SERFOSS_RANGE_DOWNSAMPLE_FACTOR;
            float scaleAmount = SERFOSS_RESOLUTION_FACTOR * distance - SERFOSS_FOV_FACTOR * math.sqrt(distance);

            transform.localScale = OriginalScales[index] + scaleAmount;
        }
    }
}