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
            ref DynamicHashMap<int, AilmentInfo> count,
            ref DynamicBuffer<AilmentRuntime> elements,
            in Entity target, out int ailmentIndex)
        {
            ailmentIndex = -1;
            var stackGroupId = apply.rootRuntimeData.stackGroupId;
            if (!(apply.rootRuntimeData.duration > 0)) return false;
            // int insertIndex = -1;
            // bool isNew = true;
            int overrideIndex = -1;
            AilmentInfo currentStacksCount = default;
            GameDebug.Spam("Ailment", $"Try to find stack group {stackGroupId}", target);
            if (count.TryGetValue(stackGroupId, out  currentStacksCount))
            {
                GameDebug.Spam("Ailment", $"Stack group found {stackGroupId}, max stacks: {apply.rootRuntimeData.maxStacks}, current stacks: {currentStacksCount}", target);
                if (currentStacksCount.stacksCount == 0 && apply.rootRuntimeData.maxStacks > 0)
                {
                    GameDebug.Spam("Ailment", $"Its new* ailment, we need update map - just append ", target);
                }
                else if (currentStacksCount.stacksCount < apply.rootRuntimeData.maxStacks)
                {
                    //Current ailment is not full, we can just add it
                    GameDebug.Spam("Ailment", $"Is just inserting", target);
                    // insertIndex = stackGroupMapData.x;
                }
                else
                {
                    //todo iterate over all??
                    for (int i = 0; i < elements.Length; i++)
                    {
                        var exists = elements[i];
                        if (exists.rootRuntimeData.stackGroupId==stackGroupId && exists.rootRuntimeData.value < apply.rootRuntimeData.value)
                        {
                            //We have weak ailment , just override it
                            GameDebug.Spam("Ailment", $"Is just override", target);
                            overrideIndex = i;
                            break;
                        }
                    }

                    if (overrideIndex == -1)
                    {
                        //Stacks is full and no weaks ailments, skip
                        GameDebug.Spam("Ailment", $"Is just skip", target);
                        return false;
                    }
                }
            }

            if (overrideIndex == -1)
            {
                //Its new ailment, just append and map
                var index = elements.Length;
                currentStacksCount.stacksCount++;
                currentStacksCount.blob = apply.blob;
                GameDebug.Spam("Ailment", $"New ailment. Appliend new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                elements.Add(apply);
                GameDebug.Spam("Ailment", $"New ailment. Freshing new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                apply.ailment.OnFresh(ref ctx, elements[index]);
                GameDebug.Spam("Ailment", $"New ailment. Update map new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                count.AddOrSet(stackGroupId, currentStacksCount);
                GameDebug.Spam("Ailment", $"New ailment. New ailment {apply.rootRuntimeData.stackGroupId} was added to {target}");
            }
            else if (overrideIndex != -1)
            {
                GameDebug.Spam("Ailment", $"Override ailment.New ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                var overrideAilment = elements[overrideIndex];
                GameDebug.Spam("Ailment", $"Override ailment.Expiring new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                overrideAilment.ailment.OnExpired(ref ctx, overrideAilment);
                GameDebug.Spam("Ailment", $"Override ailment.Updating new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                elements[overrideIndex] = apply;
                GameDebug.Spam("Ailment", $"Override ailment.Freshing new ailment {apply.rootRuntimeData.stackGroupId} to {target}");
                apply.ailment.OnFresh(ref ctx, overrideAilment);
                GameDebug.Spam("Ailment", $"New ailment {apply.rootRuntimeData.stackGroupId} was override to {target}");
            }


            // ref var activeAilment = ref elements.ElementAt(ailmentIndex);
            return true;
        }
    }
}