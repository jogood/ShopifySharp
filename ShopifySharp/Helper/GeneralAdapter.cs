using System;
using System.Collections.Generic;
using System.Text;

namespace ShopifySharp.Helper
{
    public static class GeneralAdapter
    {
        /// <summary>
        /// PLace your own string output method here
        /// </summary>
        public static Action<string> OuputString = Console.WriteLine;

    }
}
