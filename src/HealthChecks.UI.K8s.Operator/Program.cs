﻿using k8s;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.K8s.Operator.Controller;
using HealthChecks.UI.K8s.Operator.Handlers;


namespace HealthChecks.UI.K8s.Operator
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = InitializeProvider();

            var @operator = provider.GetRequiredService<IKubernetesOperator>();
            
            var cancelTokenSource = new CancellationTokenSource();

            await @operator.RunAsync(cancelTokenSource.Token);

            var reset = new ManualResetEventSlim();
            Console.CancelKeyPress += (s, a) => reset.Set();

            reset.Wait();
            @operator.Dispose();
        }

        private static IServiceProvider InitializeProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(sp => GetKubernetesClient());
            services.AddTransient<IKubernetesOperator, HealthChecksOperator>();
            services.AddTransient<IHealthChecksController, HealthChecksController>();
            services.AddSingleton<DeploymentHandler>();
            services.AddSingleton<ServiceHandler>();
            services.AddSingleton<HealthCheckServiceWatcher>();

            return services.BuildServiceProvider();
        }

        private static IKubernetes GetKubernetesClient()
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            return new Kubernetes(config);
        }
    }
}

