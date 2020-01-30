using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryExtensions
    {
        // file:///

        public static Task<Response> GetFileAsync(this IResourceRepository resources, string path, Action<FileRequest>? configureRequest = default)
        {
            return resources.GetAsync(CreateUri(path), default, configureRequest);
        }

        public static async Task<string> ReadTextFileAsync(this IResourceRepository resourceRepository, string path, Action<FileRequest>? configureRequest = default)
        {
            using var file = await resourceRepository.GetFileAsync(path, configureRequest);
            return await file.DeserializeTextAsync();
        }

        public static string ReadTextFile(this IResourceRepository resources, string path, Action<FileRequest>? configureRequest = default)
        {
            using var file = resources.GetFileAsync(path, configureRequest).GetAwaiter().GetResult();
            return file.DeserializeTextAsync().GetAwaiter().GetResult();
        }

        public static async Task WriteTextFileAsync(this IResourceRepository resources, string path, string value, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.PutAsync(CreateUri(path), value, configureRequest)) { }
        }

        public static async Task WriteFileAsync(this IResourceRepository resources, string path, CreateBodyStreamDelegate createBodyStream, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.PutAsync(CreateUri(path), createBodyStream, configureRequest)) { }
        }

        public static async Task DeleteFileAsync(this IResourceRepository resources, string path, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.DeleteAsync(CreateUri(path), default, configureRequest)) { }
        }

        private static UriString CreateUri(string path)
        {
            return
                Path.IsPathRooted(path) || IsUnc(path)
                    ? new UriString(UriSchemes.Known.File, path)
                    : new UriString(path);
        }

        // https://www.pcmag.com/encyclopedia/term/53398/unc
        private static bool IsUnc(string value) => value.StartsWith("//");
    }
    
    public class FileRequest : Request { }

    public class FileResponse : Response { }
}