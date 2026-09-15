using System.Text;
using Firelink.Core.Models.Mo2;

namespace Firelink.Platform.MO2.Writers;

/// <summary>
/// Запись plugins.txt.
/// MO2 пишет файл в UTF-8 с BOM.
/// </summary>
public static class PluginsWriter
{
    public static void WriteFile(string path, PluginsFile file)
    {
        var sb = new StringBuilder();

        // MO2 сохраняет заголовки. Не критично, но для совместимости — пишем.
        sb.Append("# This file is used by Skyrim to keep track of your downloaded content.").Append("\r\n");
        sb.Append("# Please do not modify this file.").Append("\r\n");

        foreach (var entry in file.Entries)
        {
            if (entry.Enabled) sb.Append('*');
            sb.Append(entry.Name);
            sb.Append("\r\n");
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
