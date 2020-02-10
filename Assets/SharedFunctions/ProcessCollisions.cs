using UnityEngine;
using System.Collections.Generic;

public class ProcessCollisions {

    public static void ProcessSphereCollisions(List<uint> toProcess){

        ECSManager manager = ECSManager.Instance;

        //Traitement des collisions
        foreach(uint id in toProcess)
        {
            float newSize = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;

            //Réduction de 50% du rayon du cercle
            manager.UpdateShapeSize(id, newSize);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
            SizeComponent shapeSize = new SizeComponent();
            shapeSize.size = newSize;
            ComponentHandler.SetComponent<SizeComponent>(id, shapeSize);

            //Mise à jour du tag "traversable"
            if(newSize < manager.Config.minSize)
            {
                manager.UpdateShapeColor(id, Color.green);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
                IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                ComponentHandler.SetComponent<IsTraversableComponent>(id, noCollisionTag);
            }
        }
    }

    public static void ProcessScreenCollisions(uint id) {
        
        ECSManager manager = ECSManager.Instance;

        float radius = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;
        float initialSize = ComponentHandler.GetComponent<InitialSizeComponent>(id).initialSize;
        Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;
        Vector2 speed = ComponentHandler.GetComponent<SpeedComponent>(id).speed;

        Vector3 shapeWorldPos = new Vector3(position[0], position[1], 0.0f);
        Vector3 shapeScreenPos = Camera.main.WorldToScreenPoint(shapeWorldPos);

        //Détection des collisions avec les bords de l'écran (si le cercle courant n'est pas statique)
        if(speed!=Vector2.zero)
        {
            // Vector3 horizWorldRadius = new Vector3(radius, 0.0f, 0.0f);
            // Vector3 horizScreenRadius = Camera.main.WorldToScreenPoint(horizWorldRadius);
            // float hScreenRadius = horizScreenRadius.x;
            float hScreenRadius = 60.0f*radius;

            // Vector3 vertWorldRadius = new Vector3(0.0f, radius, 0.0f);
            // Vector3 vertScreenRadius = Camera.main.WorldToScreenPoint(vertWorldRadius);
            // float vScreenRadius = vertScreenRadius.y;
            float vScreenRadius = 60.0f*radius;

            //Y a-t-il contact avec le bord de l'écran ?
            if(shapeScreenPos.x <= hScreenRadius | shapeScreenPos.x >= Screen.width - hScreenRadius | shapeScreenPos.y <= vScreenRadius | shapeScreenPos.y >= Screen.height - vScreenRadius)
            {
                Vector2 nextSpeed = speed;

                //Avant une sortie d'écran par la gauche / droite
                if(shapeScreenPos.x <= hScreenRadius & nextSpeed.x < 0 | shapeScreenPos.x > Screen.width - hScreenRadius & nextSpeed.x > 0 )
                {
                    nextSpeed.x = - nextSpeed.x;
                }

                //Avant une sortie d'écran par le haut / bas
                if(shapeScreenPos.y <= vScreenRadius & nextSpeed.y < 0  | shapeScreenPos.y > Screen.height - vScreenRadius & nextSpeed.y > 0 )
                {
                    nextSpeed.y = - nextSpeed.y;
                }

                //Mise à jour du vecteur vitesse
                SpeedComponent shapeSpeed = new SpeedComponent();
                shapeSpeed.speed = nextSpeed;
                ComponentHandler.SetComponent<SpeedComponent>(id, shapeSpeed);

                speed = nextSpeed;

                //Réinitialisation de la taille de l'entité
                manager.UpdateShapeSize(id, initialSize);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
                SizeComponent shapeSize = new SizeComponent();
                shapeSize.size = initialSize;
                ComponentHandler.SetComponent<SizeComponent>(id, shapeSize);

                //Réinitialisation du type de l'entité
                if(initialSize >= manager.Config.minSize)
                {
                    manager.UpdateShapeColor(id, Color.blue);
                    if(ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(id))
                    {
                        ComponentHandler.RemoveComponent<IsTraversableComponent>(id);
                    }
                }
            }
        }
    }

}