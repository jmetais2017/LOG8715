using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enregistre le temps à chaque frame, mais en n'en conservant que 2 fois le nombre max de fps. Ainsi,
// quoi qu'il arrive, la liste des temps enregistrés recouvre au moins 2 secondes.

// Accès : FrameTimesHistoryComponent (lecture, écriture)
public class FrameTimeSystem : ISystem
{
    public string Name 
    { 
        get
        {
            return "frameTimes";
        }
    }


    // Update is called once per frame
    public void UpdateSystem()
    {
        float frameTime = Time.time;

        //Sauvegarde du timecode de la frame courante
        ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.AddLast(frameTime);

        //Ne pas garder plus de timecodes que nécessaire
        if(ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.Count > 2*ComponentHandler.GetComponent<MaxFramesPerSecComponent>(0).maxFramesPerSec){
            ComponentHandler.GetComponent<FrameTimesHistoryComponent>(0).frameTimesHistory.RemoveFirst();
        }
    }
}
