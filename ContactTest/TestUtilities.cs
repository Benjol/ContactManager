using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ContactTest
{
    ////http://stackoverflow.com/questions/113395/how-can-i-test-for-an-expected-exception-with-a-specific-exception-message-from-a
    //public static class ExceptionAssert
    //{
    //    public static T Throws<T>(Action action) where T : Exception
    //    {
    //        try
    //        {
    //            action();
    //        }
    //        catch(T ex)
    //        {
    //            return ex;
    //        }
    //        Assert.Fail("Exception of type {0} should be thrown.", typeof(T));

    //        //  The compiler doesn't know that Assert.Fail
    //        //  will always throw an exception
    //        return null;
    //    }
    //}

    public static class TestUtilities
    {
        internal static bool ArraysEqual(Array a1, Array a2)
        {
            if(a1 == a2)
                return true;

            if(a1 == null || a2 == null)
                return false;

            if(a1.Length != a2.Length)
                return false;

            IList list1 = a1, list2 = a2;

            for(int i = 0; i < a1.Length; i++)
                if(!Object.Equals(list1[i], list2[i]))
                    return false;
            return true;
        }
    }
}
