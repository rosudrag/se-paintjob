using Sandbox.ModAPI;

namespace ClientPlugin.App
{
    public class PaintApp : IPaintApp
    {
        private readonly ICommandInterpreter _interpreter;
        private readonly IPaintJobStateSystem _stateSystem;

        public PaintApp(ICommandInterpreter interpreter, IPaintJobStateSystem stateSystem)
        {
            _interpreter = interpreter;
            _stateSystem = stateSystem;
        }

        public void Initialize()
        {
            MyAPIGateway.Utilities.MessageEntered += PaintCommand;
            MyAPIGateway.Utilities.ShowMessage("Logger", "Paint plugin initialized");
            _stateSystem.Load();
        }
        public void CustomUpdate()
        {
            // Update code here. It is called on every simulation frame!
        }
        public void Save()
        {
            _stateSystem.Save();
        }

        private void PaintCommand(string messageText, ref bool sendToOthers)
        {
            if (messageText.StartsWith("/paint"))
            {
                sendToOthers = false;
                var args = messageText.Split(' ');
                _interpreter.Interpret(args);
            }
        }
    }

}