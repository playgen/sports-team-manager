using System;

namespace TrackerAssetPackage.Exceptions
{
    public class ValueExtensionException : ExtensionException{
        public ValueExtensionException(string message) : base(message){
        }
    }
}