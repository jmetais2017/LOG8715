using UnityEngine;
using System.Collections.Generic;

public class MoveSystem : ISystem {

    // Checks if the new position (calculated as actual position + speed) would collide with something.
    // If yes, CollideEntity is called on both entities. 
    // In both cases, the position is updated with position + speed. Note that in case of a collision,
    // this position update will use the post-collision speed.

    // Reads SpeedComponent, SizeComponent.
    // Reads and writes PositionComponent.
    public static void CheckCollisionsAndMove(uint id) {
        Vector2 speed = TODOComponents<SpeedComponent>[id].speed;
        Vector2 position = TODOComponents<PositionComponent>[id].position;
        Vector2 newPos = position + speed;
        float size = TODOComponents<SizeComponent>[id].last();

        bool hasCollided = false;

        foreach (uint idOther in TODOComponents<EntityComponent>) {
            Vector2 otherPos = TODOComponents<PositionsComponent>[id];
            float otherSize = TODOComponents<SizesComponent>[id];
            if (Vector2.Distance(newPos, otherPos) < size + otherSize){
                CollideEntity(otherId);
                hasCollided = true;
            }
        }

        if (hasCollided){
            CollideEntity(id);
            // Updates the speed and the resulting position
            speed = -speed;
            newPos = position + speed;
        }

        // Updates the history of positions, speed, and sizes
        TODOComponents<PositionComponent>[id].position = newPos;
    }

    // Handles what happens when a collision happens, except for the speed update.
    public static void CollideEntity(uint id) {

    }
}