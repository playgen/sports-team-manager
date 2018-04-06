/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
*/
namespace RAGE.EvaluationAsset
{
    using System;
    using System.Collections.Generic;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class EvaluationAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private EvaluationAssetSettings settings = null;

        /// <summary>
        /// Instance of the class EvaluationAssetAsset - Singelton pattern
        /// </summary>
        static readonly EvaluationAsset instance = new EvaluationAsset();

        /// <summary>
        /// Instance of the class EvaluationAssetHandler
        /// </summary>
        static internal EvaluationAssetHandler evaluationAssetHandler = new EvaluationAssetHandler();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the A_Evaluation.Asset class.
        /// </summary>
        private EvaluationAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            settings = new EvaluationAssetSettings();
        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as EvaluationAssetSettings);
            }
        }


        /// <summary>
        /// Getter for Instance of the EvaluationAsset - Singelton pattern
        /// </summary>
        public static EvaluationAsset Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Getter for Instance of the EvaluationAssetHandler 
        /// </summary>
        internal static EvaluationAssetHandler Handler
        {
            get
            {
                return evaluationAssetHandler;
            }
        }

        #endregion Properties
        #region Methods

        /// <summary>
        /// Method for sending data to the evaluation server
        /// </summary>
        /// <param name="gameEvent"> Type of event </param>
        /// <param name="parameter"> Event information </param>
        public void sensorData(String gameEvent, String parameter)
        {
            Handler.sensorData(this.settings.GameId, this.settings.GameVersion, this.settings.PlayerId , gameEvent, parameter, this.settings.Language);
        }


        /// <summary>
        /// Method returning the Asset settings.
        /// </summary>
        /// <returns> Settings of the Asset. </returns>
        internal EvaluationAssetSettings getEASettings()
        {
            return this.settings;
        }

        /// <summary>
        /// Query if this object issue request 2.
        /// </summary>
        ///
        /// <param name="method">   The method. </param>
        /// <param name="uri">      URI of the document. </param>
        /// <param name="headers">  The headers. </param>
        /// <param name="body">     (Optional) The body. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        internal RequestResponse IssueRequest(string method, Uri uri, Dictionary<string, string> headers, string body = "")
        {
            IWebServiceRequest ds = getInterface<IWebServiceRequest>();

            RequestResponse response = new RequestResponse();

            if (ds != null)
            {
                ds.WebServiceRequest(
                   new RequestSetttings
                   {
                       method = method,
                       uri = uri,
                       requestHeaders = headers,
                       //! allowedResponsCodes,     // TODO default is ok
                       body = body,
                   }, out response);
            }

            return response;
        }

        /// <summary>
        /// Query if this object issue request 2.
        /// </summary>
        ///
        /// <param name="host">     The host. </param>
        /// <param name="path">     Full pathname of the file. </param>
        /// <param name="headers">  The headers. </param>
        /// <param name="method">   (Optional) The method. </param>
        /// <param name="body">     (Optional) The body. </param>
        /// <param name="secure">   (Optional) true to secure. </param>
        /// <param name="port">     (Optional) The port. </param>
        ///
        /// <returns>
        /// true if it succeeds, false if it fails.
        /// </returns>
        internal RequestResponse IssueRequest(
            string host,
            string path,
            Dictionary<string, string> headers,
            string method = "GET",
            string body = "",
            Boolean secure = false,
            Int32 port = 80)
        {
            IWebServiceRequest ds = getInterface<IWebServiceRequest>();

            RequestResponse response = new RequestResponse();

            if (ds != null)
            {
                ds.WebServiceRequest(
                   new RequestSetttings
                   {
                       method = method,
                       uri = new Uri(string.Format("http{0}://{1}{2}/{3}",
                                   secure ? "s" : String.Empty,
                                   host,
                                   port == 80 ? String.Empty : String.Format(":{0}", port),
                                   path.TrimStart('/')
                                   )),
                       requestHeaders = headers,
                       //! allowedResponsCodes,     // TODO default is ok
                       body = body, // or method.Equals("GET")?string.Empty:body
                   }, out response);
            }

            return response;
        }

        #endregion Methods
        #region internal Methods

        /// <summary>
        /// Wrapper method for getting the getInterface method of the base Asset
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>Corresponding Interface</returns>
        internal T getInterfaceFromAsset<T>()
        {
            return this.getInterface<T>();
        }

        #endregion internal Methods
    }
}