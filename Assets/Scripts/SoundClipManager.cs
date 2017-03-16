using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class SoundClipManager : ThreadedJob
    {
        public bool ShouldRecord { get; set; }
        static public bool IsInited = false;
        static private string m_RecordingDevice = "";
        private MemoryStream m_clipMemory = null;
        private bool m_DidConsolidate = false;

        //--------------------------------------------------------------------------------------------------------------------------------
        public SoundClipManager()
        {
            // check initialization and initialize if needed.
            if (!IsInited)
            {
                ShouldRecord = false;
                // let's figure out how to record sound in Unity.
                foreach (string micDevice in Microphone.devices)
                {
                    m_RecordingDevice = micDevice;
                }
            }
            IsInited = true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void ClipStart()
        {
            m_DidConsolidate = false;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void ThreadFunction()
        {
            while (m_clipMemory == null)
            {
                System.Threading.Thread.Sleep(100);
            }

            MemoryStream cloudFormattedStream = createWaveFileStream(m_clipMemory);
            AITransactionHandler AITransact = new AITransactionHandler();
            AITransact.SendDataToCloud(cloudFormattedStream);
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public void Clear()
        {
            if (m_clipMemory != null)
            {
                m_clipMemory.Dispose();
                m_clipMemory = null;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ConsolidateClips(AudioClip inputClip, int samplePosition)
        {
            if (
                (inputClip != null) &&
                (inputClip.length > 0) &&
                (!m_DidConsolidate)
                )
            {
                if (m_clipMemory == null)
                    m_clipMemory = new MemoryStream();
                float[] data = new float[inputClip.samples];
                inputClip.GetData(data, 0);
                for (int i = 0; i <= samplePosition; i++)
                {
                    Int16 thisSampleAsPCM = (Int16)(data[i] * Int16.MaxValue);
                    byte[] sampleBytes = BitConverter.GetBytes(thisSampleAsPCM);
                    m_clipMemory.Write(sampleBytes, 0, sampleBytes.Length);
                }
                m_DidConsolidate = true;
            }
            return (m_DidConsolidate);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public void WriteWavHeader(System.IO.MemoryStream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            stream.Position = 0;

            // RIFF header.
            // Chunk ID.
            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

            // Chunk size.
            stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);

            // Format.
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);



            // Sub-chunk 1.
            // Sub-chunk 1 ID.
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

            // Sub-chunk 1 size.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // Audio format (floating point (3) or PCM (1)). Any other format indicates compression.
            stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

            // Channels.
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

            // Sample rate.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // Bytes rate.
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

            // Block align.
            stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

            // Bits per sample.
            stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);

            // Sub-chunk 2.
            // Sub-chunk 2 ID.
            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

            // Sub-chunk 2 size.
            stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        // Creates a file-headered wave memory stream out of stream
        public MemoryStream createWaveFileStream(System.IO.MemoryStream stream, int samplingRate = 16000)
        {
            MemoryStream outputStream = new MemoryStream();
            WriteWavHeader(outputStream, false, 1, 16, samplingRate, (int)stream.Length / 2);
            outputStream.Write(stream.ToArray(), 0, (int)stream.Length);
            outputStream.Flush();
            outputStream.Position = 0;
            return (outputStream);
        }
    }
}
