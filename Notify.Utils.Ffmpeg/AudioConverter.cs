ï»¿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Notify.Utils.Ffmpeg
{
    public interface IAudioConverter
    {
        Stream ConvertTo(Stream input, InputFormat format);
    }

    public class AudioConverter : IAudioConverter
    {
        private readonly IFileSystem fileSystem;
        private readonly Func<IProcessWrapper> processWrapperFactory;
        private string ffmpegPath;

        public AudioConverter() : this(new FileSystem(), Environment.OSVersion.Platform, () => new ProcessWrapper())
        {
        }

        internal AudioConverter(IFileSystem fileSystem, PlatformID platform, Func<IProcessWrapper> processWrapperFactory)
        {
            this.fileSystem = fileSystem;
            this.processWrapperFactory = processWrapperFactory;
            ffmpegPath = platform switch
            {
                PlatformID.Win32NT => "./ffmpeg/ffmpeg.exe",
                PlatformID.Unix => "./ffmpeg/ffmpeg",
                _ => throw new PlatformNotSupportedException()
            };
        }

        public Stream ConvertTo(Stream input, InputFormat format)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(format);

            using var destinationFile = new TempFile(fileSystem);

            var ffmpegStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = @$"-hide_banner -y -f {format} -i pipe:0 -acodec pcm_mulaw -ar 8000 -ac 1 -f wav ""{destinationFile}""",
                RedirectStandardInput = true,
                CreateNoWindow = false,
                RedirectStandardError = true
            };

            var processWrapper = processWrapperFactory();
            processWrapper.Start(ffmpegStartInfo);

            return null;
        }
    }
}
