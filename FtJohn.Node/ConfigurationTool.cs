﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FtJohn.Node
{
    public class ConfigurationTool
    {
        public T GetAppSettings<T>(string key) where T : class, new()
        {
            IConfiguration config = new ConfigurationBuilder()
            .Add(new JsonConfigurationSource { Path = "AppSettings.json", ReloadOnChange = true })
            .Build();

            T appconfig = new ServiceCollection()
                .AddOptions()
                .Configure<T>(config.GetSection(key))
                .BuildServiceProvider()
                .GetService<IOptions<T>>()
                .Value;

            return appconfig;
        }
    }
}
