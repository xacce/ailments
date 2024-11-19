﻿using Core.Hybrid;
using GameReady.Ailments.Runtime;
using Src.PackageCandidate.Ailments.Runtime;

namespace Src.PackageCandidate.Ailments.Hybrid
{
    public abstract class AilmentBakedSo : BakedScriptableObject<AilmentBlob>, IUniqueIdProvider
    {
        public abstract int id { get; }
        public abstract Ailment ailment { get; }
    }
}