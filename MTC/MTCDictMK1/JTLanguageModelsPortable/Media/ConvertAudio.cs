using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
//using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Object;
#if NOTHING
using cspeex;
using java.io;
#endif

namespace JTLanguageModelsPortable.Media
{
    public static class ConvertAudio
    {
        public static Audio Convert(Audio input, string convertToMimeType)
        {
            Audio output = null;

            if (input == null)
                return output;

            if (input.AudioMimeType == convertToMimeType)
                return input;

            switch (input.AudioMimeType)
            {
                case "audio/speex":
                    switch (convertToMimeType)
                    {
                        case "audio/wav":
                            output = ConvertSpeexToWav(input);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            return output;
        }

        public static Audio ConvertSpeexToWav(Audio input)
        {
#if NOTHING
            JSpeexDec decoder = new JSpeexDec();
            decoder.setDestFormat(JSpeexDec.FILE_FORMAT_WAVE);
            decoder.setStereo(false);
            Stream InStream = input.AudioStream;
            MemoryStream OutStream = new MemoryStream();
            decoder.decode(new RandomInputStream(InStream), new RandomOutputStream(OutStream));
            int outputLength = (int)OutStream.Length;
            byte[] outputData = new byte[outputLength];
            OutStream.Read(outputData, 0, outputLength);
            MemoryBuffer outputBuffer = new MemoryBuffer(outputData);
            Audio output = new Audio(input.Key, input.Name, input.Owner, "audio/wav", outputBuffer);
            return output;
#else
            return null;
#endif
        }
    }
}
