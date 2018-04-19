using System;

namespace WebAPI.Exceptions
{
    /*--------------------------------------------------------------------------------------------------
     That exception is thrown a user tries to authenticate in the app with an empty or invalid code
    --------------------------------------------------------------------------------------------------*/
    public class InvalidCode : Exception
    {
        int code = 102;
        string msg = "Code is invalid";
    }
}