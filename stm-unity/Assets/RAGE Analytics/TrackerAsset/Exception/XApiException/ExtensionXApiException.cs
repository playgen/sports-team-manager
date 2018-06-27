using System;

namespace TrackerAssetPackage.Exceptions
{
    public class ExtensionXApiException : XApiException{
        public ExtensionXApiException(string message) : base(message){
        }
    }
}