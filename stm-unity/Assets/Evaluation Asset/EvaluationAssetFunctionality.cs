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

using AssetPackage;
using System;
using System.Collections.Generic;

namespace RAGE.EvaluationAsset
{
    internal class EvaluationAssetHandler
    {
        #region Fields
        #endregion Fields
        #region Constructors

        /// <summary>
        /// private EvaluationAssetHandler-ctor for Singelton-pattern 
        /// </summary>
        public EvaluationAssetHandler() { }

        #endregion Constructors
        #region Properties
        #endregion Properties
        #region Methods

        /// <summary>
        /// Method returning an instance of the EvaluationAssetHandler.
        /// </summary>
        /// <returns> Instance of the EvaluationAssetHandler </returns>
        internal EvaluationAsset getEA()
        {
            return EvaluationAsset.Instance;
        }

        /// <summary>
        /// Method for sending data to the evaluation server
        /// </summary>
        /// <param name="gameId"> Game identifier </param>
        /// <param name="playerId">Player Identifier </param>
        /// <param name="gameEvent"> Type of event </param>
        /// <param name="parameter"> Event information </param>
        /// <param name="gameversion"> version of the game </param>
        internal void sensorData(String gameId, String gameversion, String playerId, String gameEvent, String parameter, String language)
        {
            if (!isReceivedDataValid(gameId, playerId, gameEvent, parameter))
            {
                loggingEA("Received data(" + gameId + "/" + playerId + "/" + gameEvent + "/" + parameter + ") not valid, input ignored!");
                return;
            }
            else
                loggingEA("Reiceveid sensor data ("+gameId+"/"+playerId+"/"+gameEvent+"/"+parameter+").");

            String xmlString = buildXMLString(gameId, gameversion, playerId, gameEvent, parameter, language);
            loggingEA("Created xml string: \"" +xmlString+"\"." );

            postData(xmlString);
        }

        /// <summary>
        /// Method for converting input data into a xml string for the evaluation service
        /// </summary>
        /// <param name="gameId"> Game identifier </param>
        /// <param name="playerId">Player Identifier </param>
        /// <param name="gameEvent"> Type of event </param>
        /// <param name="parameter"> Event information </param>
        /// <param name="gameversion"> version of the game </param>
        /// <returns> A XML string representation of the data </returns>
        internal String buildXMLString(String gameId, String gameversion, String playerId, String gameEvent, String parameter, String language)
        {
            String xml = "<sensordata>";

            xml += "<context project = \"rage\" appid = \""+gameId+ "\" appversion=\""+gameversion+"\" applang = \""+ language + "\"/>";
            xml += "<actor id = \""+playerId+"\" group = \"\" ref= \"\"/>";
            xml += "<predicate tag = \""+gameEvent+"\"/>";

            String[] parameterPairs = parameter.Split('&');

            xml += "<valuedata ";
            foreach(String parameterPair in parameterPairs)
            {
                String[] currentParameterPair = parameterPair.Split('=');
                xml += currentParameterPair[0] + "=\""+ currentParameterPair[1] + "\" ";
            }
            xml += "/>";

            xml += "</sensordata>";
            return (xml);
        }

        /// <summary>
        /// Method for performing POST request for sending evaluation data.
        /// </summary>
        /// <param name="body"> data to be send to the evaluation service. </param>
        internal void postData(String body)
        {
            IWebServiceRequest iwr =  EvaluationAsset.Instance.getInterfaceFromAsset<IWebServiceRequest>();
            if (iwr != null)
            {
                loggingEA("performing POST request with evaluation data.");
                Uri uri = new Uri(getEA().getEASettings().PostUrl);
                Dictionary<string, string> headers = new Dictionary<string, string>();
                //headers.Add("user", playerId);

                RequestResponse response = getEA().IssueRequest("POST", uri, headers, body);
                

                if (response.ResultAllowed)
                {
                    loggingEA("WebClient request successful!");
                }
                else
                {
                    loggingEA("Web Request for sending evaluation data to " +
                        response.uri.ToString() + " failed! " + response.responsMessage, Severity.Error);

                    //throw new Exception("EXCEPTION: Web Request for sending evaluation data to " + response.uri.ToString() + " failed! " + response.responsMessage);
                }
            }
            else
            {
                loggingEA("IWebServiceRequest bridge absent for performing POST request for sending evaluation data.", Severity.Error);
                //throw new Exception("EXCEPTION: IWebServiceRequest bridge absent for performing POST request for sending evaluation data.");
            }
        }

        /// <summary>
        /// Method for checking input data format
        /// </summary>
        /// <param name="gameId"> Game identifier </param>
        /// <param name="playerId">Player Identifier </param>
        /// <param name="gameEvent"> Type of event </param>
        /// <param name="parameter"> Event information </param>
        /// <returns> True, if the received data is valid, false otherwise </returns>
        internal Boolean isReceivedDataValid(String gameId, String playerId, String gameEvent, String parameter)
        {
            string[] parameterPairs = parameter.Split('&');
            List<string> keys = new List<string>();

            foreach(string pair in parameterPairs)
                keys.Add(pair.Split('=')[0]);

            switch (gameEvent)
            {
                case "gameusage":
                    return (keys.Count == 1 && keys.Contains("event"));
                case "userprofile":
                    return (keys.Count == 1 && keys.Contains("event"));
                case "gameactivity":
                    return (keys.Count == 3 && keys.Contains("event") && keys.Contains("goalorientation") && keys.Contains("tool"));
                case "gamification":
                    return (keys.Count == 1 && keys.Contains("event"));
                case "gameflow":
                    return (keys.Count == 3 && keys.Contains("type") && keys.Contains("id") && keys.Contains("completed"));
                case "support":
                    return (keys.Count == 1 && keys.Contains("event"));
                case "assetactivity":
                    return (keys.Count == 2 && keys.Contains("asset") && keys.Contains("done"));
            }

            return (false);
        }

		#endregion MethodssensorData
		#region Testmethods

		/// <summary>
		/// Method for logging (Diagnostics).sensorData
		/// </summary>
		/// 
		/// <param name="msg"> Message to be logged. </param>
		internal void loggingEA(String msg, Severity severity = Severity.Information)
        {
            getEA().Log(severity, "[EA]: " + msg);
        }
        
        #endregion Testmethods
    }
    
}
