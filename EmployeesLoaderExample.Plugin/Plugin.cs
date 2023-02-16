using Newtonsoft.Json;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EmployeesLoaderExample.Plugin
{
    [Author(Name = "Andrew Petrov")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading employees");
            var employeesList = new List<EmployeesDTO>();
            //get employees from https://dummyjson.com/users
            string url = "https://dummyjson.com/users";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                string json = reader.ReadToEnd();
                //deserialize json
                dynamic result = JsonConvert.DeserializeObject<dynamic>(json);
                dynamic[] employees = result.users.ToObject<object[]>();
                //convert employee to EmployeesDTO
                foreach (var obj in employees)
                {
                    var emp = new EmployeesDTO { Name = $"{obj.firstName} {obj.lastName}" };
                    emp.AddPhone(obj.phone.ToString().Replace(" ", ""));
                    employeesList.Add(emp);
                }

            }
            catch (Exception ex)
            {

                logger.Error(ex.Message);
                logger.Trace(ex.StackTrace);
            }

            logger.Info($"Loaded {employeesList.Count()} employees");
            return employeesList.Cast<DataTransferObject>();
        }
    }
}
