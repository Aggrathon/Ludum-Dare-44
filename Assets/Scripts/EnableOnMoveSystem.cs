using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

public class EnableOnMoveSystem : ComponentSystem
{
    
    protected override void OnUpdate()
    {
        var mgr = World.Active.EntityManager;
        Entities.ForEach((Entity entity, ref Translation translation, ref EnableOnMove eom) => {
            if (math.distancesq(translation.Value, eom.prevPos) > 0f) {
                eom.prevPos = translation.Value;
                if (eom.disabled && mgr.HasComponent(entity, typeof(Disabled))) {
                    eom.disabled = false;
                    PostUpdateCommands.RemoveComponent(entity, typeof(Disabled));
                }
            } else {
                if (!eom.disabled && !mgr.HasComponent(entity, typeof(Disabled))) {
                    eom.disabled = true;
                    PostUpdateCommands.AddComponent(entity, new Disabled());
                }
            }
        });
    }
}