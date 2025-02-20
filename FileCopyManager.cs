using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Persona2EP_TextIOmanager
{
    public class FileInfoJson
    {
        public string path { get; set; }
        public Dictionary<string, string> files { get; set; }
    }

    public static class FileCopyManager
    {
        private const string BASE_PATH = @"C:\Users\user\Documents\p2\p2_tool\.work\clean\Persona2EP.iso$";

        private static string GetPathFromUser(string title = "경로 입력")
        {
            Console.WriteLine(title);
            return Console.ReadLine();
        }

        public static void ProcessFileCopy()
        {
            try
            {
                // JSON 파일 선택
                string jsonFilePath = Utilities.GetFilePathFromDialog("JSON 파일 선택", "JSON 파일 (*.json)|*.json");
                if (string.IsNullOrEmpty(jsonFilePath))
                    return;

                // 저장 경로 선택
                string savePath = Utilities.GetFolderPathFromDialog("파일 저장 경로 선택");
                if (string.IsNullOrEmpty(savePath))
                    return;

                // JSON 파일 읽기
                string jsonContent = File.ReadAllText(jsonFilePath);
                FileInfoJson fileInfo = JsonSerializer.Deserialize<FileInfoJson>(jsonContent);

                string normalizedPath;

                // path가 없는 경우 사용자에게 입력 요청
                if (string.IsNullOrEmpty(fileInfo.path))
                {
                    string userPath = GetPathFromUser("파일 경로 입력");
                    if (string.IsNullOrEmpty(userPath))
                    {
                        Console.WriteLine("경로가 입력되지 않아 작업을 취소합니다.");
                        return;
                    }
                    normalizedPath = userPath.Replace("/", "\\").TrimStart('\\');
                }
                else
                {
                    normalizedPath = fileInfo.path.Replace("/", "\\").TrimStart('\\');
                }

                string fullSourcePath = Path.Combine(BASE_PATH, normalizedPath);

                int totalFiles = 0;
                int copiedFiles = 0;
                int skippedFiles = 0;

                // 각 파일 복사
                foreach (var file in fileInfo.files)
                {
                    totalFiles++;
                    string targetFileName = file.Key;
                    string sourceFilePath = file.Value.Replace("/", "\\").TrimStart('\\');

                    // 전체 소스 경로 구성
                    string sourceFullPath = Path.Combine(BASE_PATH, normalizedPath, sourceFilePath);
                    string targetFullPath = Path.Combine(savePath, targetFileName);

                    try
                    {
                        // 파일이 존재하는지 확인
                        if (!File.Exists(sourceFullPath))
                        {
                            Console.WriteLine($"파일을 찾을 수 없음 ({targetFileName}): {sourceFullPath}");
                            continue;
                        }

                        // 파일 크기 확인
                        var sourceFileInfo = new System.IO.FileInfo(sourceFullPath);
                        if (sourceFileInfo.Length == 0)
                        {
                            Console.WriteLine($"빈 파일 건너뜀: {targetFileName}");
                            skippedFiles++;
                            continue;
                        }

                        // 대상 디렉토리가 없으면 생성
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFullPath));

                        // 파일 복사
                        File.Copy(sourceFullPath, targetFullPath, true);
                        Console.WriteLine($"파일 복사 성공: {targetFileName} (크기: {sourceFileInfo.Length:N0} 바이트)");
                        copiedFiles++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"파일 복사 실패 ({targetFileName}): {ex.Message}");
                    }
                }

                Console.WriteLine("\n작업 완료 요약:");
                Console.WriteLine($"총 파일 수: {totalFiles}");
                Console.WriteLine($"복사된 파일: {copiedFiles}");
                Console.WriteLine($"건너뛴 빈 파일: {skippedFiles}");
                Console.WriteLine($"실패한 파일: {totalFiles - copiedFiles - skippedFiles}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
        }
    }
}