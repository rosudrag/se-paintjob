using Sandbox.ModAPI;

namespace ClientPlugin
{
    public class PaintApp : IPaintApp
    {
        private readonly ICommandInterpreter _interpreter;

        public PaintApp(ICommandInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void Initialize()
        {
            MyAPIGateway.Utilities.MessageEntered += PaintCommand;
            MyAPIGateway.Utilities.ShowMessage("Logger", "Paint plugin initialized");
        }
        public void CustomUpdate()
        {
            // Update code here. It is called on every simulation frame!
        }

        private void PaintCommand(string messageText, ref bool sendToOthers)
        {
            sendToOthers = false;
            var args = messageText.Split(' ');
            _interpreter.Interpret(args);
        }
    }

}