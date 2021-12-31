// Derived of https://github.com/PMUniverse/PMU-Client/blob/5f369cfbe90c2b7d1251bbed364407a6900e0ab5/Client/Graphics/Tileset.cs (Copyright (c) 2014 PMU Staff)

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace pmu_stubs
{
    public class Tileset
    {
        private readonly int _pxHeight, _pxWidth;

        public Dictionary<int, byte[]> TileData { get; } = new();

        public int Height => _pxHeight / Constants.TileHeight;
        public int Width => _pxWidth / Constants.TileWidth;

        public int TileCount => Height * Width;

        public Tileset(Stream data)
        {
            data.Seek(0L, SeekOrigin.Begin);
            using var reader = new BinaryReader(data);

            _pxWidth = reader.ReadInt32();
            _pxHeight = reader.ReadInt32();

            var tileDataSizes = new int[TileCount];
            var tileDataOffsets = new long[TileCount];
            var headerSize = 8 + 12 * TileCount;

            for (var i = 0; i < TileCount; i++)
            {
                tileDataOffsets[i] = reader.ReadInt64() + headerSize;
                tileDataSizes[i] = reader.ReadInt32();
            }

            for (var i = 0; i < TileCount; i++)
            {
                data.Seek(tileDataOffsets[i], SeekOrigin.Begin);
                TileData[i] = new byte[tileDataSizes[i]];
                data.Read(TileData[i]);
            }
        }

        public void WriteToStream(Stream stream)
        {
            using var writer = new BinaryWriter(stream);

            // Write tileset bounds.
            writer.Write(_pxWidth);
            writer.Write(_pxHeight);

            // Build meta table data.
            var tileDataOffsets = new List<long>(TileCount) { 0L };
            var tileDataSizes = TileData.Select(pair => pair.Value.Length).ToList();
            for (var i = 1; i < TileCount; i++)
            {
                tileDataOffsets.Add(tileDataOffsets[i-1] + tileDataSizes[i-1]);
            }

            // Write meta table.
            for (var i = 0; i < TileCount; i++)
            {
                writer.Write(tileDataOffsets[i]);
                writer.Write(tileDataSizes[i]);
            }

            // Write all tile data.
            for (var i = 0; i < TileCount; i++)
            {
                writer.Write(TileData[i]);
            }
        }
    }
}