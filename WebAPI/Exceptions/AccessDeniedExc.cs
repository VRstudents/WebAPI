using System;

namespace WebAPI.Exceptions
{
    /*--------------------------------------------------------------------------------------------------
     That exception is thrown when a token protected API is requested without a valid token in a header
    --------------------------------------------------------------------------------------------------*/
    public class AccessDeniedExc : Exception
    {
        int code = 101;
        string msg = "Access denied";
    }
}