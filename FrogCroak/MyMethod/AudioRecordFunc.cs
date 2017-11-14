using System;
using Android.Media;
using Java.IO;
using System.Threading.Tasks;
using Android.Content;

namespace FrogCroak.MyMethod
{
    public class AudioRecordFunc
    {
        public int AUDIO_SAMPLE_RATE = 44100;
        private int bufferSizeInBytes = 0;

        private String AudioName = "";

        public String NewAudioName = "";

        private AudioRecord audioRecord;
        public static bool isRecord = false;

        private static AudioRecordFunc mInstance;

        private AudioRecordFunc(Context context)
        {
            string fileBasePath = context.FilesDir.AbsolutePath;
            AudioName = fileBasePath + "/RawAudio.raw";
            NewAudioName = fileBasePath + "/FinalAudio.wav";
        }

        public static AudioRecordFunc getInstance(Context context)
        {
            if (mInstance == null)
                mInstance = new AudioRecordFunc(context);
            return mInstance;
        }

        public void startRecordAndFile()
        {
            if (audioRecord == null)
                creatAudioRecord();

            audioRecord.StartRecording();

            isRecord = true;
            Task.Run(() =>
            {
                writeDataTOFile();
                copyWaveFile(AudioName, NewAudioName);
            });
        }

        public void stopRecordAndFile()
        {
            close();
        }

        private void close()
        {
            if (audioRecord != null)
            {
                isRecord = false;
                audioRecord.Stop();
                audioRecord.Release();
                audioRecord = null;
            }
        }

        private void creatAudioRecord()
        {
            bufferSizeInBytes = AudioRecord.GetMinBufferSize(AUDIO_SAMPLE_RATE,
                    ChannelIn.Stereo, Encoding.Pcm16bit);

            // 创建AudioRecord对象  
            audioRecord = new AudioRecord(AudioSource.Mic, AUDIO_SAMPLE_RATE,
                    ChannelIn.Stereo, Encoding.Pcm16bit, bufferSizeInBytes);
        }

        private void writeDataTOFile()
        {
            byte[] audiodata = new byte[bufferSizeInBytes];
            FileOutputStream fos = null;
            int readsize = 0;
            try
            {
                File file = new File(AudioName);
                if (file.Exists())
                {
                    file.Delete();
                }
                fos = new FileOutputStream(file);
            }
            catch (Exception e)
            {
                if (e.Source != null)
                    System.Console.WriteLine("Exception source: {0}", e.Source);
            }
            while (isRecord == true)
            {
                readsize = audioRecord.Read(audiodata, 0, bufferSizeInBytes);
                if ((int)RecordStatus.ErrorInvalidOperation != readsize && fos != null)
                {
                    try
                    {
                        fos.Write(audiodata);
                    }
                    catch (IOException e)
                    {
                        if (e.Source != null)
                            System.Console.WriteLine("IOException source: {0}", e.Source);
                    }
                }
            }
            try
            {
                if (fos != null)
                    fos.Close();
            }
            catch (IOException e)
            {
                if (e.Source != null)
                    System.Console.WriteLine("IOException source: {0}", e.Source);
            }
        }

        private void copyWaveFile(String inFilename, String outFilename)
        {
            FileInputStream inputStream = null;
            FileOutputStream outputStream = null;
            long totalAudioLen = 0;
            long totalDataLen = totalAudioLen + 36;
            long longSampleRate = AUDIO_SAMPLE_RATE;
            int channels = 2;
            long byteRate = 16 * AUDIO_SAMPLE_RATE * channels / 8;
            byte[] data = new byte[bufferSizeInBytes];
            try
            {
                inputStream = new FileInputStream(inFilename);
                outputStream = new FileOutputStream(outFilename);
                totalAudioLen = inputStream.Channel.Size();
                totalDataLen = totalAudioLen + 36;
                WriteWaveFileHeader(outputStream, totalAudioLen, totalDataLen,
                        longSampleRate, channels, byteRate);
                while (inputStream.Read(data) != -1)
                {
                    outputStream.Write(data);
                }
                inputStream.Close();
                outputStream.Close();
            }
            catch (FileNotFoundException e)
            {
                if (e.Source != null)
                    System.Console.WriteLine("FileNotFoundException source: {0}", e.Source);
            }
            catch (IOException e)
            {
                if (e.Source != null)
                    System.Console.WriteLine("IOException source: {0}", e.Source);
            }
        }

        private void WriteWaveFileHeader(FileOutputStream outputStream, long totalAudioLen,
                long totalDataLen, long longSampleRate, int channels, long byteRate)
        {
            byte[] header = new byte[44];
            header[0] = (byte)'R'; // RIFF/WAVE header  
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            header[4] = (byte)(totalDataLen & 0xff);
            header[5] = (byte)((totalDataLen >> 8) & 0xff);
            header[6] = (byte)((totalDataLen >> 16) & 0xff);
            header[7] = (byte)((totalDataLen >> 24) & 0xff);
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            header[12] = (byte)'f'; // 'fmt ' chunk  
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            header[16] = 16; // 4 bytes: size of 'fmt ' chunk  
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            header[20] = 1; // format = 1  
            header[21] = 0;
            header[22] = (byte)channels;
            header[23] = 0;
            header[24] = (byte)(longSampleRate & 0xff);
            header[25] = (byte)((longSampleRate >> 8) & 0xff);
            header[26] = (byte)((longSampleRate >> 16) & 0xff);
            header[27] = (byte)((longSampleRate >> 24) & 0xff);
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);
            header[32] = (byte)(2 * 16 / 8); // block align  
            header[33] = 0;
            header[34] = 16; // bits per sample  
            header[35] = 0;
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            header[40] = (byte)(totalAudioLen & 0xff);
            header[41] = (byte)((totalAudioLen >> 8) & 0xff);
            header[42] = (byte)((totalAudioLen >> 16) & 0xff);
            header[43] = (byte)((totalAudioLen >> 24) & 0xff);
            outputStream.Write(header, 0, 44);
        }
    }

}