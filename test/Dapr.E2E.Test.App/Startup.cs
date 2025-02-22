﻿// ------------------------------------------------------------------------
// Copyright 2021 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

namespace Dapr.E2E.Test
{
    using Dapr.E2E.Test.Actors.Reentrancy;
    using Dapr.E2E.Test.Actors.Reminders;
    using Dapr.E2E.Test.Actors.Timers;
    using Dapr.E2E.Test.Actors.ExceptionTesting;
    using Dapr.E2E.Test.App.ErrorTesting;
    using Dapr.Workflow;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;

    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures Services.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication().AddDapr();
            services.AddAuthorization(o => o.AddDapr());
            services.AddControllers().AddDapr();
            // Register a workflow and associated activity
            services.AddDaprWorkflow(options =>
            {
                // Example of registering a "PlaceOrder" workflow function
                options.RegisterWorkflow<string, string>("PlaceOrder", implementation: async (context, input) =>
                {
                    // In real life there are other steps related to placing an order, like reserving
                    // inventory and charging the customer credit card etc. But let's keep it simple ;)
                    return await context.CallActivityAsync<string>("ShipProduct", "Coffee Beans");
                });

                // Example of registering a "ShipProduct" workflow activity function
                options.RegisterActivity<string, string>("ShipProduct", implementation: (context, input) =>
                {
                    System.Threading.Thread.Sleep(10000); // sleep for 10s to allow the terminate command to come through
                    return Task.FromResult($"We are shipping {input} to the customer using our hoard of drones!");
                });
            });
            services.AddActors(options =>
            {
                options.Actors.RegisterActor<ReminderActor>();
                options.Actors.RegisterActor<TimerActor>();
                options.Actors.RegisterActor<Regression762Actor>();
                options.Actors.RegisterActor<ExceptionActor>();
            });
        }

        /// <summary>
        /// Configures Application Builder and WebHost environment.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="env">Webhost environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCloudEvents();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapActorsHandlers();
                endpoints.MapControllers();
            });
        }
    }
}
