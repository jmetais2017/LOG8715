using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Effectue l'initialisation de tous les sytèmes et composants.
// Accès : tous les composants (une fois).
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
        // Si on a déjà créé la liste des entités, on n'a pas à ré-entrer dans ce système.
        if (ComponentHandler.ComponentExists<EntityComponent>()){
            return;
        }
        ECSManager manager = ECSManager.Instance;
        uint id = 1;


        //Initialisation du cooldown
        float frameTime = Time.time;


        //Création des conteneurs de components

        //ID
        ComponentHandler.RegisterComponentType<EntityComponent>(new Dictionary<uint, EntityComponent>());

        //Transform
        ComponentHandler.RegisterComponentType<PositionComponent>(new Dictionary<uint, PositionComponent>());
        ComponentHandler.RegisterComponentType<SizeComponent>(new Dictionary<uint, SizeComponent>());
        ComponentHandler.RegisterComponentType<SpeedComponent>(new Dictionary<uint, SpeedComponent>());

        //Initial sizes
        ComponentHandler.RegisterComponentType<InitialSizeComponent>(new Dictionary<uint, InitialSizeComponent>());

        //Histories
        ComponentHandler.RegisterComponentType<PositionHistoryComponent>(new Dictionary<uint, PositionHistoryComponent>());
        ComponentHandler.RegisterComponentType<SizeHistoryComponent>(new Dictionary<uint, SizeHistoryComponent>());
        ComponentHandler.RegisterComponentType<SpeedHistoryComponent>(new Dictionary<uint, SpeedHistoryComponent>());

        //Tags
        ComponentHandler.RegisterComponentType<IsTraversableComponent>(new Dictionary<uint, IsTraversableComponent>());
        ComponentHandler.RegisterComponentType<IsOnTopHalfComponent>(new Dictionary<uint, IsOnTopHalfComponent>());

        // Création de l'entité "settings" 
        EntityComponent settingsEntity = new EntityComponent();
        settingsEntity.id = 0;
        ComponentHandler.SetComponent<EntityComponent>(0, settingsEntity);

        //Settings
        FrameTimesHistoryComponent timeHistory = new FrameTimesHistoryComponent();
        LinkedList<float> frameTimes = new LinkedList<float>();
        timeHistory.frameTimesHistory = frameTimes;
        Dictionary<uint, FrameTimesHistoryComponent> timeHistoryDico = new Dictionary<uint, FrameTimesHistoryComponent>();
        timeHistoryDico.Add(0, timeHistory);
        ComponentHandler.RegisterComponentType<FrameTimesHistoryComponent>(timeHistoryDico);

        MaxFramesPerSecComponent maxFrameRate = new MaxFramesPerSecComponent();
        int maxFramesPerSec = 100;
        maxFrameRate.maxFramesPerSec = maxFramesPerSec;
        Dictionary<uint, MaxFramesPerSecComponent> maxFrameRateDico = new Dictionary<uint, MaxFramesPerSecComponent>();
        maxFrameRateDico.Add(0, maxFrameRate);
        ComponentHandler.RegisterComponentType<MaxFramesPerSecComponent>(maxFrameRateDico);

        LastRollBackComponent lastRollBack = new LastRollBackComponent();
        float firstTimer = 0.0f;
        lastRollBack.lastRollBack = firstTimer;
        Dictionary<uint, LastRollBackComponent> lastRollBackDico = new Dictionary<uint, LastRollBackComponent>();
        lastRollBackDico.Add(0, lastRollBack);
        ComponentHandler.RegisterComponentType<LastRollBackComponent>(lastRollBackDico);




        //Pour chaque cercle prédéfini
        manager.Config.allShapesToSpawn.ForEach(delegate(Config.ShapeConfig shape)
        {
            //Création de l'entité (id)
            manager.CreateShape(id, shape);
            EntityComponent entity = new EntityComponent();
            entity.id = id;
            ComponentHandler.SetComponent<EntityComponent>(id, entity);

            //Création de la composante "position"
            manager.UpdateShapePosition(id, shape.initialPos);
            PositionComponent shapePos = new PositionComponent();
            shapePos.position = shape.initialPos;
            ComponentHandler.SetComponent<PositionComponent>(id, shapePos);
            
            Vector3 shapeWorldPos = new Vector3(shape.initialPos[0], shape.initialPos[1], 0.0f);
            Vector3 shapeScreenPos = Camera.main.WorldToScreenPoint(shapeWorldPos);

            if(shapeScreenPos.y >= Screen.height / 2.0f){
                IsOnTopHalfComponent upperHalfTag = new IsOnTopHalfComponent();
                ComponentHandler.SetComponent<IsOnTopHalfComponent>(id, upperHalfTag);
            }


            //Création des composantes "size" et "initialSize"
            manager.UpdateShapeSize(id, shape.size);

            SizeComponent shapeSize = new SizeComponent();
            InitialSizeComponent initialShapeSize = new InitialSizeComponent();

            shapeSize.size = shape.size;
            initialShapeSize.initialSize = shape.size;

            ComponentHandler.SetComponent<SizeComponent>(id, shapeSize);
            ComponentHandler.SetComponent<InitialSizeComponent>(id, initialShapeSize);


            //Création de la composante "speed"
            SpeedComponent shapeSpeed = new SpeedComponent();
            shapeSpeed.speed = shape.initialSpeed;
            ComponentHandler.SetComponent<SpeedComponent>(id, shapeSpeed);


            //Initialisation des historiques
            PositionHistoryComponent shapePosHistory = new PositionHistoryComponent();
            shapePosHistory.positionHistory = new LinkedList<Vector2>();
            shapePosHistory.positionHistory.AddLast(shape.initialPos);
            ComponentHandler.SetComponent<PositionHistoryComponent>(id, shapePosHistory);

            SizeHistoryComponent shapeSizeHistory = new SizeHistoryComponent();
            shapeSizeHistory.sizeHistory = new LinkedList<float>();
            shapeSizeHistory.sizeHistory.AddLast(shape.size);
            ComponentHandler.SetComponent<SizeHistoryComponent>(id, shapeSizeHistory);

            SpeedHistoryComponent shapeSpeedHistory = new SpeedHistoryComponent();
            shapeSpeedHistory.speedHistory = new LinkedList<Vector2>();
            shapeSpeedHistory.speedHistory.AddLast(shape.initialSpeed);
            ComponentHandler.SetComponent<SpeedHistoryComponent>(id, shapeSpeedHistory);


            //Choix du type, et donc de la couleur
            float p = Random.Range(0.0f, 1.0f);
            if(p < 0.25f)
            {
                //Le cercle créé est statique
                manager.UpdateShapeColor(id, Color.red);
                SpeedComponent newSpeed = new SpeedComponent();
                newSpeed.speed = new Vector2(0.0f, 0.0f);
                ComponentHandler.SetComponent<SpeedComponent>(id, newSpeed);
            }
            else
            {
                //Le cercle créé est dynamique
                if(shape.size < manager.Config.minSize)
                {
                    //Sans collisions
                    manager.UpdateShapeColor(id, Color.green);
                    IsTraversableComponent noCollisionTag = new IsTraversableComponent();
                    ComponentHandler.SetComponent<IsTraversableComponent>(id, noCollisionTag);
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
        //manager.Config.systemsEnabled["creation"] = false;
    }
}
