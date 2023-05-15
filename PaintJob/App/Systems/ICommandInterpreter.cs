namespace PaintJob.App.Systems
{
    public interface ICommandInterpreter
    {
        void Interpret(string[] args);
        string[] GetCommands();
    }
}