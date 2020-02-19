public class CustomArray<T> where T: IComponent
{
    private T[] elements {get; set;}

    public CustomArray(int size) 
    {
        elements = new T[size];
    }



}