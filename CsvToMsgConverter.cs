﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Persona2EP_TextIOmanager
{
    public class CsvToMsgConverter
    {
        public void ConvertCsvToMsg(string msgFilePath, string csvFilePath)
        {
            try
            {
                var parsedCsv = new List<string[]>();
                using (TextFieldParser parser = new TextFieldParser(csvFilePath))
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

                if (parsedCsv.Count == 0)
                {
                    Console.WriteLine("CSV 파일에 데이터가 없습니다.");
                    return;
                }

                var messageData = new SortedDictionary<string, MessageData>();

                foreach (var columns in parsedCsv)
                {
                    if (columns.Length < 3) continue;

                    string key = columns[0].Trim();
                    string originalText = columns[1];
                    string translatedText = columns[2];

                    // 번역문이 비어있으면 원문을 사용
                    string textToUse = string.IsNullOrWhiteSpace(translatedText) ? originalText : translatedText;

                    if (key.Contains("_Choice"))
                    {
                        string parentKey = key.Split(new[] { "_Choice" }, StringSplitOptions.None)[0];
                        if (!messageData.ContainsKey(parentKey))
                            messageData[parentKey] = new MessageData();

                        messageData[parentKey].Choices.Add(textToUse);
                    }
                    else if (key.StartsWith("msg_"))
                    {
                        if (!messageData.ContainsKey(key))
                            messageData[key] = new MessageData();

                        string[] lines = textToUse.Split(new[] { "\n" }, StringSplitOptions.None);

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

                File.WriteAllText(msgFilePath, result, Encoding.UTF8);
                Console.WriteLine($"텍스트 파일로 변환 완료: {msgFilePath}");
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