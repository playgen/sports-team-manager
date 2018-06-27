using System;

namespace TrackerAssetPackage.Exceptions
{
    public class VerbXApiException : XApiException{
        public VerbXApiException(string message) : base(message){
        }
    }
}