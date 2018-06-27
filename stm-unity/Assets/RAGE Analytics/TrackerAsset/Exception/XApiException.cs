using System;

namespace TrackerAssetPackage.Exceptions
{
    public class XApiException : TrackerException{

        public XApiException(string message) : base(message){
        }
    }
}