using System;

namespace TrackerAssetPackage.Exceptions
{
    public class TargetXApiException : XApiException{
        public TargetXApiException(string message) : base(message){
        }
    }
}