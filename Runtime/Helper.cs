using System.Runtime.CompilerServices;
using Core.Runtime.LatiosHashMap.Latios;
using Src.PackageCandidate.Ailments.Runtime;
using Src.PackageCandidate.LogTest;
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
        public static bool TryAddAilment(
            ref AilmentCreatedContext ctx,
            in AilmentRuntime apply,
            ref DynamicHashMap<int, int2> mapped,
            ref DynamicBuffer<AilmentRuntime> elements,
            Entity target, out int ailmentIndex)
        {
            ailmentIndex = -1;
            var stackGroupId = apply.rootRuntimeData.stackGroupId;
            if (!(apply.rootRuntimeData.duration > 0)) return false;
            int insertIndex = -1;
            int overrideIndex = -1;
            int2 stackGroupMapData = int2.zero;
            if (mapped.TryGetValue(stackGroupId, out stackGroupMapData))
            {
                var currentStacksCount = stackGroupMapData.y;
                if (currentStacksCount < apply.rootRuntimeData.maxStacks)
                {
                    //Current ailment is not full, we can just add it
                    insertIndex = stackGroupMapData.x + stackGroupMapData.y;
                }
                else
                {
                    for (int i = stackGroupMapData.x; i < stackGroupMapData.x + stackGroupMapData.y; i++)
                    {
                        var exists = elements[i];
                        if (exists.rootRuntimeData.value < apply.rootRuntimeData.value)
                        {
                            //We have weak ailment , just override it
                            overrideIndex = i;
                            break;
                        }
                    }

                    if (overrideIndex == -1)
                    {
                        //Stacks is full and no weaks ailments, skip
                        return false;
                    }
                }
            }

            if (insertIndex == -1 && overrideIndex == -1)
            {
                //Its new ailment, just append and map
                var index = elements.Length;
                elements.Add(apply);
                apply.ailment.OnFresh(ref ctx, elements[index]);
                mapped.AddOrSet(stackGroupId, new int2(index, 1));
                GameDebug.Log("Ailment", $"New ailment {apply.rootRuntimeData.stackGroupId} was added to {target}");
            }
            else if (insertIndex != -1)
            {
                elements.Insert(insertIndex, apply);
                apply.ailment.OnFresh(ref ctx, elements[insertIndex]);
                stackGroupMapData.y++;
                mapped.AddOrSet(stackGroupId, stackGroupMapData);
                GameDebug.Log("Ailment", $"New ailment {apply.rootRuntimeData.stackGroupId} was inserted to {target}");
            }
            else if (overrideIndex != -1)
            {
                var overrideAilment = elements[overrideIndex];
                overrideAilment.ailment.OnExpired(ref ctx, overrideAilment);
                elements[overrideIndex] = apply;
                apply.ailment.OnFresh(ref ctx, overrideAilment);
                GameDebug.Log("Ailment", $"New ailment {apply.rootRuntimeData.stackGroupId} was override to {target}");
            }


            // ref var activeAilment = ref elements.ElementAt(ailmentIndex);
            return true;
        }
    }
}