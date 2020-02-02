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

    // Checks if the new position (calculated as actual position + speed) would collide with something.
    // If yes, CollideEntity is called on both entities. 
    // In both cases, the position is updated with position + speed. Note that in case of a collision,
    // this position update will use the post-collision speed.

    // Reads SpeedComponent, SizeComponent.
    // Reads and writes PositionComponent.
    // public static void CheckCollisionsAndMove(uint id) {
    //     Vector2 speed = TODOComponents<SpeedComponent>[id].speed;
    //     Vector2 position = TODOComponents<PositionComponent>[id].position;
    //     Vector2 newPos = position + speed;
    //     float size = TODOComponents<SizeComponent>[id].last();

    //     bool hasCollided = false;

    //     foreach (uint idOther in TODOComponents<EntityComponent>) {
    //         Vector2 otherPos = TODOComponents<PositionsComponent>[id];
    //         float otherSize = TODOComponents<SizesComponent>[id];
    //         if (Vector2.Distance(newPos, otherPos) < size + otherSize){
    //             CollideEntity(otherId);
    //             hasCollided = true;
    //         }
    //     }

    //     if (hasCollided){
    //         CollideEntity(id);
    //         // Updates the speed and the resulting position
    //         speed = -speed;
    //         newPos = position + speed;
    //     }

    //     // Updates the history of positions, speed, and sizes
    //     TODOComponents<PositionComponent>[id].position = newPos;
    // }

    // Handles what happens when a collision happens, except for the speed update.
    // public static void CollideEntity(uint id) {

    // }


    public void UpdateSystem()
    {
        ECSManager manager = ECSManager.Instance;
        float frameDuration = Time.deltaTime;

        //Déplacement
        ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
        {
            //Récupération des données utiles sur l'entité courante
            uint id = entity.id;
            float radius = ComponentHandler.sizes[(int) id].size / 2.0f;
            Vector2 position = ComponentHandler.positions[(int) id].position;
            Vector2 speed = ComponentHandler.speeds[(int) id].speed;

            //Mise à jour de la position selon sa vitesse
            Vector2 newPos = position + speed*frameDuration;
            
            manager.UpdateShapePosition(id, newPos); //Pas nécessaire d'update l'affichage 5 fois dans la même frame
            PositionComponent shapePos = new PositionComponent();
            shapePos.position = newPos;
            ComponentHandler.positions[(int) id] = shapePos;

            //La nouvelle position se trouve-t-elle dans la moitié supérieure de l'écran ?
            Vector3 shapeWorldPos = new Vector3(newPos[0], newPos[1], 0.0f);
            Vector3 shapeScreenPos = Camera.main.WorldToScreenPoint(shapeWorldPos);

            if(shapeScreenPos.y >= Screen.height / 2.0f)
            {
                if(!ComponentHandler.upperHalf.ContainsKey(id))
                {
                    IsOnTopHalfComponent upperHalfTag = new IsOnTopHalfComponent();
                    ComponentHandler.upperHalf.Add(id, upperHalfTag);
                }
            }
            else
            {
                if(ComponentHandler.upperHalf.ContainsKey(id))
                {
                    ComponentHandler.upperHalf.Remove(id);
                }
            }
        });


        //Liste des id des cercles dynamiques concernés par une collision (et qu'il faudra donc réduire et tagger)
        List<uint> toProcess = new List<uint>();

        //Détection des collisions cercle-cercle
        ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
        {
            //Récupération des données utiles sur l'entité courante
            uint id = entity.id;
            float radius = ComponentHandler.sizes[(int) id].size / 2.0f;
            Vector2 position = ComponentHandler.positions[(int) id].position;
            Vector2 speed = ComponentHandler.speeds[(int) id].speed;


            //Détection des collisions avec les autres cercles (si le cercle courant est non-statique et non-traversable)
            if(!ComponentHandler.traversables.ContainsKey(id) & speed!=Vector2.zero)
            {
                //On teste toutes les autres entités
                ComponentHandler.entities.ForEach(delegate(EntityComponent otherEntity)
                {
                    uint otherId = otherEntity.id;

                    //Seulement si l'autre entité est non-traversable et différente de l'entité courante
                    if(!ComponentHandler.traversables.ContainsKey(otherId) & id != otherId)
                    {

                        float otherRadius = ComponentHandler.sizes[(int) otherId].size / 2.0f;
                        Vector2 otherPosition = ComponentHandler.positions[(int) otherId].position;
                        float distance = (otherPosition - position).magnitude;

                        //Y a-t-il collision ?
                        if(distance < otherRadius + radius)
                        {
                            toProcess.Add(id);

                            //Mise à jour du vecteur vitesse
                            Vector2 newSpeed = (position - otherPosition) * speed.magnitude / distance;

                            SpeedComponent shapeSpeed = new SpeedComponent();
                            shapeSpeed.speed = newSpeed;
                            ComponentHandler.speeds[(int) id] = shapeSpeed;
                        }
                    }
                });
            }
        });


        //Traitement des collisions
        foreach(uint id in toProcess){
            float newSize = ComponentHandler.sizes[(int) id].size / 2.0f;

            //Réduction de 50% du rayon du cercle
            manager.UpdateShapeSize(id, newSize);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
            SizeComponent shapeSize = new SizeComponent();
            shapeSize.size = newSize;
            ComponentHandler.sizes[(int) id] = shapeSize;

            //Mise à jour du tag "traversable"
            if(newSize < manager.Config.minSize)
            {
                manager.UpdateShapeColor(id, Color.green);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
                IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                ComponentHandler.traversables.Add(id, noCollisionTag);
            }
        }


        //Collisions écran-cercle
        ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
        {
            //Récupération des données utiles sur l'entité courante
            uint id = entity.id;
            float radius = ComponentHandler.sizes[(int) id].size / 2.0f;
            float initialSize = ComponentHandler.initialSizes[(int) id].initialSize;
            Vector2 position = ComponentHandler.positions[(int) id].position;
            Vector2 speed = ComponentHandler.speeds[(int) id].speed;

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
                    ComponentHandler.speeds[(int) id] = shapeSpeed;

                    speed = nextSpeed;

                    //Réinitialisation de la taille de l'entité
                    manager.UpdateShapeSize(id, initialSize);  //Pas nécessaire d'update l'affichage 5 fois dans la même frame
                    SizeComponent shapeSize = new SizeComponent();
                    shapeSize.size = initialSize;
                    ComponentHandler.sizes[(int) id] = shapeSize;

                    //Réinitialisation du type de l'entité
                    if(initialSize >= manager.Config.minSize)
                    {
                        manager.UpdateShapeColor(id, Color.blue);
                        if(ComponentHandler.traversables.ContainsKey(id))
                        {
                            ComponentHandler.traversables.Remove(id);
                        }
                    }
                }
            }
        });


        //Mise à jour des historiques (1 seule fois par frame)
        ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
        {
            //Récupération des données utiles sur l'entité courante
            uint id = entity.id;
            float size = ComponentHandler.sizes[(int) id].size;
            Vector2 position = ComponentHandler.positions[(int) id].position;
            Vector2 speed = ComponentHandler.speeds[(int) id].speed;

            ComponentHandler.positionHistories[(int) id].positionHistory.AddLast(position);
            if(ComponentHandler.positionHistories[(int) id].positionHistory.Count > 2*ComponentHandler.maxFramesPerSec)
            {
                ComponentHandler.positionHistories[(int) id].positionHistory.RemoveFirst();
            }

            ComponentHandler.sizeHistories[(int) id].sizeHistory.AddLast(size);
            if(ComponentHandler.sizeHistories[(int) id].sizeHistory.Count > 2*ComponentHandler.maxFramesPerSec)
            {
                ComponentHandler.sizeHistories[(int) id].sizeHistory.RemoveFirst();
            }

            ComponentHandler.speedHistories[(int) id].speedHistory.AddLast(speed);
            if(ComponentHandler.speedHistories[(int) id].speedHistory.Count > 2*ComponentHandler.maxFramesPerSec)
            {
                ComponentHandler.speedHistories[(int) id].speedHistory.RemoveFirst();
            }
        });
    }
}