using System;
using System.Collections.ObjectModel;

namespace eDoctrinaUtils
{
    public class BubbleEventArgs : EventArgs
    {
        public bool NoException;
        public RegionsArea[] Areas;
        public int AmoutOfQuestions;
        public int MaxAmoutOfQuestions;
        public int IndexOfFirstQuestion;
        public int[] LinesPerArea;
        public int[] BubblesPerLine;
        public ObservableCollection<BubbleItem> BubbleItems;
        public BubbleEventArgs(bool noException, ObservableCollection<BubbleItem> bubbleItems
            , RegionsArea[] areas, int amoutOfQuestions, int maxAmoutOfQuestions, int indexOfFirstQuestion, int[] linesPerArea, int[] bubblesPerLine)
        {
            NoException = noException;
            Areas = areas;
            AmoutOfQuestions = amoutOfQuestions;
            MaxAmoutOfQuestions = maxAmoutOfQuestions;
            IndexOfFirstQuestion = indexOfFirstQuestion;
            LinesPerArea = linesPerArea;
            BubblesPerLine = bubblesPerLine;
            BubbleItems = bubbleItems;
        }
    }
}
