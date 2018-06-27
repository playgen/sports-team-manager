using System;

namespace TrackerAssetPackage.Exceptions
{
    public class TrackerException : Exception{
        public TrackerException(string message) : base(message){
        }
    }
}