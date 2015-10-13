using System;
using System.Runtime.InteropServices;

namespace eDoctrinaUtils
{
    /// <summary>
    /// Summary description for RecycleBin.
    /// </summary>
    public class RecycleBin
    {
        /// <summary>
        /// Перечисление FileOperationType для функции определяет, что делать с файлом. 
        /// </summary>
        public enum FileOperationType : uint
        {
            /// <summary>
            ///Переместить файл            
            /// <summary>
            FO_MOVE = 0x0001,
            /// <summary>
            /// Копировать файл 
            /// </summary>
            FO_COPY = 0x0002,
            /// <summary>
            /// Удалить (в корзину или безвозвратно) файл 
            /// </summary>
            FO_DELETE = 0x0003,
            /// <summary>
            /// Переименовать файл 
            /// </summary>
            FO_RENAME = 0x0004,
        }

        /// <summary>
        /// Необходимые флаги для функции SHFileOperation. 
        /// </summary>
        [Flags]
        public enum FileOperationFlags : ushort
        {
            /// <summary>
            /// Не показывать диалог с индикатором прогресса в течение процесса удаления.
            /// </summary>
            FOF_SILENT = 0x0004,
            /// <summary>
            /// Не спрашивать у пользователя подтверждения удаления.
            /// </summary>
            FOF_NOCONFIRMATION = 0x0010,
            /// <summary>
            /// Удалить файл в корзину. Этот флаг нужен для того, чтобы файл был удален именно в корзину. 
            /// </summary>
            FOF_ALLOWUNDO = 0x0040,
            /// <summary>
            /// Не показывать, какие файлы и\или папки удаляются, в диалоге с индикатором прогресса.
            /// </summary>
            FOF_SIMPLEPROGRESS = 0x0100,
            /// <summary>
            /// Не показывать сообщения об ошибках, которые могут возникнуть в течение процесса.
            /// </summary>
            FOF_NOERRORUI = 0x0400,
            /// <summary>
            /// Предупреждать, что удаляемые файлы слишком велики для помещения в корзину и поэтому 
            /// будут удалены безвозвратно.
            /// </summary>
            FOF_WANTNUKEWARNING = 0x4000,
        }

        /// <summary>
        /// SHFILEOPSTRUCT для функции. Здесь два объявления самой функции и этой структуры, 
        /// используемые в зависимости от разрядности системы 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        private struct SHFILEOPSTRUCT_x86
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT_x64
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        //Собственно, само объявление функции 
        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
        private static extern int SHFileOperation_x86(ref SHFILEOPSTRUCT_x86 FileOp);

        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
        private static extern int SHFileOperation_x64(ref SHFILEOPSTRUCT_x64 FileOp);

        //Эта функция определяет, 32-битная или 64-битная система 
        private static bool IsWOW64Process()
        {
            return IntPtr.Size == 8;
        }

        /// <summary>
        /// Эта функция отправляет файл или папку в корзину 
        /// </summary>
        /// <param name="path">Полное имя файла или папки, которую нужно удалить</param>
        /// <param name="flags"> FileOperationFlags в дополнение к флагу FOF_ALLOWUNDO,FOF_SILENT,FOF_NOCONFIRMATION который уже задан</param>
        public static void DeleteToRecycleBin(string path, FileOperationFlags flags)
        {
            int result;
            bool operationsAborted;
            //если 64-битная система 
            if (IsWOW64Process())
            {
                //создаем новую структуру 
                SHFILEOPSTRUCT_x64 fs = new SHFILEOPSTRUCT_x64();
                //указываем, что файл будет удаляться 
                fs.wFunc = FileOperationType.FO_DELETE;
                //Непонятно, почему, но если функции передать структуру, в которой указан "нормальный" 
                //путь, то произойдет ошибка 
                fs.pFrom = path + '\0' + '\0';
                //установка флагов 
                fs.fFlags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | flags;
                //выполнение функции 
                result = SHFileOperation_x64(ref fs);
                operationsAborted = fs.fAnyOperationsAborted;
            }
            else
            {
                //система 32-битная. Здесь все так же, как для 64-битной системы, только структура и 
                //функция уже 32-битные
                SHFILEOPSTRUCT_x86 fs = new SHFILEOPSTRUCT_x86();
                fs.wFunc = FileOperationType.FO_DELETE;
                fs.pFrom = path + '\0' + '\0';
                fs.fFlags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | flags;//без подтверждения пользователя(ushort)(0x0004 | 0x0040 | 0x0010);
                result = SHFileOperation_x86(ref fs);
                operationsAborted = fs.fAnyOperationsAborted;
            }
            if (result != 0)
            {
                // произошла ошибка
                Console.WriteLine("Delete to RecycleBin Error");
                throw new Exception("Delete to RecycleBin Error (result = " + result + ")");
            }
            else
            {
                if (operationsAborted)
                {
                    // операция была прервана
                    Console.WriteLine("Remove operation aborted");
                }
            }
        }
    }
}