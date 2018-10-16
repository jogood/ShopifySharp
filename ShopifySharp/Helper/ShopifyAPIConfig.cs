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

        /// <summary>
        /// Make service without creating a <see cref="ShopifyAPIConfig"/> instance.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="APIConfigFile"></param>
        /// <returns></returns>
        public static S MakeService<S>(string APIConfigFile)
        {
            string[] config = File.ReadAllLines(APIConfigFile);
            string apikey = config[0];
            string password = config[1];
            string shopname = config[2];
            string baseurl = "https://" + apikey + ":" + password + "@" + shopname;
            return (S)Activator.CreateInstance(typeof(S), new object[] { baseurl, password });
        }
    }
}