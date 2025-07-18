using PaintJob.App.Systems;
using Sandbox.ModAPI;

namespace PaintJob.App
{
    public class PaintApp : IPaintApp
    {
        private readonly ICommandInterpreter _interpreter;

        public PaintApp(ICommandInterpreter interpreter)
        {
            _interpreter = interpreter;
        }


        public void CustomUpdate()
        {
            // Update code here. It is called on every simulation frame!
        }
        public void Save()
        {
            // No longer needed
        }
        
        public void Initialize()
        {
            MyAPIGateway.Utilities.MessageEnteredSender += PaintCommand;
            MyAPIGateway.Utilities.ShowMessage("Logger", "Paint plugin initialized");
        }
        private void PaintCommand(ulong sender, string messageText, ref bool sendToOthers)
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