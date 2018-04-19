using System;

namespace WebAPI.Exceptions
{
    /*---------------------------------------------------------------------------------------------------------------
     That exception is thrown when the app tries to retrieve lessons list for the class which has no lessons added
    ---------------------------------------------------------------------------------------------------------------*/
    public class NoLessonsInClass : Exception
    {
        int code = 104;
        string msg = "No lessons assingned to the class";
    }
}