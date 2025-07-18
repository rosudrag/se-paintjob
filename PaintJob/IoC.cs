using PaintJob.App;
using PaintJob.App.Systems;
using PaintJob.Shared.Logging;

namespace PaintJob
{
    public class IoC
    {
        public static void Init(IPluginLogger pluginLogger)
        {
            SimpleIoC.Register(pluginLogger);
            
            var helpSystem = new PaintJobHelpSystem();
            SimpleIoC.Register<IPaintJobHelpSystem, PaintJobHelpSystem>(helpSystem);
            
            var paintJob = new PaintJob.App.PaintJob();
            SimpleIoC.Register<IPaintJob, PaintJob.App.PaintJob>(paintJob);
            
            var commandInterpreter = new CommandInterpreter(helpSystem, paintJob);
            SimpleIoC.Register<ICommandInterpreter, CommandInterpreter>(commandInterpreter);
            
            var paintApp = new PaintApp(commandInterpreter);
            SimpleIoC.Register<IPaintApp, PaintApp>(paintApp);

        }
        
        public static T Resolve<T>() where T : class
        {
            return SimpleIoC.Resolve<T>();
        }
    }
}