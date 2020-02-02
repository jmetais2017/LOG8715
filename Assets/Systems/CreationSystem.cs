using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationSystem : ISystem
{
    public string Name 
    { 
        get
        {
            return "creation";
        }
    }

    public void UpdateSystem()
    {
        ECSManager manager = ECSManager.Instance;
        uint id = 0;

        //Initialisation du cooldown
        float frameTime = Time.time;
        ComponentHandler.lastRollback = frameTime;


        //Pour chaque cercle prédéfini
        manager.Config.allShapesToSpawn.ForEach(delegate(Config.ShapeConfig shape)
        {
            //Création de l'entité (id)
            manager.CreateShape(id, shape);
            EntityComponent entity = new EntityComponent();
            entity.id = id;
            ComponentHandler.entities.Add(entity);

            //Création de la composante "position"
            manager.UpdateShapePosition(id, shape.initialPos);
            PositionComponent shapePos = new PositionComponent();
            shapePos.position = shape.initialPos;
            ComponentHandler.positions.Add(shapePos);
            
            Vector3 shapeWorldPos = new Vector3(shape.initialPos[0], shape.initialPos[1], 0.0f);
            Vector3 shapeScreenPos = Camera.main.WorldToScreenPoint(shapeWorldPos);

            if(shapeScreenPos.y >= Screen.height / 2.0f){
                IsOnTopHalfComponent upperHalfTag = new IsOnTopHalfComponent();
                ComponentHandler.upperHalf.Add(id, upperHalfTag);
            }

            //Création des composantes "size" et "initialSize"
            manager.UpdateShapeSize(id, shape.size);
            SizeComponent shapeSize = new SizeComponent();
            InitialSizeComponent initialShapeSize = new InitialSizeComponent();
            shapeSize.size = shape.size;
            initialShapeSize.initialSize = shape.size;
            ComponentHandler.sizes.Add(shapeSize);
            ComponentHandler.initialSizes.Add(initialShapeSize);

            //Création de la composante "speed"
            SpeedComponent shapeSpeed = new SpeedComponent();
            shapeSpeed.speed = shape.initialSpeed;
            ComponentHandler.speeds.Add(shapeSpeed);


            //Initialisation des historiques
            PositionHistoryComponent shapePosHistory = new PositionHistoryComponent();
            shapePosHistory.positionHistory = new LinkedList<Vector2>();
            shapePosHistory.positionHistory.AddLast(shape.initialPos);
            ComponentHandler.positionHistories.Add(shapePosHistory);

            SizeHistoryComponent shapeSizeHistory = new SizeHistoryComponent();
            shapeSizeHistory.sizeHistory = new LinkedList<float>();
            shapeSizeHistory.sizeHistory.AddLast(shape.size);
            ComponentHandler.sizeHistories.Add(shapeSizeHistory);

            SpeedHistoryComponent shapeSpeedHistory = new SpeedHistoryComponent();
            shapeSpeedHistory.speedHistory = new LinkedList<Vector2>();
            shapeSpeedHistory.speedHistory.AddLast(shape.initialSpeed);
            ComponentHandler.speedHistories.Add(shapeSpeedHistory);


            //Choix du type, et donc de la couleur
            float p = Random.Range(0.0f, 1.0f);
            if(p < 0.25f)
            {
                //Le cercle créé est statique
                manager.UpdateShapeColor(id, Color.red);
                SpeedComponent newSpeed = new SpeedComponent();
                newSpeed.speed = new Vector2(0.0f, 0.0f);
                ComponentHandler.speeds[(int) id] = newSpeed;
            }
            else
            {
                //Le cercle créé est dynamique
                if(shape.size < manager.Config.minSize)
                {
                    //Sans collisions
                    manager.UpdateShapeColor(id, Color.green);
                    IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                    ComponentHandler.traversables.Add(id, noCollisionTag);
                }
                else
                {
                    //Avec collisions
                    manager.UpdateShapeColor(id, Color.blue);
                }
            }

            id++;

        });

        //Désactivation du système après la première frame
        manager.Config.systemsEnabled["creation"] = false;
    }
}
