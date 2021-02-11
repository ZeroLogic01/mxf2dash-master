using Commons;
using Master;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class MASTER_SETTINGS
    {
        private string PathRoot = @"..\..\Files\Repo\Configurations\MasterSettings\";

        private void ChangeConfigFilename(string filename)
        {
            Settings.ConfigFilename = filename;
        }

        [TestMethod]
        public void READ_CORRECT_FILE()
        {
            string filename = PathRoot + "correct.json";

            ChangeConfigFilename(filename);

            _ = Settings.Instance;
        }

    }
}
