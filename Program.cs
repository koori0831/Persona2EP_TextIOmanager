using System;
using System.IO;

namespace Persona2EP_TextIOmanager
{
    internal class Program
    {
        public static string msgPath;
        public static string csvPath;

        [STAThread]
        static void Main(string[] args)
        {
            msgPath = null;
            csvPath = null;

            Console.WriteLine("msg 파일을 CSV 파일로 변환하거나 CSV 파일을 msg 파일로 변환하는 프로그램입니다.");

            while (true)
            {
                Console.WriteLine("\n작업을 선택하세요:");
                Console.WriteLine("1. msg 파일을 CSV 파일로 변환");
                Console.WriteLine("2. CSV 파일을 msg 파일로 변환");
                Console.WriteLine("3. 폴더의 모든 msg 파일을 CSV 파일로 변환");
                Console.WriteLine("4. 폴더의 모든 CSV 파일을 msg 파일로 변환");
                Console.WriteLine("5. 종료");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MsgToCsvConverter converter1 = new MsgToCsvConverter();

                        msgPath = Utilities.GetFilePathFromDialog("변환할 msg 파일 선택", "msg files (*.msg)|*.msg|All files (*.*)|*.*");
                        if (string.IsNullOrEmpty(msgPath)) continue;
                        csvPath = Utilities.GetSaveFilePathFromDialog("CSV 파일 저장 경로 선택", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
                        if (string.IsNullOrEmpty(csvPath)) continue;

                        converter1.ConvertMsgToCsv(msgPath, csvPath);
                        break;
                    case "2":
                        CsvToMsgConverter converter2 = new CsvToMsgConverter();

                        csvPath = Utilities.GetFilePathFromDialog("변환할 CSV 파일 선택", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
                        if (string.IsNullOrEmpty(csvPath)) continue;

                        msgPath = Utilities.GetSaveFilePathFromDialog("msg 파일 저장 경로 선택", "msg files (*.msg)|*.msg|All files (*.*)|*.*");
                        if (string.IsNullOrEmpty(msgPath)) continue;

                        converter2.ConvertCsvToMsg(msgPath, csvPath);
                        break;
                    case "3":
                        MsgToCsvConverter converter3 = new MsgToCsvConverter();
                        string msgFolderPath = Utilities.GetFolderPathFromDialog("msg 파일이 있는 폴더 선택");
                        if (string.IsNullOrEmpty(msgFolderPath)) continue;

                        string csvOutputFolderPath = Utilities.GetFolderPathFromDialog("CSV 파일을 저장할 폴더 선택");
                        if (string.IsNullOrEmpty(csvOutputFolderPath)) continue;

                        string[] msgFiles = Directory.GetFiles(msgFolderPath, "*.msg");
                        if (msgFiles.Length == 0)
                        {
                            Console.WriteLine("선택한 폴더에 msg 파일이 없습니다.");
                            continue;
                        }

                        Console.WriteLine($"총 {msgFiles.Length}개의 msg 파일을 변환합니다.");
                        foreach (string filePath in msgFiles)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                            string csvSavePath = Path.Combine(csvOutputFolderPath, fileNameWithoutExtension + ".csv");
                            converter3.ConvertMsgToCsv(filePath, csvSavePath);
                            Console.WriteLine($"{filePath} -> {csvSavePath} 변환 완료");
                        }
                        Console.WriteLine("폴더 내 모든 msg 파일 변환 완료.");
                        break;
                    case "4":
                        CsvToMsgConverter converter4 = new CsvToMsgConverter();
                        string csvFolderPath = Utilities.GetFolderPathFromDialog("CSV 파일이 있는 폴더 선택");
                        if (string.IsNullOrEmpty(csvFolderPath)) continue;

                        string msgOutputFolderPath = Utilities.GetFolderPathFromDialog("msg 파일을 저장할 폴더 선택");
                        if (string.IsNullOrEmpty(msgOutputFolderPath)) continue;

                        string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");
                        if (csvFiles.Length == 0)
                        {
                            Console.WriteLine("선택한 폴더에 CSV 파일이 없습니다.");
                            continue;
                        }

                        Console.WriteLine($"총 {csvFiles.Length}개의 CSV 파일을 변환합니다.");
                        foreach (string filePath in csvFiles)
                        {
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                            string msgSavePath = Path.Combine(msgOutputFolderPath, fileNameWithoutExtension + ".msg");
                            converter4.ConvertCsvToMsg(msgSavePath, filePath);
                            Console.WriteLine($"{filePath} -> {msgSavePath} 변환 완료");
                        }
                        Console.WriteLine("폴더 내 모든 CSV 파일 변환 완료.");
                        break;
                    case "5":
                        Console.WriteLine("프로그램을 종료합니다.");
                        return;
                    default:
                        Console.WriteLine("잘못된 선택입니다. 다시 시도하세요.");
                        break;
                }
            }
        }
    }
}