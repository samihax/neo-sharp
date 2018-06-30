using NeoSharp.BinarySerialization;
using NeoSharp.Core.Models;
using NeoSharp.Core.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoSharp.Persistence.RedisDB
{
    public class RedisDbRepository : IRepository
    {
        private readonly IRepositoryConfiguration _config;
        private readonly IBinarySerializer _serializer;
        private readonly IBinaryDeserializer _deserializer;
        private RedisHelper _redis;

        public RedisDbRepository(IRepositoryConfiguration config, IBinarySerializer serializer, IBinaryDeserializer deserializer)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

            //If no connection string provided, we can just default to localhost
            if (String.IsNullOrEmpty(_config.ConnectionString))
                _config.ConnectionString = "localhost";

            //Default to DB 0
            int dbId = _config.DatabaseId == null ? 0 : (int)_config.DatabaseId;

            //Make the connection to the specified server and database
            if(_redis == null)
                _redis = new RedisHelper(_config.ConnectionString, dbId);
        }

        #region IRepository Members

        public void AddBlockHeader(BlockHeader blockHeader)
        {
            //Serialize
            if(_config.StorageFormat == RepositoryPersistenceFormat.Binary)
            {
                var blockHeaderBytes = _serializer.Serialize(blockHeader);
                //Write the redis database with the binary bytes
                _redis.Database.Set(DataEntryPrefix.DataBlock, blockHeader.Hash.ToString(), blockHeaderBytes);
            }
            else if(_config.StorageFormat == RepositoryPersistenceFormat.JSON)
            {
                var blockHeaderJson = JsonConvert.SerializeObject(blockHeader);
                //Write the redis database with the binary bytes
                _redis.Database.Set(DataEntryPrefix.DataBlock, blockHeader.Hash.ToString(), blockHeaderJson);
            }
            
            //Add secondary indexes to find block hash by timestamp or height
            //Add to timestamp / blockhash index
            _redis.Database.AddToIndex(RedisIndex.BlockTimestamp, blockHeader.Timestamp, blockHeader.Hash.ToString());

            //Add to heignt / blockhash index
            _redis.Database.AddToIndex(RedisIndex.BlockHeight, blockHeader.Index, blockHeader.Hash.ToString());
        }

        public void AddTransaction(Transaction transaction)
        {
            if(_config.StorageFormat == RepositoryPersistenceFormat.Binary)
            {
                //Convert to bytes
                var transactionBytes = _serializer.Serialize(transaction);
                //Write the redis database with the binary bytes
                _redis.Database.Set(DataEntryPrefix.DataTransaction, transaction.Hash.ToString(), transactionBytes);
            }
            else if(_config.StorageFormat == RepositoryPersistenceFormat.JSON)
            {
                //Convert to bytes
                var transactionJson = JsonConvert.SerializeObject(transaction);
                //Write the redis database with the binary bytes
                _redis.Database.Set(DataEntryPrefix.DataTransaction, transaction.Hash.ToString(), transactionJson);
            }
        }

        public BlockHeader GetBlockHeaderByHeight(int height)
        {
            //Get the hash for the block at the specified height from our secondary index
            var values = _redis.Database.GetFromIndex(RedisIndex.BlockHeight, height);

            //We want only the first result
            if (values.Length > 0)
                return GetBlockHeaderById(values[0]);

            return null;
        }

        public BlockHeader GetBlockHeaderByTimestamp(int timestamp)
        {
            //Get the hash for the block with the specified timestamp from our secondary index
            var values = _redis.Database.GetFromIndex(RedisIndex.BlockTimestamp, timestamp);

            //We want only the first result
            if (values.Length > 0)
                return GetBlockHeaderById(values[0]);

            return null;
        }

        public BlockHeader GetBlockHeaderById(byte[] id)
        {
            return GetBlockHeaderById(Encoding.UTF8.GetString(id));
        }

        public BlockHeader GetBlockHeaderById(string id)
        {
            //Retrieve the block header
            var blockHeader = GetRawBlock(id);

            if(_config.StorageFormat == RepositoryPersistenceFormat.Binary)
            {
                return _deserializer.Deserialize<BlockHeader>((byte[])blockHeader);
            }
            else if(_config.StorageFormat == RepositoryPersistenceFormat.JSON)
            {
                return JsonConvert.DeserializeObject<BlockHeader>((string)blockHeader);
            }

            return null;
        }

        public object GetRawBlock(byte[] id)
        {
            return GetRawBlock(Encoding.UTF8.GetString(id));
        }

        public object GetRawBlock(string id)
        {
            return _redis.Database.Get(DataEntryPrefix.DataBlock, id);
        }

        public long GetTotalBlockHeight()
        {
            //Use the block height secondary index to tell us what our block height is
            return _redis.Database.GetIndexLength(RedisIndex.BlockHeight);
        }

        public Transaction GetTransaction(byte[] id)
        {
            return GetTransaction(Encoding.UTF8.GetString(id));
        }

        public Transaction GetTransaction(string id)
        {
            var transaction = _redis.Database.Get(DataEntryPrefix.DataTransaction, id);

            if (_config.StorageFormat == RepositoryPersistenceFormat.Binary)
            {
                return _deserializer.Deserialize<Transaction>(transaction);
            }
            else if (_config.StorageFormat == RepositoryPersistenceFormat.JSON)
            {
                return JsonConvert.DeserializeObject<Transaction>(transaction);
            }

            return null;
        }

        public Transaction[] GetTransactionsForBlock(byte[] id)
        {
            return GetTransactionsForBlock(Encoding.UTF8.GetString(id));
        }

        public Transaction[] GetTransactionsForBlock(string id)
        {
            List<Transaction> transactions = new List<Transaction>();
            var block = GetBlockHeaderById(id);

            foreach (var transactionHash in block.TransactionHashes)
            {
                var transaction = GetTransaction(transactionHash.ToString());
                transactions.Add(transaction);
            }

            return transactions.ToArray();
        }

        #endregion
    }
}
