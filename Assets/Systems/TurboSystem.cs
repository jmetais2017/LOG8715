using UnityEngine;
using System.Collections.Generic;

public class TurboSystem : ISystem 
{
    public string Name 
    { 
        get
        {
            return "turbo";
        }
    }

    public void UpdateSystem()
    {
        ECSManager manager = ECSManager.Instance;

        // 4 itérations (on a déjà fait bouger tout le monde une fois : 5x plus vite veut dire 4 mouvements de plus)
        for (int i = 0; i < 4; i++) {
            //Déplacement + collisions bord de l'écran
            foreach(KeyValuePair<uint, IsOnTopHalfComponent> keyValuePair in ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>())
            {
                uint id = keyValuePair.Key;
                MoveSphere.Move(id);
                ProcessCollisions.ProcessScreenCollisions(id);
            }
            
            //Liste des id des cercles dynamiques concernés par une collision (et qu'il faudra donc réduire et tagger)
            List<uint> toProcess = CheckCollisions.FindCollisions(true);

            ProcessCollisions.ProcessSphereCollisions(toProcess);
            }

    }
}