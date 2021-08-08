using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Signature
{
    public class FileBlockItem
    {
        private ManualResetEvent _doneEvent;
        public byte[] Content { get; set; }

        public int BlockNumber { get; set; }

        public int SizeToHash { get; set; }

        public string Hash { get; private set; }

        public FileBlockItem(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }

        public void CalculateHashMultiBlock(object obj)
        {
            using SHA256 sha = SHA256.Create();
            int offset = 0;

            while (Content.Length - offset >= SizeToHash)
                offset += sha.TransformBlock(Content, offset, SizeToHash, Content, offset);

            sha.TransformFinalBlock(Content, offset, Content.Length - offset);

            Hash = BytesToStr(sha.Hash);
            _doneEvent.Set();
        }

        public void PrintHash()
        {
            Console.WriteLine($"block = {BlockNumber + 1}, Hash: {Hash}");
        }

        private string BytesToStr(byte[] bytes)
        {
            StringBuilder str = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
                str.AppendFormat("{0:X2}", bytes[i]);

            return str.ToString();
        }

    }
}
