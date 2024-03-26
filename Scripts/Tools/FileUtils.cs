using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

namespace JZzzzzzzTools
{
    public class FileUtils
    {
        /// <summary>
        /// 将文件复制到本地
        /// </summary>
        /// <param name="sourcePath">文件所在地(包括文件名)</param>
        /// <param name="targetPath">目标所在地(包括文件名)</param>
        public static void SaveLocally(string sourcePath, string targetPath) => File.Copy(sourcePath, targetPath);
        
        /// <summary>
        /// 将文件数据保存到本地
        /// </summary>
        /// <param name="targetPath">目标所在地(包括文件名)</param>
        /// <param name="fileData">文件所在地(包括文件名)</param>
        /// <returns></returns>
        public static async Task SaveLocallyByBytes(string targetPath, byte[] fileData)
        {
            await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await fileStream.WriteAsync(fileData, 0, fileData.Length);
        }
        
        /// <summary>
        /// 根据文件数据计算MD5值
        /// </summary>
        /// <param name="fileData"></param>
        public static string GetFileMD5ByBytes(byte[] fileData)
        {
            using var md5 = MD5.Create();
            var md5Data = md5.ComputeHash(fileData);
            var sb = new System.Text.StringBuilder();
            foreach (var t in md5Data)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString().Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 根据文件路径计算MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5ByPath(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        
        /// <summary>
        /// 读取路径下所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetFilesFromPath(string path) => Directory.GetFiles(path);


        /// <summary>
        /// 删除特定的文件
        /// </summary>
        /// <param name="path"></param>
        public static void RemoveFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            else
            {
                Debug.LogError($"文件不存在，无法删除：{path}");
            }
        }

        /// <summary>
        /// 获取当前文件的大小，单位：KB
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetFileSize(string path)
        {
            var fs = File.OpenRead(path);
            var fileSize = fs.Length;
            fs.Close();
            return (fileSize / 1024) + 1;
        }

        /// <summary>
        /// 异步读取文件数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetDataByPathAsync(string path)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var data = new byte[stream.Length];
            _ = await stream.ReadAsync(data, 0, data.Length);
            return data;
        } 
    }
}