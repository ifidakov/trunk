using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace eDoctrinaOcrTestWPF
{
    public class EtalonFileItem
    {
        //простая таблица - исходный хеш-хещ отдельного фрейма-результат (ok/failed)
        //все это по идее должно совпадать между запусками
        //ну или как вариант исходный хеш - номер страницы внутри тифа - корректный результат (распознан/нет)
        //для случая не распознан - с какой ошибкой не распознан

        public string SourcePage { get; set; }
        public string SourceSha1 { get; set; }

        public string DataSha1 { get; set; }
        public string FrameSha1 { get; set; }

        private string error;
        public string Error
        { 
            get { return error; }
            set { error = (String.IsNullOrEmpty(value)) ? "" : value; }
        }
        public string CorrectFileName { get; set; }
        public string AutorName { get; set; }
    }
}
