using System.Collections;
using System.Collections.Generic;

public struct FrameTimesHistoryComponent : IComponent 
{
    public LinkedList<float> frameTimesHistory;
}