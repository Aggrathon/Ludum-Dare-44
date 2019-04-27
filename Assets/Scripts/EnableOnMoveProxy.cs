using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct EnableOnMove : IComponentData
{
    [NonSerialized] public float3 prevPos;
    [NonSerialized] public bool disabled;
}


public class EnableOnMoveProxy : ComponentDataProxy<EnableOnMove>
{
}
