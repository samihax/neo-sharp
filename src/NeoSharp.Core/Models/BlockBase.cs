﻿using NeoSharp.BinarySerialization;
using NeoSharp.Core.Cryptography;
using NeoSharp.Core.Types;
using Newtonsoft.Json;

namespace NeoSharp.Core.Models
{
    public abstract class BlockBase
    {
        #region Public Properties 
        [BinaryProperty(0)]
        [JsonProperty("version")]
        public uint Version { get; private set; }

        [BinaryProperty(1)]
        [JsonProperty("previousblockhash")]
        public UInt256 PreviousBlockHash { get; private set; }

        [BinaryProperty(3)]
        [JsonProperty("time")]
        public uint Timestamp { get; private set; }

        [BinaryProperty(4)]
        [JsonProperty("index")]
        public uint Index { get; private set; }

        [BinaryProperty(5)]
        [JsonProperty("nonce")]
        public ulong ConsensusData { get; private set; }

        [BinaryProperty(6)]
        [JsonProperty("nextconsensus")]
        public UInt160 NextConsensus { get; private set; }

        [BinaryProperty(7)]
        public HeaderType Type { get; private set; }

        [JsonProperty("hash")] public UInt256 Hash { get; private set; }
        #endregion

        #region Protected Methods 
        protected void Sign(BlockBase blockBase, byte[] signingSettings)
        {
            this.Version = blockBase.Version;
            this.PreviousBlockHash = blockBase.PreviousBlockHash;
            this.Timestamp = blockBase.Timestamp;
            this.Index = blockBase.Index;
            this.ConsensusData = blockBase.ConsensusData;
            this.NextConsensus = blockBase.NextConsensus;
            this.Type = blockBase.Type;

            this.Hash = new UInt256(Crypto.Default.Hash256(signingSettings));

        }
        #endregion

    }
}
