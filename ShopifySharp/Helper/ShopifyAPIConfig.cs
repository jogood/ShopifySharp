using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShopifySharp
{
    public class ShopifyAPIConfig
    {
        public string apikey;
        public string password;
        public string shopname;
        public string BaseUrl;

        public ShopifyAPIConfig(string APIConfigFile)
        {

            string[] config = File.ReadAllLines(APIConfigFile);
            apikey = config[0];
            password = config[1];
            shopname = config[2];
            BaseUrl = "https://" + apikey + ":" + password + "@" + shopname;

        }

        /// <summary>
        /// Use config to return the required service. 
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <returns></returns>
        public S MakeService<S>()
        {
            return (S)Activator.CreateInstance(typeof(S), new object[] { BaseUrl, password });
        }
    }
}