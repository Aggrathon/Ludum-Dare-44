using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class SoundDestroySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity ent, ref Translation tr, ref SoundDestroy sd) => {
            sd.time -= UnityEngine.Time.deltaTime;
            if (sd.time < 0) {
                PostUpdateCommands.DestroyEntity(ent);
            } else if (sd.soundId > 0) {
                //TODO play sound
            }
        });
    }
}