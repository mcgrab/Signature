using System;
using System.IO;

namespace Signature
{
    class Program
    {
        private static Lazy<Pool> _threadPool = new Lazy<Pool>(() => new Pool(GetOptimizedNumberOfThreads(_fileLength, _blockLength)));

        private static long _fileLength;

        private static int _blockLength;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Enter path.");
                    Console.WriteLine("Enter blocksize.");
                    return;
                }

                var firstArgument = args[0];

                switch (firstArgument)
                {
                    case "path" when args.Length == 2:
                        ReadFile(args[1], 5000);
                        break;
                    case "path" when args.Length == 4 && args[2] == "blocksize":
                        ReadFile(args[1], int.Parse(args[3]));
                        break;
                    case "blocksize" when args.Length == 4 && args[2] == "path":
                        ReadFile(args[3], int.Parse(args[1]));
                        break;
                    default:
                        throw new Exception("Invalid argument");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ReadFile(string filePath, int blockLength)
        {
            if (blockLength <= 0)
                throw new ArgumentException("Block size should exceed 0.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Path to file should not be null or whitespace.");

            if (!File.Exists(filePath))
                throw new ArgumentException($"Couldn't find a file: {filePath}");

            _fileLength = new FileInfo(filePath).Length;
            _blockLength = blockLength;

            var blockNumber = 0;

            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                while (br.BaseStream.Position != br.BaseStream.Length)
                {
                    var fileBlock = new FileBlockItem(blockNumber + 1, br.ReadBytes(blockLength));
                    _threadPool.Value.QueueUserWorkItem(() => fileBlock.CalculateBlockHash());
                    blockNumber++;
                }
            }
        }

        private static int GetOptimizedNumberOfThreads(long fileLength, int blockLength)
        {
            var numberOfActions = fileLength / blockLength + (fileLength % blockLength > 0 ? 1 : 0);
            return (int)(numberOfActions < Environment.ProcessorCount ? numberOfActions : Environment.ProcessorCount);
        }
    }
}

