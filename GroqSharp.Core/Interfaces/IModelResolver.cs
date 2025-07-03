namespace GroqSharp.Core.Interfaces
{
    public interface IModelResolver
    {
        string GetModelFor(string command);
    }
}
