using System.Collections.Generic;

public class RegisterSystems
{
    public static List<ISystem> GetListOfSystems()
    {
        // determine order of systems to add
        List<ISystem> toRegister = new List<ISystem>();

        // AJOUTEZ VOS SYSTEMS ICI
        CreationSystem createSyst = new CreationSystem();
        MoveSystem moveSyst = new MoveSystem();
        FrameTimeSystem frameTimeSyst = new FrameTimeSystem();
        RollBackSystem rollBackSyst = new RollBackSystem();
        TurboSystem turboSyst = new TurboSystem();

        toRegister.Add(createSyst);
        toRegister.Add(moveSyst);
        toRegister.Add(turboSyst);
        toRegister.Add(frameTimeSyst);
        toRegister.Add(rollBackSyst);

        return toRegister;
    }
}