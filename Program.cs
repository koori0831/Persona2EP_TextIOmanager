using System;

namespace Persona2EP_TextIOmanager
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("msg 파일을 CSV 파일로 변환하거나 CSV 파일을 msg 파일로 변환하는 프로그램입니다.");

            while (true)
            {
                Console.WriteLine("\n작업을 선택하세요:");
                Console.WriteLine("1. msg 파일을 CSV 파일로 변환");
                Console.WriteLine("2. CSV 파일을 msg 파일로 변환");
                Console.WriteLine("3. 종료");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        MsgToCsvConverter converter1 = new MsgToCsvConverter();
                        converter1.ConvertMsgToCsv();
                        break;
                    case "2":
                        CsvToMsgConverter converter2 = new CsvToMsgConverter();
                        converter2.ConvertCsvToMsg();
                        break;
                    case "3":
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
