using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        ECSManager manager = ECSManager.Instance;

        // foreach( KeyValuePair<uint, IsOnTopHalfComponent> kvp in ComponentHandler.upperHalf )
        // {
        //     manager.UpdateShapeColor(kvp.Key, Color.yellow);
        // }
        // ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
        // {
        //     uint id = entity.id;
        //     if(!ComponentHandler.upperHalf.ContainsKey(id))
        //     {
        //         manager.UpdateShapeColor(id, Color.black);
        //     }
        // });



        float frameTime = Time.time;

        //Activation par appui sur la barre d'espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(frameTime - ComponentHandler.lastRollback > 2.0f)
            {
                //Si plus de 2 secondes se sont écoulées depuis la dernière activation, on effectue le roll back

                //Récupération des données à utiliser pour le roll back
                float currentHistoryTime = ComponentHandler.frameTimesHistory.First.Value;
                while(currentHistoryTime < frameTime - 2.0f){
                    ComponentHandler.frameTimesHistory.RemoveFirst();

                    //On vide l'historique de chaque entité jusqu'à arriver à 2 secondes dans le passé
                    ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
                    {
                        uint id = entity.id;

                        ComponentHandler.positionHistories[(int) id].positionHistory.RemoveFirst();
                        ComponentHandler.sizeHistories[(int) id].sizeHistory.RemoveFirst();
                        ComponentHandler.speedHistories[(int) id].speedHistory.RemoveFirst();
                    });

                    currentHistoryTime = ComponentHandler.frameTimesHistory.First.Value;
                }

                //Réinitialisation des components
                ComponentHandler.entities.ForEach(delegate(EntityComponent entity)
                {
                    uint id = entity.id;

                    //Position
                    Vector2 newPos = ComponentHandler.positionHistories[(int) id].positionHistory.First.Value;

                    manager.UpdateShapePosition(id, newPos);
                    PositionComponent shapePos = new PositionComponent();
                    shapePos.position = newPos;
                    ComponentHandler.positions[(int) id] = shapePos;

                    //Size
                    float newSize = ComponentHandler.sizeHistories[(int) id].sizeHistory.First.Value;

                    manager.UpdateShapeSize(id, newSize);
                    SizeComponent shapeSize = new SizeComponent();
                    shapeSize.size = newSize;
                    ComponentHandler.sizes[(int) id] = shapeSize;

                    //Speed
                    Vector2 newSpeed = ComponentHandler.speedHistories[(int) id].speedHistory.First.Value;

                    SpeedComponent shapeSpeed = new SpeedComponent();
                    shapeSpeed.speed = newSpeed;
                    ComponentHandler.speeds[(int) id] = shapeSpeed;

                    //Collision tag
                    if(newSpeed != Vector2.zero)
                    {
                        if(newSize < manager.Config.minSize)
                        {
                            //Sans collisions
                            manager.UpdateShapeColor(id, Color.green);
                            if(!ComponentHandler.traversables.ContainsKey(id))
                            {
                                IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                                ComponentHandler.traversables.Add(id, noCollisionTag);
                            }
                        }
                        else
                        {
                            //Avec collisions
                            manager.UpdateShapeColor(id, Color.blue);
                            if(ComponentHandler.traversables.ContainsKey(id))
                            {
                                ComponentHandler.traversables.Remove(id);
                            }
                        }
                    }

                    //Upper half tag
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

