using System.Collections.Generic;
using UnityEngine;

/*
   DO NOT CHANGE, THIS WILL BE REPLACED CORRECTION TIME
   NE PAS CHANGER, CE FICHIER VA ETRE REMPLACER A LA CORRECTION
*/
public class ECSManager : MonoBehaviour
{
    #region Private attributes
    [SerializeField]
    private Config _config;

    [SerializeField]
    private GameObject _circlePrefab;

    private Dictionary<uint, GameObject> _gameObjectsForDisplay = new Dictionary<uint, GameObject>();
    #endregion

    #region Singleton
    private static ECSManager _instance;
    public static ECSManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ECSManager>();
                if (_instance == null)
                {
                    Debug.LogError("Can't find instance in scene!!");
                }
            }
            return _instance;
        }
    }
    private ECSManager() { }
    #endregion

    #region Public API
    public Config Config { get { return _config; } }

    public List<ISystem> AllSystems { get { return _allSystems; } }

    public void CreateShape(uint id, Config.ShapeConfig entityConfig)
    {
        GameObject instance = Instantiate(_circlePrefab);
        instance.transform.localScale *= entityConfig.size;
        _gameObjectsForDisplay[id] = instance;
    }

    public void DestroyShape(uint id)
    {
        Destroy(_gameObjectsForDisplay[id]);
    }

    public void UpdateShapePosition(uint id, Vector2 position)
    {
        _gameObjectsForDisplay[id].transform.position = position;
    }

    public void UpdateShapeSize(uint id, float size)
    {
        _gameObjectsForDisplay[id].transform.localScale = Vector2.one * size;
    }

    public void UpdateShapeColor(uint id, Color color)
    {
        _gameObjectsForDisplay[id].GetComponent<SpriteRenderer>().color = color;
    }
    #endregion

    #region System Management
    private List<ISystem> _allSystems = new List<ISystem>();

    private void Awake()
    {
        _allSystems = RegisterSystems.GetListOfSystems();

        // If system missing from config, add it and enable it.
        foreach (var system in _allSystems)
        {
            if (!Config.systemsEnabled.ContainsKey(system.Name))
            {
                Config.systemsEnabled[system.Name] = true;
            }
        }
    }
    // Update is called once per frame
    private void Update()
    {
        foreach (var system in _allSystems)
        {
            if (Config.systemsEnabled[system.Name])
            {
                system.UpdateSystem();
            }
        }
    }
    #endregion
}

public interface ISystem
{
    void UpdateSystem();
    string Name { get; }
}

public interface IComponent
{

}

public class EntityComponent : IComponent
{
    public uint id;
}