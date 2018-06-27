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

using PlayGen.SUGAR.Unity;
using PlayGen.Unity.Utilities.Localization;

namespace RAGE.EvaluationAsset
{
    using AssetPackage;
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// An asset settings.
    /// 
    /// BaseSettings contains the (de-)serialization methods.
    /// </summary>
    public class EvaluationAssetSettings : BaseSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the A_Evaluation.AssetSettings class.
        /// </summary>
        public EvaluationAssetSettings()
            : base()
        {
            // Set Default values here.
            PostUrl = "http://css-kti.tugraz.at/evaluationasset/rest/sensordatapost";
			GameId = "SportsTeamManager";
            GameVersion = "1.0";
            PlayerId = SUGARManager.CurrentUser?.Name;
            Language = Localization.SelectedLanguage.TwoLetterISOLanguageName;
		}

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the Url for the post request.
        /// </summary>
        ///
        /// <value>
        /// The Url property for the post request.
        /// </value>
        [XmlElement()]
        public String PostUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Game Id for the xml creation.
        /// </summary>
        ///
        /// <value>
        /// The Game Id for the xml creation.
        /// </value>
        [XmlElement()]
        public String GameId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Game Version for the xml creation.
        /// </summary>
        ///
        /// <value>
        /// The Game Version for the xml creation.
        /// </value>
        [XmlElement()]
        public String GameVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Player Id for the xml creation.
        /// </summary>
        ///
        /// <value>
        /// The Player Id for the xml creation.
        /// </value>
        [XmlElement()]
        public String PlayerId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the language for the xml creation.
        /// </summary>
        ///
        /// <value>
        /// The Language for the xml creation.
        /// </value>
        [XmlElement()]
        public String Language
        {
            get;
            set;
        }



        #endregion Properties
    }
}
