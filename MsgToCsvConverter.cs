using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Persona2EP_TextIOmanager
{
    public class MsgToCsvConverter
    {
        public void ConvertMsgToCsv()
        {
            string msgFilePath = Utilities.GetFilePathFromDialog("변환할 msg 파일 선택", "msg files (*.msg)|*.msg|All files (*.*)|*.*");
            if (string.IsNullOrEmpty(msgFilePath)) return;

            string csvFilePath = Utilities.GetSaveFilePathFromDialog("CSV 파일 저장 경로 선택", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
            if (string.IsNullOrEmpty(csvFilePath)) return;

            try
            {
                var dialogues = ParseMsgFileForCsv(msgFilePath);
                WriteCsvFile(dialogues, csvFilePath);
                Console.WriteLine("msg 파일이 CSV 파일로 성공적으로 변환되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"변환 중 오류 발생: {ex.Message}");
            }
        }

        private Dictionary<string, List<string>> ParseMsgFileForCsv(string filePath)
        {
            var dialogues = new Dictionary<string, List<string>>();
            string currentKey = null;
            List<string> currentBlock = new List<string>();
            bool inChoice = false;
            int expectedChoices = 0;
            int choiceCount = 0;
            var lines = File.ReadAllLines(filePath).ToList();
            int lineIndex = 0;

            while (lineIndex < lines.Count)
            {
                string line = lines[lineIndex].Trim();
                if (line.StartsWith("msg_"))
                {
                    if (currentKey != null && currentBlock.Count > 0)
                    {
                        dialogues[currentKey] = currentBlock;
                    }
                    currentKey = line.Split(':')[0];
                    currentBlock = new List<string>();
                    inChoice = false;
                    choiceCount = 0;
                    lineIndex++;
                }
                else if (line.StartsWith("[choice("))
                {
                    int startIndex = "[choice(".Length;
                    int endIndex = line.IndexOf(')', startIndex);
                    if (endIndex != -1)
                    {
                        string nStr = line.Substring(startIndex, endIndex - startIndex);
                        if (int.TryParse(nStr, out int nChoices))
                        {
                            expectedChoices = nChoices;
                            string choiceText = line.Substring(endIndex + 2).Trim();
                            if (!string.IsNullOrEmpty(choiceText))
                            {
                                choiceCount++;
                                string choiceKey = $"{currentKey}_Choice{choiceCount}";
                                dialogues[choiceKey] = new List<string> { choiceText };
                            }
                            for (int i = 1; i < nChoices; i++)
                            {
                                if (lineIndex + 1 < lines.Count)
                                {
                                    string choiceLine = lines[lineIndex + 1].Trim().Replace("[tab]", "");
                                    choiceCount++;
                                    string choiceKey = $"{currentKey}_Choice{choiceCount}";
                                    dialogues[choiceKey] = new List<string> { choiceLine };
                                    lineIndex++;
                                }
                            }
                            if (lineIndex + 1 < lines.Count)
                            {
                                lineIndex++;
                            }
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (!inChoice)
                    {
                        currentBlock.Add(line.Replace("[tab]", ""));
                        lineIndex++;
                    }
                    else
                    {
                        lineIndex++;
                    }
                }
                else
                {
                    lineIndex++;
                }
            }

            if (currentKey != null && currentBlock.Count > 0)
            {
                dialogues[currentKey] = currentBlock;
            }

            return dialogues;
        }

        private void WriteCsvFile(Dictionary<string, List<string>> dialogues, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Key,Original,Translated");
                foreach (var entry in dialogues.OrderBy(k => k.Key))
                {
                    if (entry.Key.StartsWith("msg_") && !entry.Key.Contains("_Choice"))
                    {
                        string originalText = string.Join("\n", entry.Value);
                        writer.WriteLine($"{entry.Key},{EscapeCsv(originalText)},");
                    }
                    else if (entry.Key.StartsWith("msg_") && entry.Key.Contains("_Choice"))
                    {
                        string originalText = entry.Value.FirstOrDefault();
                        writer.WriteLine($"{entry.Key},{EscapeCsv(originalText)},");
                    }
                }
            }
        }

        private string EscapeCsv(string str)
        {
            return $"\"{str.Replace("\"", "\"\"")}\"";
        }
    }
}