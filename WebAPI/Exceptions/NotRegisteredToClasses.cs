using System;

namespace WebAPI.Exceptions
{
    /*----------------------------------------------------------------------------------------------------------------------
     That exception is thrown when the app tries to retrieve classes info for a student who is not registered for any class
    ----------------------------------------------------------------------------------------------------------------------*/
    public class NotRegisteredToClasses : Exception
    {
        int code = 103;
        string msg = "You are not registered to any class";
    }
}