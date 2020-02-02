using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentHandler
{
    //Liste des id utilisés
    public static List<EntityComponent> entities = new List<EntityComponent>();

    //Transform
    public static List<PositionComponent> positions = new List<PositionComponent>();
    public static List<SizeComponent> sizes = new List<SizeComponent>();
    public static List<SpeedComponent> speeds = new List<SpeedComponent>();

    public static List<InitialSizeComponent> initialSizes = new List<InitialSizeComponent>();

    //Historiques pour le retour en arrière
    public static List<PositionHistoryComponent> positionHistories = new List<PositionHistoryComponent>();
    public static List<SizeHistoryComponent> sizeHistories = new List<SizeHistoryComponent>();
    public static List<SpeedHistoryComponent> speedHistories = new List<SpeedHistoryComponent>();

    public static LinkedList<float> frameTimesHistory = new LinkedList<float>();

    //Tags
    public static Dictionary<uint, IsTraversableComponent> traversables = new Dictionary<uint, IsTraversableComponent>();
    public static Dictionary<uint, IsOnTopHalfComponent> upperHalf = new Dictionary<uint, IsOnTopHalfComponent>();

    //Constantes de la simulation
    public static int maxFramesPerSec = 100;

    public static float lastRollback = 0.0f;
}
