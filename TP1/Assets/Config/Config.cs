using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
   NE PAS CHANGER, CE FICHIER VA ETRE REMPLACER A LA CORRECTION
*/
[CreateAssetMenu(fileName = "Config", menuName = "General Config", order = 1)]
public class Config : ScriptableObject {

    [Serializable]
    public struct ShapeConfig
    {
        public Vector2 initialPos;
        public float size;
        public Vector2 initialSpeed;
    }

    [SerializeField]
    public float minSize = 2;

    [SerializeField]
    public List<ShapeConfig> allShapesToSpawn;

    [SerializeField]
    [Tooltip("Key is the name of the system to enable")]
    private StringBoolDictionary _systemsEnabled = StringBoolDictionary.New<StringBoolDictionary>();
    public Dictionary<string, bool> systemsEnabled { get { return _systemsEnabled.dictionary; } }
}
