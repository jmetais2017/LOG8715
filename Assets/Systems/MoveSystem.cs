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
        float frameDuration = Time.deltaTime;

        //Déplacement
        foreach(KeyValuePair<uint, EntityComponent> keyValuePair in ComponentHandler.GetComponentsOfType<EntityComponent>())
        {
            //Récupération des données utiles sur l'entité courante
            uint id = keyValuePair.Value.id;
            float radius = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;
            Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;
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


        //Liste des id des cercles dynamiques concernés par une collision (et qu'il faudra donc réduire et tagger)
        List<uint> toProcess = new List<uint>();

        //Détection des collisions cercle-cercle
        foreach(KeyValuePair<uint, EntityComponent> keyValuePair in ComponentHandler.GetComponentsOfType<EntityComponent>())
        {
            //Récupération des données utiles sur l'entité courante
            uint id = keyValuePair.Value.id;
            float radius = ComponentHandler.GetComponent<SizeComponent>(id).size / 2.0f;
            Vector2 position = ComponentHandler.GetComponent<PositionComponent>(id).position;
            Vector2 speed = ComponentHandler.GetComponent<SpeedComponent>(id).speed;


            //Détection des collisions avec les autres cercles (si le cercle courant est non-statique et non-traversable)
            if(!ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(id) & speed!=Vector2.zero)
            {
                //On teste toutes les autres entités
                foreach(KeyValuePair<uint, EntityComponent> otherKeyValuePair in ComponentHandler.GetComponentsOfType<EntityComponent>())
                {
                    uint otherId = otherKeyValuePair.Value.id;

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


        //Collisions écran-cercle
        foreach(KeyValuePair<uint, EntityComponent> keyValuePair in ComponentHandler.GetComponentsOfType<EntityComponent>())
        {
            //Récupération des données utiles sur l'entité courante
            uint id = keyValuePair.Value.id;
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


        //Mise à jour des historiques (1 seule fois par frame)
        foreach(KeyValuePair<uint, EntityComponent> keyValuePair in ComponentHandler.GetComponentsOfType<EntityComponent>())
        {
            //Récupération des données utiles sur l'entité courante
            uint id = keyValuePair.Value.id;
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