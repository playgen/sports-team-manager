using System;

namespace TrackerAssetPackage.Exceptions
{
    public class ActorXApiException : XApiException {
        public ActorXApiException(string message) : base(message){
        }
    }
}