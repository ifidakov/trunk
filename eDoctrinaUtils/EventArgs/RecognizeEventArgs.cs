using System;

namespace eDoctrinaUtils
{
    public class RecognizeEventArgs : EventArgs
    {
        public MarkersEvents markersEvents = MarkersEvents.NULL;
        public RecognizeEventArgs()
        { }
        public RecognizeEventArgs(string barCodesPrompt)
        {
            switch (barCodesPrompt)
            {
                case "Markers not found ":
                    markersEvents = MarkersEvents.Markers_not_found;
                    break;
                case "Sheet identifier is not recognized":
                    markersEvents = MarkersEvents.Sheet_identifier_is_not_recognized;
                    break;
                case "Sheet identifier not found":
                    markersEvents = MarkersEvents.Sheet_identifier_not_found;
                    break;
                case "Unsupported answer sheet ":
                    markersEvents = MarkersEvents.Unsupported_answer_sheet;
                    break;
                case "Empty scan":
                    markersEvents = MarkersEvents.Empty_scan;
                    break;
            }
        }
    }
}
