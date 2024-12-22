namespace file_processing_helper.Extensions;

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

public static class FileExtensions
{
    private static readonly Dictionary<string, string> FileTypeMap = new()
    {
        { ".jpg", FileTypes.Image },
        { ".jpeg", FileTypes.Image },
        { ".png", FileTypes.Image },
        { ".gif", FileTypes.Image },
        { ".bmp", FileTypes.Image },
        { ".tiff", FileTypes.Image },
        { ".svg", FileTypes.Image },
        { ".webp", FileTypes.Image },

        { ".pdf", FileTypes.Document },
        { ".doc", FileTypes.Document },
        { ".docx", FileTypes.Document },
        { ".xls", FileTypes.Document },
        { ".xlsx", FileTypes.Document },
        { ".ppt", FileTypes.Document },
        { ".pptx", FileTypes.Document },
        { ".txt", FileTypes.Document },
        { ".csv", FileTypes.Document },
        { ".rtf", FileTypes.Document },

        { ".mp4", FileTypes.Video },
        { ".avi", FileTypes.Video },
        { ".mov", FileTypes.Video },
        { ".webm", FileTypes.Video },
        { ".mkv", FileTypes.Video },

        { ".mp3", FileTypes.Audio },
        { ".wav", FileTypes.Audio },
        { ".ogg", FileTypes.Audio },
        { ".flac", FileTypes.Audio },

        { ".zip", FileTypes.Archive },
        { ".tar", FileTypes.Archive },
        { ".gzip", FileTypes.Archive },
        { ".rar", FileTypes.Archive },
        { ".7z", FileTypes.Archive },

        { ".html", FileTypes.Web },
        { ".htm", FileTypes.Web },
        { ".css", FileTypes.Web },
        { ".js", FileTypes.Web },
        { ".json", FileTypes.Web },
        { ".xml", FileTypes.Web },

        { ".woff", FileTypes.Font },
        { ".woff2", FileTypes.Font },
        { ".ttf", FileTypes.Font },
        { ".otf", FileTypes.Font },

        { ".md", FileTypes.Other },
        { ".psd", FileTypes.Image },

        { ".exe", FileTypes.Special },
        { ".bat", FileTypes.Special },
        { ".apk", FileTypes.Special }
    };

    public static string GetFileType(this IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (FileTypeMap.TryGetValue(extension, out var fileType))
        {
            return fileType;
        }

        throw new InvalidOperationException();
    }
}

public static class FileTypes
{
    public const string Image = "image";
    public const string Document = "document";
    public const string Video = "video";
    public const string Audio = "audio";
    public const string Archive = "archive";
    public const string Web = "web";
    public const string Font = "font";
    public const string Other = "other";
    public const string Special = "special";
}
