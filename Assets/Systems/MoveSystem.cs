using UnityEngine;
using System.Collections.Generic;

public class MoveSystem : ISystem 
{
    public string Name 
    { 
        get
        {
            return "movement";
        }
    }

    public void UpdateSystem()
    {
        ECSManager manager = ECSManager.Instance;

        //Déplacement + collisions bord de l'écran
        // Seules les sphères ayant une vitesse doivent bouger
        foreach(KeyValuePair<uint, SpeedComponent> keyValuePair in ComponentHandler.GetComponentsOfType<SpeedComponent>())
        {
            uint id = keyValuePair.Key;
            MoveSphere.Move(id);
            ProcessCollisions.ProcessScreenCollisions(id);
        }


        //Liste des id des cercles dynamiques concernés par une collision (et qu'il faudra donc réduire et tagger)
        List<uint> toProcess = CheckCollisions.FindCollisions(false);

        ProcessCollisions.ProcessSphereCollisions(toProcess);



        //Mise à jour des historiques (1 seule fois par frame)
        foreach(KeyValuePair<uint, SpeedComponent> keyValuePair in ComponentHandler.GetComponentsOfType<SpeedComponent>())
        {
            //Récupération des données utiles sur l'entité courante
            uint id = keyValuePair.Key;
            float size = ComponentHandler.GetComponent<SizeComponent>(id).size;
            Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;
            Vector2 speed = ComponentHandler.GetComponent<SpeedComponent>(id).speed;


            ComponentHandler.GetComponent<PositionHistoryComponent>(id).positionHistory.AddLast(position);
            if(ComponentHandler.GetComponent<PositionHistoryComponent>(id).positionHistory.Count > 2*ComponentHandler.GetComponent<MaxFramesPerSecComponent>(0).maxFramesPerSec)
            {
                ComponentHandler.GetComponent<PositionHistoryComponent>(id).positionHistory.RemoveFirst();
            }

            ComponentHandler.GetComponent<SizeHistoryComponent>(id).sizeHistory.AddLast(size);
            if(ComponentHandler.GetComponent<SizeHistoryComponent>(id).sizeHistory.Count > 2*ComponentHandler.GetComponent<MaxFramesPerSecComponent>(0).maxFramesPerSec)
            {
                ComponentHandler.GetComponent<SizeHistoryComponent>(id).sizeHistory.RemoveFirst();
            }

            ComponentHandler.GetComponent<SpeedHistoryComponent>(id).speedHistory.AddLast(speed);
            if(ComponentHandler.GetComponent<SpeedHistoryComponent>(id).speedHistory.Count > 2*ComponentHandler.GetComponent<MaxFramesPerSecComponent>(0).maxFramesPerSec)
            {
                ComponentHandler.GetComponent<SpeedHistoryComponent>(id).speedHistory.RemoveFirst();
            }
        }
    }
}