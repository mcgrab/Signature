using System;
using System.Security.Cryptography;

namespace Signature
{
    public class FileBlockItem
    {
        private readonly byte[] _content;

        private readonly int _blockNumber;

        public string Hash { get; private set; }

        public FileBlockItem(int blockNumber, byte[] content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _blockNumber = blockNumber;
        }

        public void CalculateBlockHash()
        {
            using SHA256 sha = SHA256.Create();
            Hash = BitConverter.ToString(sha.ComputeHash(_content));
            PrintHash();
        }

        public void PrintHash()
        {
            Console.WriteLine($"Block = {_blockNumber}, Hash: {Hash}");
        }
    }
}
