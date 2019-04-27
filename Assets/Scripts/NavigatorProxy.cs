using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Navigator : IComponentData
{
    public float3 target;

    public float speed;
    public float turning;
    public bool pause;
}

public class NavigatorProxy : ComponentDataProxy<Navigator>
{
}
