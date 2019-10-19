using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TradeRobotApp.Data;

namespace TradeRobotApp.Service.Changes
{
    public class ChangesFacory: IChangesFacory
    {
        private readonly Dictionary<string, IChanges> instances;

        public ChangesFacory(IHttpClientFactory httpClientFactory, IServiceProvider services)
        {
            instances = new Dictionary<string, IChanges>();

            using (IServiceScope serviceScope = services.CreateScope())
            using (ApplicationDbContext dbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
            {
                foreach (Data.Models.Changes changes in dbContext.Changeses)
                {
                    Type type = Type.GetType($"TradeRobotApp.Service.Changes.{changes.Name}");
                    IChanges instance = (IChanges)Activator.CreateInstance(type, httpClientFactory, changes);
                    instances.Add(changes.Name, instance);
                }
            }
        }

        public IChanges GetChangesInstance(string name)
        {
            Debug.Assert(instances.ContainsKey(name));
            return instances[name];
        }
    }
}
