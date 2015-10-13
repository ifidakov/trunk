using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaUtils
{
    //-------------------------------------------------------------------------
    public enum StatusMessage
    {
        NULL, Delete, Next, Verify, ChangeAmoutOfQuestions, ChangeIndexOfFirstQuestion
    }
    //-------------------------------------------------------------------------
    public enum MarkersEvents
    {
        NULL
      ,
        Markers_not_found
            ,
        Sheet_identifier_is_not_recognized
          ,
        Sheet_identifier_not_found
          ,
        Unsupported_answer_sheet
          , Empty_scan
    }
    //-------------------------------------------------------------------------
    public enum RecognizeAction
    {
        NULL, Created, InProcess,
        WaitingForUserResponse, Cancelled,
        SearchMarkersFinished, SearchBarcodesFinished, SearchBublesFinished,
        RecAll, SearchMarkers, SearchBarcodes, SearchBubles, Stopped, Grid
    }
    //-------------------------------------------------------------------------
}
