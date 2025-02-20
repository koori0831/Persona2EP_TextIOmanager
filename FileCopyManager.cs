using System;
using System.IO;
using System.Text.Json;

namespace Persona2EP_TextIOmanager
{
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
                JsonDocument document = JsonDocument.Parse(jsonContent);
                JsonElement root = document.RootElement;

                string normalizedPath;

                // path 처리
                if (root.TryGetProperty("path", out JsonElement pathElement))
                {
                    normalizedPath = pathElement.GetString().Replace("/", "\\").TrimStart('\\');
                }
                else
                {
                    string userPath = GetPathFromUser("파일 경로 입력 (예: PSP_GAME/USRDIR/pack/)");
                    if (string.IsNullOrEmpty(userPath))
                    {
                        Console.WriteLine("경로가 입력되지 않아 작업을 취소합니다.");
                        return;
                    }
                    normalizedPath = userPath.Replace("/", "\\").TrimStart('\\');
                }

                // files 처리
                if (!root.TryGetProperty("files", out JsonElement filesElement))
                {
                    Console.WriteLine("'files' 속성을 찾을 수 없습니다.");
                    return;
                }

                int totalFiles = 0;
                int copiedFiles = 0;
                int skippedFiles = 0;

                foreach (JsonProperty fileEntry in filesElement.EnumerateObject())
                {
                    totalFiles++;
                    string targetFileName = fileEntry.Name;
                    JsonElement sourceValue = fileEntry.Value;

                    string sourceFilePath;

                    // 배열/문자열 처리
                    if (sourceValue.ValueKind == JsonValueKind.Array)
                    {
                        if (sourceValue.GetArrayLength() == 0)
                        {
                            Console.WriteLine($"빈 배열 건너뜀: {targetFileName}");
                            skippedFiles++;
                            continue;
                        }
                        sourceFilePath = sourceValue[0].GetString();
                    }
                    else if (sourceValue.ValueKind == JsonValueKind.String)
                    {
                        sourceFilePath = sourceValue.GetString();
                    }
                    else
                    {
                        Console.WriteLine($"지원하지 않는 형식: {targetFileName}");
                        continue;
                    }

                    // 경로 정규화
                    sourceFilePath = sourceFilePath.Replace("/", "\\").TrimStart('\\');
                    string sourceFullPath = Path.Combine(BASE_PATH, normalizedPath, sourceFilePath);
                    string targetFullPath = Path.Combine(savePath, targetFileName);

                    try
                    {
                        // 파일 존재 확인
                        if (!File.Exists(sourceFullPath))
                        {
                            Console.WriteLine($"파일 누락: {targetFileName} ({sourceFullPath})");
                            continue;
                        }

                        // 파일 크기 확인
                        var fileInfo = new FileInfo(sourceFullPath);
                        if (fileInfo.Length == 0)
                        {
                            Console.WriteLine($"빈 파일 건너뜀: {targetFileName}");
                            skippedFiles++;
                            continue;
                        }

                        // 디렉토리 생성
                        Directory.CreateDirectory(Path.GetDirectoryName(targetFullPath));

                        // 파일 복사
                        File.Copy(sourceFullPath, targetFullPath, true);
                        Console.WriteLine($"복사 성공: {targetFileName} ({fileInfo.Length:N0} bytes)");
                        copiedFiles++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"복사 실패 ({targetFileName}): {ex.Message}");
                    }
                }

                Console.WriteLine("\n작업 요약:");
                Console.WriteLine($"총 파일: {totalFiles}");
                Console.WriteLine($"성공: {copiedFiles}");
                Console.WriteLine($"건너뜀 (빈 파일): {skippedFiles}");
                Console.WriteLine($"실패: {totalFiles - copiedFiles - skippedFiles}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"치명적 오류: {ex.Message}");
            }
        }
    }
}