using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


public static class ComponentHandler
{
    //Conteneur principal : un dictionnaire dans lequel le conteneur de chaque type de components a pour clé ce type
    private static Dictionary<Type, Dictionary<uint, IComponent>> components = new Dictionary<Type, Dictionary<uint, IComponent>>();


    public static bool ComponentExists<T>() where T : IComponent
    {
        return components.ContainsKey(typeof(T));
    }

    //Renvoie le conteneur de tous les components du type spécifié
    public static Dictionary<uint, T> GetComponentsOfType<T>() where T : IComponent
    {
        Dictionary<uint, T> dico = new Dictionary<uint, T>();
        foreach(KeyValuePair<uint, IComponent> keyvaluepair in components[typeof(T)])
        {
            dico.Add(keyvaluepair.Key, (T) keyvaluepair.Value);
        }
        return dico;
    }

    //Renvoie le component du type spécifié de l'entité id
    public static T GetComponent<T>(uint id)
    {
        return (T) components[typeof(T)][id];
    }

    //Ajoute une entrée au conteneur principal, et donc un type de component reconnu
    public static void RegisterComponentType<T>(Dictionary<uint, T> compDico) where T : IComponent
    {
        Dictionary<uint, IComponent> dico = new Dictionary<uint, IComponent>();
        foreach(KeyValuePair<uint, T> keyvaluepair in compDico)
        {
            dico.Add(keyvaluepair.Key, (IComponent) keyvaluepair.Value);
        }
        components[typeof(T)] = dico;;
    }

    //Affecte la valeur du component du type spécifié de l'entité id
    public static void SetComponent<T>(uint id, IComponent comp)
    {
        components[typeof(T)][id] = comp;
    }

    //Supprime le component du type spécifié de l'entité id
    public static void RemoveComponent<T>(uint id)
    {
        components[typeof(T)].Remove(id);
    }
}
