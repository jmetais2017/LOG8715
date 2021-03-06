﻿//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using UnityEngine;
using System.Collections.Generic;

#if BAD_PERF
using InnerType = System.Collections.Generic.Dictionary<uint, IComponent>;
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<uint, IComponent>>;
#else
//using InnerType = IComponent[]; // Nous n'avons pas utilisé InnerType, utiliser un tableau natif de C#
// était suffisant pour ce que nous en faisons.
using AllComponents = System.Collections.Generic.Dictionary<uint, IComponent[]>; 
#endif

// Appeler GetHashCode sur un Type est couteux. Cette classe sert a precalculer le hashcode
public static class TypeRegistry<T> where T : IComponent
{
    public static uint typeID = (uint)Mathf.Abs(default(T).GetRandomNumber()) % ComponentsManager.maxEntities;
}

public class Singleton<V> where V : new()
{
    private static bool isInitiated = false;
    private static V _instance;
    public static V Instance
    {
        get
        {
            if (!isInitiated)
            {
                isInitiated = true;
                _instance = new V();
            }
            return _instance;
        }
    }
    protected Singleton() { }
}

internal class ComponentsManager : Singleton<ComponentsManager>
{
    private AllComponents _allComponents = new AllComponents();

    public const int maxEntities = 2000;

    private int nbEntities = ECSManager.Instance.Config.numberOfShapesToSpawn;

    public void DebugPrint()
    {
        string toPrint = "";
        var allComponents = Instance.DebugGetAllComponents();
        foreach (var type in allComponents)
        {
            toPrint += $"{type}: \n";
#if !BAD_PERF
            for(int i = 0; i < type.Value.Length; i++)
            {
                var component = type.Value[i]; 
#else
            foreach (var component in type.Value)
            {
#endif
#if BAD_PERF
                toPrint += $"\t{component.Key}: {component.Value}\n";
#else
                toPrint += $"\t{component}: {component}\n";
#endif
            }
            toPrint += "\n";
        }
        Debug.Log(toPrint);
    }

    // CRUD
    public void SetComponent<T>(EntityComponent entityID, IComponent component) where T : IComponent
    {
        // Ancienne version
       /*  if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            //_allComponents[TypeRegistry<T>.typeID] = new Dictionary<uint, IComponent>();
            _allComponents[TypeRegistry<T>.typeID] = new InnerType();
        }
        _allComponents[TypeRegistry<T>.typeID][entityID] = component; */   

        // Nouvelle version 
        if(!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            // Si le composant n'existe pas : création d'un tableau de taille nbEntities, 
            // de type IComponent, et ajout de ce tableau au dictionnaire allComponents. Ce
            // tableau est initialisé avec la valeur par défaut de IComponent, i.e. null
            _allComponents[TypeRegistry<T>.typeID] = new IComponent[nbEntities];
        }

        // Ajout du component fourni au bon endroit
        _allComponents[TypeRegistry<T>.typeID][entityID.id] = component;

    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        //_allComponents[TypeRegistry<T>.typeID].Remove(entityID);
        // Mise à null de la case correspondant au type et à l'entité fournie
        _allComponents[TypeRegistry<T>.typeID][entityID.id] = null;
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        //return (T)_allComponents[TypeRegistry<T>.typeID][entityID];

        // Contrairement à avant, on a besoin d'un entier comme indice de la case à accéder,
        // il faut donc extraire l'id de l'entité
        return (T)_allComponents[TypeRegistry<T>.typeID][entityID.id];
    }
    public bool TryGetComponent<T>(EntityComponent entityID, out T component) where T : IComponent
    {
       /*  if (_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            if (_allComponents[TypeRegistry<T>.typeID].ContainsKey(entityID))
            {
                component = (T)_allComponents[TypeRegistry<T>.typeID][entityID];
                return true;
            }
        }
        component = default;
        return false; */
        
        // Très semblable à la version initiale, en remplaçant containsKey par un test null.
        // Comme la valeur par défaut est à null, et la valeur est mise à null lorsqu'on supprime
        // le component, le comportement est identique. 
        if (_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            if (_allComponents[TypeRegistry<T>.typeID][entityID.id] != null)
            {
                component = (T)_allComponents[TypeRegistry<T>.typeID][entityID.id];
                return true;
            }
        }
        component = default;
        return false;
    }

    public bool EntityContains<T>(EntityComponent entity) where T : IComponent
    {
        //return _allComponents[TypeRegistry<T>.typeID].ContainsKey(entity);
        return _allComponents[TypeRegistry<T>.typeID][entity.id] != null;
    }

    public void ClearComponents<T>() where T : IComponent
    {
        if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(TypeRegistry<T>.typeID, new IComponent[nbEntities]);
        }
        else
        {
           _allComponents[TypeRegistry<T>.typeID] = new IComponent[nbEntities];
        }
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {
        /*var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach (EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity))
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity]);
        }*/


        // Récupération des tableaux correspondant au type T1 et aux entityComponent
        IComponent[] arrayT1 = _allComponents[TypeRegistry<T1>.typeID];
        IComponent[] entityArray = _allComponents[TypeRegistry<EntityComponent>.typeID];

        // Itération sur tous ces tableaux (ils sont tous de même longueur)
        for(uint i = 0; i < arrayT1.Length; i++)
        {
            if (arrayT1[i] == null)
            {
                continue;
            }
            var entity = (EntityComponent) entityArray[i];
            lambda(entity, (T1) arrayT1[i]);
        }
    }

    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach(EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity)
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity]);
        } */

        // Récupération des tableaux
        IComponent[] arrayT1 = _allComponents[TypeRegistry<T1>.typeID];
        IComponent[] arrayT2 = _allComponents[TypeRegistry<T2>.typeID];
        
        IComponent[] entityArray = _allComponents[TypeRegistry<EntityComponent>.typeID];

        // Itération simultanée sur les deux tableaux (de même taille)
        for(uint i = 0; i < arrayT1.Length; i++)
        {
            if (arrayT1[i] == null || arrayT2[i] == null)
            {
                continue;
            }
            var entity = (EntityComponent) entityArray[i];
            lambda(entity, (T1) arrayT1[i], (T2) arrayT2[i]);
        }
    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach (EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T3>.typeID].ContainsKey(entity)
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity], (T3)_allComponents[TypeRegistry<T3>.typeID][entity]);
        } */

        // Récupération des tableaux
        IComponent[] arrayT1 = _allComponents[TypeRegistry<T1>.typeID];
        IComponent[] arrayT2 = _allComponents[TypeRegistry<T2>.typeID];
        IComponent[] arrayT3 = _allComponents[TypeRegistry<T3>.typeID];
        
        IComponent[] entityArray = _allComponents[TypeRegistry<EntityComponent>.typeID];

        // Itération simultanée sur les trois tableaux
        for(uint i = 0; i < arrayT1.Length; i++)
        {
            if (arrayT1[i] == null || arrayT2[i] == null || arrayT3[i] == null)
            {
                continue;
            }
            var entity = (EntityComponent) entityArray[i];
            lambda(entity, (T1) arrayT1[i], (T2) arrayT2[i], (T3) arrayT3[i]);
        }
    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach (EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T3>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T4>.typeID].ContainsKey(entity)
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity], (T3)_allComponents[TypeRegistry<T3>.typeID][entity], (T4)_allComponents[TypeRegistry<T4>.typeID][entity]);
        } */

        // Récupération des tableaux
        IComponent[] arrayT1 = _allComponents[TypeRegistry<T1>.typeID];
        IComponent[] arrayT2 = _allComponents[TypeRegistry<T2>.typeID];
        IComponent[] arrayT3 = _allComponents[TypeRegistry<T3>.typeID];
        IComponent[] arrayT4 = _allComponents[TypeRegistry<T4>.typeID];

        IComponent[] entityArray = _allComponents[TypeRegistry<EntityComponent>.typeID];

        // Itération simultanée sur les 4 tableaux
        for(uint i = 0; i < arrayT1.Length; i++)
        {
            if (arrayT1[i] == null || arrayT2[i] == null || arrayT3[i] == null || arrayT4[i] == null)
            {
                continue;
            }
            var entity = (EntityComponent) entityArray[i];
            lambda(entity, (T1) arrayT1[i], (T2) arrayT2[i], (T3) arrayT3[i], (T4) arrayT4[i]);
        }
    }

    public AllComponents DebugGetAllComponents()
    {
        return _allComponents;
    }
}
