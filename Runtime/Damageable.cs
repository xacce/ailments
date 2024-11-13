using System;
using GameReady.Ailments.Runtime;
using Sufferenger;
using Unity.Entities;
using Unity.Mathematics;

namespace Src.PackageCandidate.GameReady.Ailments.Runtime
{
    [InternalBufferCapacity(0)]
    public partial struct DamageAilmentInflict : IBufferElementData
    {
        public struct Blob
        {
            public DamageAttributeScaleMap chancesMap; //Идексы атрибутов откуда брать шанс на срабатывание
            public AilmentConstructors constructors; //список конструкторов
            public int3x3 indexes; //индексы в поле выше для каждого типа урона, -1 если не задействован

            //chance[x][y] -> constructors[indexes[x][y]] -> construct -> apply
        }

        //Содержит данные о недугах которые применяются от базового урона
        public BlobAssetReference<Blob> blob;
    }
}