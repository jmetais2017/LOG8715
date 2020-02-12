using UnityEngine;
using System.Collections.Generic;

public class MoveSphere {

    public static void Move(uint id) {

        ECSManager manager = ECSManager.Instance;
        float frameDuration = Time.deltaTime;

        float radius = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;
        Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;

        // sanity check : if there is no speed, we don't move
        if (!ComponentHandler.HasComponent<SpeedComponent>(id))
        {
            return;
        }
        Vector2 speed = ComponentHandler.GetComponent<SpeedComponent>(id).speed;

        //Mise à jour de la position selon sa vitesse
        Vector2 newPos = position + speed*frameDuration;
        
        manager.UpdateShapePosition(id, newPos); //Pas nécessaire d'update l'affichage 5 fois dans la même frame
        PositionComponent shapePos = new PositionComponent();
        shapePos.position = newPos;
        ComponentHandler.SetComponent<PositionComponent>(id, shapePos);

        //La nouvelle position se trouve-t-elle dans la moitié supérieure de l'écran ?
        Vector3 shapeWorldPos = new Vector3(newPos[0], newPos[1], 0.0f);
        Vector3 shapeScreenPos = Camera.main.WorldToScreenPoint(shapeWorldPos);

        if(shapeScreenPos.y >= Screen.height / 2.0f)
        {
            if(!ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>().ContainsKey(id))
            {
                IsOnTopHalfComponent upperHalfTag = new IsOnTopHalfComponent();
                ComponentHandler.SetComponent<IsOnTopHalfComponent>(id, upperHalfTag);
            }
        }
        else
        {
            if(ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>().ContainsKey(id))
            {
                ComponentHandler.RemoveComponent<IsOnTopHalfComponent>(id);
            }
        }
    }
}