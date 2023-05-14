namespace ClientPlugin
{
    public interface ICommandInterpreter
    {
        void Interpret(string[] args);
        string[] GetCommands();
    }
}