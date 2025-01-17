using System;
using System.IO;
using System.Linq;

namespace Persona2EP_TextIOmanager
{
    public static class OriginalTextExtractor
    {
        public static void Extract()
        {
            try
            {
                Console.WriteLine("작업할 폴더 경로를 입력하세요:");
                string baseDir = Console.ReadLine();

                if (!Directory.Exists(baseDir))
                {
                    Console.WriteLine("오류: 입력하신 경로를 찾을 수 없습니다.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"\n입력된 경로: {baseDir}");
                Console.WriteLine("폴더 검색 중...\n");

                string outputDir = Path.Combine(baseDir, "extracted_scripts");
                Directory.CreateDirectory(outputDir);

                // 기본 디렉토리에서 모든 폴더 찾기
                string[] allFolders = Directory.GetDirectories(baseDir);
                Console.WriteLine($"발견된 전체 폴더 수: {allFolders.Length}");

                // .bin$로 끝나는 폴더만 필터링
                var binFolders = allFolders.Where(folder =>
                    Path.GetFileName(folder).EndsWith(".bin$", StringComparison.OrdinalIgnoreCase)
                ).ToArray();

                Console.WriteLine($"\n.bin$ 폴더 수: {binFolders.Length}");

                int successCount = 0;
                int errorCount = 0;
                int skippedCount = 0;

                foreach (string binFolder in binFolders)
                {
                    try
                    {
                        string folderName = Path.GetFileName(binFolder);
                        string code = folderName.Substring(0, folderName.Length - 5);
                        string scriptPath = Path.Combine(binFolder, "8.efb$", "script.msg");

                        if (File.Exists(scriptPath))
                        {
                            // 파일 크기 확인
                            FileInfo fileInfo = new FileInfo(scriptPath);
                            if (fileInfo.Length == 0)
                            {
                                Console.WriteLine($"건너뜀: {folderName} - 파일 크기가 0KB입니다.");
                                skippedCount++;
                                continue;
                            }

                            string newFileName = $"{code}.msg";
                            string destPath = Path.Combine(outputDir, newFileName);

                            File.Copy(scriptPath, destPath, true);
                            successCount++;
                            Console.WriteLine($"성공: {folderName} -> {newFileName} ({fileInfo.Length:N0} bytes)");
                        }
                        else
                        {
                            Console.WriteLine($"오류: {folderName}에서 script.msg 파일을 찾을 수 없습니다.");
                            errorCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"오류 발생: {Path.GetFileName(binFolder)} - {ex.Message}");
                        errorCount++;
                    }
                }

                Console.WriteLine("\n작업 완료!");
                Console.WriteLine($"성공: {successCount}개");
                Console.WriteLine($"건너뜀: {skippedCount}개");
                Console.WriteLine($"실패: {errorCount}개");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"프로그램 오류: {ex.Message}");
            }

            Console.WriteLine("\n아무 키나 누르면 프로그램이 종료됩니다...");
            Console.ReadKey();
        }
    }
}
