using UnityEngine;
using System.Collections.Generic;

public class CheckCollisions{

    private static void UpdateCollisionsOn(uint id, List<uint> toProcess) {

        float radius = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;
        Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;
        Vector2 speed = ComponentHandler.GetComponent<SpeedComponent>(id).speed;


        //Détection des collisions avec les autres cercles (si le cercle courant est non-statique et non-traversable)
        if(!ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(id) & speed!=Vector2.zero)
        {
            //On teste toutes les autres entités
            foreach(KeyValuePair<uint, SpeedComponent> otherKeyValuePair in ComponentHandler.GetComponentsOfType<SpeedComponent>())
            {
                uint otherId = otherKeyValuePair.Key;

                //Seulement si l'autre entité est non-traversable et différente de l'entité courante
                if(!ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(otherId) & id != otherId)
                {
                    float otherRadius = ComponentHandler.GetComponent<SizeComponent>(otherId).size / 2.0f;
                    Vector2 otherPosition = ComponentHandler.GetComponent<PositionComponent>(otherId).position;
                    float distance = (otherPosition - position).magnitude;

                    //Y a-t-il collision ?
                    if(distance < otherRadius + radius)
                    {
                        toProcess.Add(id);

                        //Mise à jour du vecteur vitesse
                        Vector2 newSpeed = (position - otherPosition) * speed.magnitude / distance;

                        SpeedComponent shapeSpeed = new SpeedComponent();
                        shapeSpeed.speed = newSpeed;
                        ComponentHandler.SetComponent<SpeedComponent>(id, shapeSpeed);
                    }
                }
            }
        }
    }

    public static List<uint> FindCollisions(bool onTopHalf) {
        List<uint> toProcess = new List<uint>();

        //Détection des collisions cercle-cercle
        if (onTopHalf) {
            foreach(KeyValuePair<uint, IsOnTopHalfComponent> keyValuePair in ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>())
            {
                //Récupération des données utiles sur l'entité courante
                uint id = keyValuePair.Key;
                UpdateCollisionsOn(id, toProcess);
            }
        } else {
            foreach(KeyValuePair<uint, SpeedComponent> keyValuePair in ComponentHandler.GetComponentsOfType<SpeedComponent>())
            {
                //Récupération des données utiles sur l'entité courante
                uint id = keyValuePair.Key;
                UpdateCollisionsOn(id, toProcess);
            }
        }
        
        
        return toProcess;
    }
}