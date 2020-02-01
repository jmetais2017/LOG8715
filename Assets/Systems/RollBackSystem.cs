using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public static void RollBack()
// {
    // float currentTime = Time.time;
    // foreach (var entity in taggedEntities)
    // {
    //     formerFrameId = frameTimes.findIdWhere(currentTime - 2.0f);

    //     positions[entity.id] = positionsHistory[entity.id][formerFrameId];
    //     speeds[entity.id] = speedsHistory[entity.id][formerFrameId];
    //     sizes[entity.id] = sizesHistory[entity.id][formerFrameId];
    // }
// }

public class RollBackSystem : ISystem
{
    public string Name 
    { 
        get
        {
            return "rollBack";
        }
    }


    public void UpdateSystem()
    {
        float frameTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(frameTime - ComponentHandler.lastRollback > 2.0f)
            {
                //Si plus de 2 secondes se sont écoulées depuis la dernière activation, on effectue le roll back


                //TODO : reset des components


                //Actualisation du cooldown
                ComponentHandler.lastRollback = frameTime;

                Debug.Log("Retour en arrière de 2 secondes !");
            }
            else
            {
                //Sinon, on affiche l'état du cooldown
                Debug.Log("Cooldown restant : ");
                Debug.Log(2.0f - (frameTime - ComponentHandler.lastRollback));
            }
        }
    }
}

