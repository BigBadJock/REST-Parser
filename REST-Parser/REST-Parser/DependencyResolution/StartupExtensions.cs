using Microsoft.Extensions.DependencyInjection;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.DependencyResolution
{
    public static class StartupExtensions
    {
        public static void RegisterRestParser<T>(this IServiceCollection services)
        {
            services.AddSingleton<IBooleanExpressionGenerator<T>, BooleanExpressionGenerator<T>>();
            services.AddSingleton<IDateExpressionGenerator<T>, DateExpressionGenerator<T>>();
            services.AddSingleton<IDecimalExpressionGenerator<T>, DecimalExpressionGenerator<T>>();
            services.AddSingleton<IDoubleExpressionGenerator<T>, DoubleExpressionGenerator<T>>();
            services.AddSingleton<IIntExpressionGenerator<T>, IntExpressionGenerator<T>>();
            services.AddSingleton<IStringExpressionGenerator<T>, StringExpressionGenerator<T>>();
            services.AddSingleton<IGuidExpressionGenerator<T>, GuidExpressionGenerator<T>>();
            services.AddSingleton<IRestToLinqParser<T>, RestToLinqParser<T>>();
        }
    }
}

