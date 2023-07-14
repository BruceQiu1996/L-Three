using SuperSocket.ProtoBase;
using System.Buffers;

namespace ThreeL.Infra.SuperSocket.Filters
{
    public abstract class AbstractPackageFilter<TPackage> : FixedHeaderPipelineFilter<TPackage> where TPackage : class
    {
        private readonly int _headerSize;

        public AbstractPackageFilter(int headerSize) : base(headerSize)
        {
            _headerSize = headerSize;
        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            reader.Advance(4 + 8);
            reader.TryReadBigEndian(out int len);

            return len - _headerSize;
        }

        public abstract TPackage ResolvePackage(ref ReadOnlySequence<byte> buffer);

        protected override TPackage DecodePackage(ref ReadOnlySequence<byte> buffer) => ResolvePackage(ref buffer);
    }
}
