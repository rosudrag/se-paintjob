using DryIoc;
using Shared.Logging;

namespace ClientPlugin
{
    public class IoC
    {
        private static Container _container;

        public static Container Init(IPluginLogger pluginLogger)
        {
            _container = new Container();
            Register(_container, pluginLogger);
            return _container;
        }
        private static void Register(IRegistrator r, IPluginLogger pluginLogger)
        {
            r.RegisterDelegate(typeof(IPluginLogger), _ => pluginLogger);
            r.Register<IPaintApp, PaintApp>(Reuse.Singleton);
            r.Register<ICommandInterpreter, CommandInterpreter>(Reuse.Singleton);
            r.Register<IPaintJobHelpSystem, PaintJobHelpSystem>(Reuse.Singleton);
            r.Register<IPaintJobStateSystem, PaintJobStateSystem>(Reuse.Singleton);
            r.Register<IPaintJob, PaintJob>(Reuse.Singleton);
        }
        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}