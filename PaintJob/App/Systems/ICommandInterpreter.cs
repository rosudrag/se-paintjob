namespace ClientPlugin.App
{
    public interface ICommandInterpreter
    {
        void Interpret(string[] args);
        string[] GetCommands();
    }
}