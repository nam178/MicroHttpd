using Autofac;

namespace MicroHttpd.Core.TcpServer.IoC
{
    public class AutofacModule : global::Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TcpClientCounter>().SingleInstance().AsImplementedInterfaces();
        }
    }
}
