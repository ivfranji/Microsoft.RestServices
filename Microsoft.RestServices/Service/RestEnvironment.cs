﻿namespace Microsoft.RestServices.Exchange
{
    using System;
    using Service;

    /// <summary>
    /// Represents rest environment to connect to.
    /// </summary>
    public class RestEnvironment
    {
        /// <summary>
        /// Graph beta.
        /// </summary>
        private static RestEnvironment graphBeta = new RestEnvironment(
            new Uri("https://graph.microsoft.com/beta"),
            "Graph Beta", 
            true);

        /// <summary>
        /// Outlook prod.
        /// </summary>
        private static RestEnvironment outlookProd = new RestEnvironment(
            new Uri("https://outlook.office365.com/api/v2.0"),
            "Outlook Prod",
            false);

        /// <summary>
        /// Outlook prod.
        /// </summary>
        private static RestEnvironment outlookBeta = new RestEnvironment(
            new Uri("https://outlook.office365.com/api/beta"),
            "Outlook Beta",
            true);

        public RestEnvironment(Uri baseUri, string name, bool isBeta)
        {
            this.BaseUri = baseUri;
            this.Name = name;
            this.IsBeta = isBeta;
        }

        /// <summary>
        /// Base uri.
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// Environment name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Indicate if environment is beta.
        /// </summary>
        public bool IsBeta { get; }

        /// <summary>
        /// Graph beta environment.
        /// </summary>
        public static RestEnvironment GraphBeta
        {
            get { return RestEnvironment.graphBeta; }
        }

        /// <summary>
        /// Outlook Prod environment.
        /// </summary>
        public static RestEnvironment OutlookProd
        {
            get
            {
                return RestEnvironment.outlookProd;
            }
        }

        /// <summary>
        /// Outlook Beta environment.
        /// </summary>
        public static RestEnvironment OutlookBeta
        {
            get
            {
                return RestEnvironment.outlookBeta;
            }
        }
    }
}