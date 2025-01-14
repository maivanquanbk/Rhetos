﻿/*
    Copyright (C) 2014 Omega software d.o.o.

    This file is part of Rhetos.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Rhetos
{
    public static class RhetosServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Rhetos components to the host application.
        /// </summary>
        /// <remarks>
        /// The following public Rhetos components are registered to <see cref="IServiceCollection"/>:
        ///   <list type="bullet">
        ///     <item><description><see cref="IRhetosComponent{T}"/>, allows direct access to Rhetos components from host application's services.</description></item>
        ///     <item><description><see cref="RhetosHost"/>, for internal access to IoC container.</description></item>
        ///   </list>
        /// </remarks>
        /// <returns>
        /// A <see cref="RhetosServiceCollectionBuilder"/> that can be used to add additional Rhetos-specific services to <see cref="IServiceCollection"/>.
        /// </returns>
        public static RhetosServiceCollectionBuilder AddRhetosHost(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, IRhetosHostBuilder> configureRhetosHost = null)
        {
            var builder = new RhetosServiceCollectionBuilder(serviceCollection);

            builder.Services.AddOptions();

            if (configureRhetosHost != null)
                builder.ConfigureRhetosHost(configureRhetosHost);

            builder.Services.TryAddSingleton(serviceProvider => CreateRhetosHost(serviceProvider));
            builder.Services.TryAddScoped<RhetosScopeServiceProvider>();
            // IRhetosComponent is registered as a transient component but the value of
            // IRhetosComponent will retain its scope that is specified in the Autofac IoC container
            builder.Services.TryAddTransient(typeof(IRhetosComponent<>), typeof(RhetosComponent<>));

            return builder;
        }

        private static RhetosHost CreateRhetosHost(IServiceProvider serviceProvider)
        {
            var rhetosHostBuilder = new RhetosHostBuilder();

            var options = serviceProvider.GetRequiredService<IOptions<RhetosHostBuilderOptions>>();

            foreach (var rhetosHostBuilderConfigureAction in options.Value.ConfigureActions)
            {
                rhetosHostBuilderConfigureAction(serviceProvider, rhetosHostBuilder);
            }

            return rhetosHostBuilder.Build();
        }
    }
}
