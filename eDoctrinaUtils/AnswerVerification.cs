using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaUtils
{
    [Serializable]
    public class AnswerVerification
    {
        //------------------------------------------------------------------------------------------
        public Data data { get; set; }
        public Answers[] answers { get; set; }
        public Coordinate coordinates;
        public AuditAnswerVerification audit;
        public Img image;

        //------------------------------------------------------------------------------------------
        public class Data
        {
            public int district_id ;

            public int test_id { get; set; }
            public int student_id { get; set; }
            public string student_uid { get; set; }
            public string amout_of_questions { get; set; }
            public string index_of_first_question { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Img
        {
            public string hash { get; set; }
            public string data { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct AuditAnswerVerification
        {
            public string processedAt { get; set; }
            public string processingTime { get; set; }
            public string serverName { get; set; }
            public string processingType { get; set; }
            public string dataFileName { get; set; }
            public string sourceFileHash { get; set; }
            public int sourceFilePageNo { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Answers
        {
            public int idx { get; set; }
            public Answer[] answers { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Answer
        {
            public int row { get; set; }
            public string[] cols { get; set; }
        }
        //------------------------------------------------------------------------------------------
       
        public struct Coordinate
        {
            public QuestionBubble[] questionBubbles { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct QuestionBubble
        {
            public int idx { get; set; }
            public Row[] answers { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Col
        {
            public int posx { get; set; }
            public int[] cols { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Bubble
        {
            public int pos { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
        //------------------------------------------------------------------------------------------
        public struct Row
        {
            public int row { get; set; }
            public Bubble[] cols { get; set; }
        }
        //------------------------------------------------------------------------------------------

    }
}
