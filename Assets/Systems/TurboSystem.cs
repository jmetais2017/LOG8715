using UnityEngine;
using System.Collections.Generic;

// S'occupe d'effectuer tout ce qui a trait au déplacement (le déplacement en soi, plus la détection et gestion
// des collisions) 4 fois supplémentaires, sur les objets en haut de l'écran. À noter, la vérification des 
// objets en haut de l'écran est fait à chaque itération : si un objet sort du haut de l'écran au bout de 2
// itérations, il ne bougera plus lors des 2 suivantes.

// Accès : PositionComponent, SpeedComponent, SizeComponent, PositionHistoryComponent, 
// SizeHistoryComponent, SpeedHistoryComponent, IsTraversableComponent, IsOnTopOfScreenComponent 
// (lecture, écriture),
// InitialSizeComponent (lecture)

// (les mêmes que MoveSystem)
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
            }

            foreach(KeyValuePair<uint, IsOnTopHalfComponent> keyValuePair in ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>())
            {
                uint id = keyValuePair.Key;
                ProcessCollisions.ProcessScreenCollisions(id);
            }
            
            //Liste des id des cercles dynamiques concernés par une collision (et qu'il faudra donc réduire et tagger)
            HashSet<uint> toProcess = CheckCollisions.FindCollisions(true);

            ProcessCollisions.ProcessSphereCollisions(toProcess);
            }

    }
}