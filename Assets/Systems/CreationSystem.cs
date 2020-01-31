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

    // Update is called once per frame
    public void UpdateSystem()
    {
        ECSManager manager = ECSManager.Instance;
        uint id = 0;


        manager.Config.allShapesToSpawn.ForEach(delegate(Config.ShapeConfig shape)
        {
            manager.CreateShape(id, shape);
            manager.UpdateShapePosition(id, shape.initialPos);
            manager.UpdateShapeSize(id, shape.size);

            float p = Random.Range(0.0f, 1.0f);
            if(p < 0.25f)
            {
                //Le cercle créé est statique
                manager.UpdateShapeColor(id, Color.red);
            }
            else
            {
                //Le cercle créé est dynamique
                if(shape.size < manager.Config.minSize)
                {
                    //Sans collisions
                    manager.UpdateShapeColor(id, Color.green);
                }
                else
                {
                    //Avec collisions
                    manager.UpdateShapeColor(id, Color.blue);
                }
            }

            EntityComponent entity = new EntityComponent();
            entity.id = id;

            id++;
        });


        // World.ForEach(delegate(EntityComponent entity)
        // {
        //     Debug.log(entity.id);
        // });


        manager.Config.systemsEnabled["creation"] = false;
    }
}
