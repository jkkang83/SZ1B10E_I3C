using FZ4P.BarcodeReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P
{
    public static class Bootstrapper
    {
        public static ServiceProvider Build()
        {
            var services = new ServiceCollection();

            RegisterServices(services);
            RegisterMvc(services);

            var provider = services.BuildServiceProvider();
            var initializeForm = provider.GetRequiredService<F_Start>();
            initializeForm.Show();

            return provider;
        }

        private static void RegisterServices(IServiceCollection services)
        {

            //services.AddSingleton<Scanner>();
            //services.AddSingleton<IDevLogManager, DevLogManager>();
            //services.AddSingleton<IOperating, COperating>();
            //services.AddSingleton<IPlcProtocol, LsPlcProtocol>();
            //services.AddSingleton<ConditionWorkerManager>();
            //services.AddSingleton<DbControl>();

            //services.AddTransient<CashedRepositoryFactory>();
            //services.AddTransient<DbContextFactory>();

            //services.AddSingleton<TransientFormTracker>();
        }

        private static void RegisterMvc(IServiceCollection services)
        {
            services.AddSingleton<F_Main>();
            services.AddSingleton<F_Start>();
            //services.AddSingleton<RibbonFormBase>();
            //services.AddSingleton<RibbonFormBaseControl>();
            //services.AddSingleton<RibbonFormBaseModel>();

            //services.AddTransient<FormPlcCommunicationControl>();
            //services.AddTransient<FormPlcCommunication>();
            //services.AddTransient<FormPlcCommunicationModel>();

            //services.AddTransient<FormWorkerControl>();
            //services.AddTransient<FormWorker>();

            //services.AddTransient<NewFormWorkerModel>();
            //services.AddTransient<NewFormWorkerControl>();
            //services.AddTransient<NewFormWorker>();

            //services.AddSingleton<FormBase>();
        }
    }
}
