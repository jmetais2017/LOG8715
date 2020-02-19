using System.Collections.Generic;
using UnityEngine;

public struct PositionHistoryComponent : IComponent 
{
    public LinkedList<Vector2> positionHistory;

}