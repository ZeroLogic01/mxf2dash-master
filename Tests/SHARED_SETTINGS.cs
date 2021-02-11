using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Commons;
using Newtonsoft.Json;
using System.IO;

namespace Tests
{
    [TestClass]
    public class SHARED_SETTINGS
    {
        private string PathRoot = @"..\..\Files\Repo\Configurations\SharedSettings\";

        private SharedSettings buildSS(string filename)
        {
            SharedSettings ss = SharedSettings.Create(filename);

            return ss;
        }

        [TestMethod]
        public void READ_CORRECT_FILE()
        {
            string filename = PathRoot + "correct.json";

            buildSS(filename);
          
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void READ_WRONG_WATCHFOLDER_NOT_EXISTS()
        {
            string filename = PathRoot + "watchfolder_not_exists.json";

            buildSS(filename);
        }
        
        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void READ_WRONG_WATCHFOLDER_NOT_STRING()
        {
            string filename = PathRoot + "watchfolder_not_string.json";

            buildSS(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(DirectoryNotFoundException))]
        public void READ_WRONG_OUTPUTFOLDER_NOT_EXISTS()
        {
            string filename = PathRoot + "outputFolder_not_exists.json";

            buildSS(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void READ_WRONG_OUTPUTFOLDER_NOT_STRING()
        {
            string filename = PathRoot + "outputFolder_not_string.json";

            buildSS(filename);
        }


        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void READ_WRONG_DELAY_NOT_INT()
        {
            string filename = PathRoot + "delay_not_int.json";

            buildSS(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void READ_WRONG_DELAY_OVERFLOW()
        {
            string filename = PathRoot + "delay_overflow.json";

            buildSS(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void READ_WRONG_DELAY_NEGATIVE()
        {
            string filename = PathRoot + "delay_negative.json";

            buildSS(filename);
        }
       
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void READ_WRONG_CONN_PORT_INVALID_RANGE_NEGATIVE()
        {
            string filename = PathRoot + "negative_port.json";

            buildSS(filename);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void READ_WRONG_CONN_PORT_INVALID_RANGE_LARGE()
        {
            string filename = PathRoot + "large_port.json";

            buildSS(filename);
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void READ_WRONG_CONN_PORT_NOT_INT()
        {
            string filename = PathRoot + "port_not_int.json";

            buildSS(filename);
        }
    }
}
