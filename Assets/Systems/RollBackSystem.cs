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


        //Test sur les tags "isOnTopHalf"

        // foreach( KeyValuePair<uint, IsOnTopHalfComponent> kvp in ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>())
        // {
        //     manager.UpdateShapeColor(kvp.Key, Color.yellow);
        // }
        // foreach(KeyValuePair<uint, EntityComponent> keyvaluepair in ComponentHandler.GetComponentsOfType<EntityComponent>())
        // {
        //     uint id = keyvaluepair.Value.id;
        //     if(!ComponentHandler.GetComponentsOfType<IsOnTopHalfComponent>().ContainsKey(id))
        //     {
        //         manager.UpdateShapeColor(id, Color.black);
        //     }
        // }



        float frameTime = Time.time;

        //Activation par appui sur la barre d'espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(frameTime - ComponentHandler.GetComponent<LastRollBackComponent>(0).lastRollBack > 2.0f)
            {
                //Si plus de 2 secondes se sont écoulées depuis la dernière activation, on effectue le roll back

                //Récupération des données à utiliser pour le roll back
                float currentHistoryTime = ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.First.Value;
                while(currentHistoryTime < frameTime - 2.0f){
                    ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.RemoveFirst();

                    //On vide l'historique de chaque entité jusqu'à arriver à 2 secondes dans le passé
                    foreach(KeyValuePair<uint, EntityComponent> keyvaluepair in ComponentHandler.GetComponentsOfType<EntityComponent>())
                    {
                        uint id = keyvaluepair.Value.id;

                        ComponentHandler.GetComponent<PositionHistoryComponent>(id).positionHistory.RemoveFirst();
                        ComponentHandler.GetComponent<SizeHistoryComponent>(id).sizeHistory.RemoveFirst();
                        ComponentHandler.GetComponent<SpeedHistoryComponent>(id).speedHistory.RemoveFirst();
                    }

                    currentHistoryTime = ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.First.Value;
                }

                //Réinitialisation des components
                foreach(KeyValuePair<uint, EntityComponent> keyvaluepair in ComponentHandler.GetComponentsOfType<EntityComponent>())
                {
                    uint id = keyvaluepair.Value.id;

                    //Position
                    Vector2 newPos = ComponentHandler.GetComponent<PositionHistoryComponent>(id).positionHistory.First.Value;

                    manager.UpdateShapePosition(id, newPos);
                    PositionComponent shapePos = new PositionComponent();
                    shapePos.position = newPos;
                    ComponentHandler.SetComponent<PositionComponent>(id, shapePos);

                    //Size
                    float newSize = ComponentHandler.GetComponent<SizeHistoryComponent>(id).sizeHistory.First.Value;

                    manager.UpdateShapeSize(id, newSize);
                    SizeComponent shapeSize = new SizeComponent();
                    shapeSize.size = newSize;
                    ComponentHandler.SetComponent<SizeComponent>(id, shapeSize);

                    //Speed
                    Vector2 newSpeed = ComponentHandler.GetComponent<SpeedHistoryComponent>(id).speedHistory.First.Value;

                    SpeedComponent shapeSpeed = new SpeedComponent();
                    shapeSpeed.speed = newSpeed;
                    ComponentHandler.SetComponent<SpeedComponent>(id, shapeSpeed);

                    //Collision tag
                    if(newSpeed != Vector2.zero)
                    {
                        if(newSize < manager.Config.minSize)
                        {
                            //Sans collisions
                            manager.UpdateShapeColor(id, Color.green);
                            if(!ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(id))
                            {
                                IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                                ComponentHandler.SetComponent<IsTraversableComponent>(id, noCollisionTag);
                            }
                        }
                        else
                        {
                            //Avec collisions
                            manager.UpdateShapeColor(id, Color.blue);
                            if(ComponentHandler.GetComponentsOfType<IsTraversableComponent>().ContainsKey(id))
                            {
                                ComponentHandler.RemoveComponent<IsTraversableComponent>(id);
                            }
                        }
                    }

                    //Upper half tag
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


                //Actualisation du cooldown
                LastRollBackComponent lastRollBack = new LastRollBackComponent();
                lastRollBack.lastRollBack = frameTime;
                ComponentHandler.SetComponent<LastRollBackComponent>(0, lastRollBack);

                Debug.Log("Retour en arrière de 2 secondes !");
            }
            else
            {
                //Sinon, on affiche l'état du cooldown
                Debug.Log("Cooldown restant : ");
                Debug.Log(2.0f - (frameTime - ComponentHandler.GetComponent<LastRollBackComponent>(0).lastRollBack));
            }
        }
    }
}

