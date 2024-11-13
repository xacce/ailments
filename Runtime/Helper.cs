﻿using System.Runtime.CompilerServices;
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
        // [BurstCompile]
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static int GetAilmentTicksCount(in ActiveAilmentElement.Effectivity effectivity, int baseTicks)
        // {
        //     Debug.Log($"{effectivity.ticksEffectivity} = {(int)math.floor(baseTicks * effectivity.ticksEffectivity)}");
        //     return (int)math.floor(baseTicks * effectivity.ticksEffectivity);
        // }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddAilment(in ConstructedAilment apply, ref DynamicBuffer<ConstructedAilment> elements,
            ref DynamicBuffer<ActiveAilmentCounter> counter, out int ailmentIndex)
        {
            ailmentIndex = -1;
            var stackGroupId = apply.root.stackGroupId;
            var count = GetAilmentCount(stackGroupId, ref counter, out var counterIndex);

            switch (apply.root.stackMode)
            {
                case StackMode.Override:
                {
                    if (count >= apply.root.maxStacks)
                    {
                        if (TryGetFirstIndexById(stackGroupId, ref elements, out ailmentIndex))
                        {
                            elements[ailmentIndex] = apply;
                            return true;
                        }
                    }
                    ailmentIndex = elements.Length;
                    elements.Add(apply);
                    break;
                }
                case StackMode.Discard:
                    if (count >= apply.root.maxStacks) return false;
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
        public static bool TryGetFirstIndexById(int ailmentId, ref DynamicBuffer<ConstructedAilment> elements, out int index)
        {
            for (var i = 0; i < elements.Length; i++)
            {
                var c = elements[i];
                if (c.root.stackGroupId == ailmentId)
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