using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaOcrTestWPF
{
    public enum VerifyFiles 
    { 
        empty, 
        missingAudit, missingTiff, missingCsv,
        wrongDataSha1,
        error, missing, extra, ok
    }
}
