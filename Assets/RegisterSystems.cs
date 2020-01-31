using System.Collections.Generic;

public class RegisterSystems
{
    public static List<ISystem> GetListOfSystems()
    {
        // determine order of systems to add
        List<ISystem> toRegister = new List<ISystem>();

        // AJOUTEZ VOS SYSTEMS ICI
        CreationSystem firstSystem = new CreationSystem();
        toRegister.Add(firstSystem);

        return toRegister;
    }
}