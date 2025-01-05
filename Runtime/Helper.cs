using System.Runtime.CompilerServices;
using Src.PackageCandidate.Ailments.Runtime;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameReady.Ailments.Runtime
{
    [BurstCompile]
    public static class Helper
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddAilment(ref AilmentCreatedContext ctx, in AilmentRuntime apply, ref DynamicBuffer<AilmentRuntime> elements,
            ref DynamicBuffer<ActiveAilmentCounter> counter, out int ailmentIndex)
        {
            ailmentIndex = -1;
            var stackGroupId = apply.rootRuntimeData.stackGroupId;
            var count = GetAilmentCount(stackGroupId, ref counter, out var counterIndex);
            if (!(apply.rootRuntimeData.duration > 0)) return false;
            switch (apply.rootRuntimeData.stackMode)
            {
                case StackMode.Override:
                {
                    if (count >= apply.rootRuntimeData.maxStacks)
                    {
                        if (TryGetFirstIndexById(stackGroupId, ref elements, out ailmentIndex))
                        {
                            //todo refact this pls, less optimization
                            elements[ailmentIndex].ailment.OnExpired(ref ctx, elements[ailmentIndex]);
                            elements[ailmentIndex] = apply;
                            elements[ailmentIndex].ailment.OnFresh(ref ctx, elements[ailmentIndex]);
                            return true;
                        }
                    }

                    ailmentIndex = elements.Length;
                    elements.Add(apply);
                    elements[ailmentIndex].ailment.OnFresh(ref ctx, elements[ailmentIndex]);
                    break;
                }
                case StackMode.Discard:
                    if (count >= apply.rootRuntimeData.maxStacks) return false;
                    ailmentIndex = elements.Length;
                    elements.Add(apply);
                    break;
            }

            if (ailmentIndex == -1)
            {
                Debug.Log("Less");
                return false;
            }

            if (counterIndex == -1)
            {
                counter.Add(
                    new ActiveAilmentCounter()
                    {
                        counter = 1,
                        id = stackGroupId,
                    });
            }
            else
            {
                var counterAilment = counter[counterIndex];
                counterAilment.counter++;
                counter[counterIndex] = counterAilment;
            }

            // ref var activeAilment = ref elements.ElementAt(ailmentIndex);
            return true;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAilmentCount(int ailmentId, ref DynamicBuffer<ActiveAilmentCounter> counter, out int index)
        {
            for (var i = 0; i < counter.Length; i++)
            {
                var c = counter[i];
                if (c.id == ailmentId)
                {
                    // c.counter++;
                    // counter[i] = c;
                    index = i;
                    return c.counter;
                }
            }

            index = -1;
            return 0;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateAilmentCount(ref DynamicBuffer<ActiveAilmentCounter> counter, int ailmentId, int decr)
        {
            for (var i = 0; i < counter.Length; i++)
            {
                var c = counter[i];
                if (c.id == ailmentId)
                {
                    c.counter -= decr;
                    counter[i] = c;
                    return;
                }
            }
        }


        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFirstIndexById(int ailmentId, ref DynamicBuffer<AilmentRuntime> elements, out int index)
        {
            for (var i = 0; i < elements.Length; i++)
            {
                var c = elements[i];
                if (c.rootRuntimeData.stackGroupId == ailmentId)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetAilmentCount(int ailmentId, ref DynamicBuffer<ActiveAilmentCounter> counter)
        {
            for (var i = 0; i < counter.Length; i++)
            {
                var c = counter[i];
                if (c.id == ailmentId) return c.counter;
            }

            return 0;
        }
    }
}