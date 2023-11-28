using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Todoist.Net;
using Todoist.Net.Models;

namespace OrdersExtractor.API
{
    internal class TodoistAPI
    {
        private readonly TodoistClient todoist;

        internal TodoistAPI(string token)
        {
            todoist = new TodoistClient(token);
        }

        internal async Task TestAuth()
        {
            try
            {
                // check if token workable
                IEnumerable<Project> projects = await todoist.Projects.GetAsync();
                if (projects.Count() == 0)
                    throw new System.Exception();
            }
            catch
            {
                throw new System.Exception("Bad token / no projects / limit exceeded");
            }

        }

        internal async Task<Project> GetProject(string name)
        {
            IEnumerable<Project> projects = await todoist.Projects.GetAsync();
            Project requiredProject = projects.FirstOrDefault(p => p.Name == name);
            if (requiredProject == null)
                throw new System.Exception($"Can't find required project ({name})");

            return requiredProject;
        }

        internal async Task<bool> AddTask(ComplexId? toProjectID, string taskName, string taskDescription)
        {
            if (toProjectID != null && taskName != "" && taskDescription != "")
            {
                Item newTask = new Item(taskName, toProjectID.Value)
                {
                    Description = taskDescription
                };

                await todoist.Items.AddAsync(newTask);
                return true;
            }
            else
                return false;
        }
    }
}
