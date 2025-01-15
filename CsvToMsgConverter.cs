using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Persona2EP_TextIOmanager
{
    public class CsvToMsgConverter
    {
        public void ConvertCsvToMsg()
        {
            string inputCsvPath = Utilities.GetFilePathFromDialog("변환할 CSV 파일 선택", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
            if (string.IsNullOrEmpty(inputCsvPath)) return;

            string outputTxtPath = Utilities.GetSaveFilePathFromDialog("msg 파일 저장 경로 선택", "msg files (*.msg)|*.msg|All files (*.*)|*.*");
            if (string.IsNullOrEmpty(outputTxtPath)) return;

            try
            {
                var parsedCsv = new List<string[]>();
                using (TextFieldParser parser = new TextFieldParser(inputCsvPath))
                {
                    parser.CommentTokens = new string[] { "#" };
                    parser.SetDelimiters(new string[] { "," });
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.TrimWhiteSpace = false; 

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        parsedCsv.Add(fields);
                    }
                }

                if (parsedCsv.Count <= 1)
                {
                    MessageBox.Show("CSV 파일에 데이터가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var messageData = new SortedDictionary<string, MessageData>();
                var header = parsedCsv[0];

                int keyIndex = Array.IndexOf(header, "Key");
                int translatedIndex = Array.IndexOf(header, "Translated");

                if (keyIndex == -1 || translatedIndex == -1)
                {
                    MessageBox.Show("CSV 파일 형식이 잘못되었습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int i = 1; i < parsedCsv.Count; i++)
                {
                    var columns = parsedCsv[i];
                    if (columns.Length <= Math.Max(keyIndex, translatedIndex)) continue;

                    string key = columns[keyIndex].Trim();
                    string translated = columns[translatedIndex];

                    if (key.Contains("_Choice"))
                    {
                        string parentKey = key.Split(new[] { "_Choice" }, StringSplitOptions.None)[0];
                        if (!messageData.ContainsKey(parentKey))
                            messageData[parentKey] = new MessageData();

                        messageData[parentKey].Choices.Add(translated);
                    }
                    else if (key.StartsWith("msg_"))
                    {
                        if (!messageData.ContainsKey(key))
                            messageData[key] = new MessageData();

                        string[] lines = translated.Split(new[] { "\n" }, StringSplitOptions.None);

                        for (int j = 0; j < lines.Length; j++)
                        {
                            string line = lines[j];
                            if (string.IsNullOrEmpty(line)) continue;

                            if (j == lines.Length - 1)
                            {
                                messageData[key].EndTags = line;
                            }
                            else
                            {
                                messageData[key].MainLines.Add(line);
                            }
                        }
                    }
                }

                StringBuilder output = new StringBuilder();

                output.AppendLine();

                foreach (var kvp in messageData)
                {
                    output.AppendLine($"{kvp.Key}:");
                    MessageData md = kvp.Value;

                    if (md.MainLines.Any())
                    {
                        if (md.Choices.Any())
                        {
                            foreach (string line in md.MainLines)
                            {
                                output.AppendLine($"[tab]{line}");
                            }
                        }
                        else
                        {
                            output.AppendLine(md.MainLines[0]);
                            for (int i = 1; i < md.MainLines.Count; i++)
                            {
                                output.AppendLine($"[tab]{md.MainLines[i]}");
                            }
                        }
                    }

                    if (md.Choices.Any())
                    {
                        output.AppendLine($"[choice({md.Choices.Count})]{string.Join("\n", md.Choices)}");
                    }

                    if (!string.IsNullOrEmpty(md.EndTags))
                    {
                        output.AppendLine(md.EndTags);
                    }

                    output.AppendLine();
                }

                string result = output.ToString();
                if (result.EndsWith("\r\n"))
                {
                    result = result.Substring(0, result.Length - 2);
                }
                else if (result.EndsWith("\r") || result.EndsWith("\n"))
                {
                    result = result.Substring(0, result.Length - 1);
                }

                File.WriteAllText(outputTxtPath, result, Encoding.UTF8);
                MessageBox.Show($"텍스트 파일로 변환 완료: {outputTxtPath}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
        }

        private class MessageData
        {
            public List<string> MainLines { get; set; } = new List<string>();
            public List<string> Choices { get; set; } = new List<string>();
            public string EndTags { get; set; } = string.Empty;
        }
    }
}