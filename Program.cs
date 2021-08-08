using System;
using System.IO;
using System.Threading;

namespace Signature
{
    class Program
    {
        static void Main(string[] args)
        {
            var length = new FileInfo(@"C:\\Users\\User\\Downloads\\result_efr.txt").Length;
            ReadFile(@"C:\\Users\\User\\Downloads\\result_efr.txt", 5000, length);
        }

        private static void ReadFile(string filePath, int bufferSize, long fileLength)
        {
            int MAX_BUFFER = bufferSize;
            byte[] buffer = new byte[MAX_BUFFER];
            int bytesRead;
            var blockNumber = 0;
            var doneEvents = new ManualResetEvent[(int)Math.Ceiling((double)fileLength / MAX_BUFFER)];
            var fileBlocks = new FileBlockItem[(int)Math.Ceiling((double)fileLength / MAX_BUFFER)];
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                while ((bytesRead = bs.Read(buffer, 0, MAX_BUFFER)) != 0)
                {
                    var tempbuffer = new byte[MAX_BUFFER];
                    Array.Copy(buffer, tempbuffer, MAX_BUFFER); // пока не понял как правильно byteRead или MaxBuffer, влияет на последний блок
                    // ну и последний блок затирал без копирования, так как ссылка
                    doneEvents[blockNumber] = new ManualResetEvent(false);
                    fileBlocks[blockNumber] = new FileBlockItem(doneEvents[blockNumber]) { SizeToHash = bytesRead, BlockNumber = blockNumber, Content = tempbuffer };
                    ThreadPool.QueueUserWorkItem(new WaitCallback(fileBlocks[blockNumber].CalculateHashMultiBlock));
                    blockNumber++;
                }
            }

            WaitHandle.WaitAll(doneEvents);

            for (int i = 0; i < fileBlocks.Length; i++)
            {
                fileBlocks[i].PrintHash();
            }
        }
    }
}

