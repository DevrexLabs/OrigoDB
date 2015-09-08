---
title: Redis Model
layout: submenu
---
# Redis Model

OrigoDB is very similar to Redis. Both are in-memory databases that use logging and snapshots for persistence. The `RedisModel` class is a partial implementation of Redis data structures and commands. The api tries to stay close to the original with regards to command names, arguments and types unless there is a c# idiom which makes more sense. Commands are implemented as methods on `RedisModel`.

## Implemented features
* Strings
* Keys
* Sets
* Lists
* Hashes
* SortedSets
* Geo
* Expiration

## Unimplemented features
* Cluster
* Connection
* Pub/sub
* HyperLogLog
* Scripting
* Server
* Transactions

## Example code

```csharp
var redis = Db.For<RedisModel>();
redis.Set("key", "42");
redis.Increment("key");
redis.IncrementBy("key", 4);
string number = redis.Get("key");
```

### Keys

Commands that operate on keys.

*Redis command* | *Method* | *Notes*
----------------|----------|----------
RANDOMKEY | RandomKey() |
RENAME | Rename(string key, string newKeyName)|
DEL | Delete(params string[] keys) | Deletes keys, returns number of keys deleted.
FLUSHDB  | Clear() | Removes all of the keys from the database
DBSIZE  | KeyCount() | Returns the total number or keys
EXISTS | Exists(string key) | true if the key Exists
? | Type(string key) | Returns a KeyType enum value
KEYS | Keys(string regex) | return array of keys matching a regex

### Strings
The redis string type represent strings, integers or bit fields.

*Redis command* | *Method* | *Notes*
----------------|----------|----------
MGET | MGet(params string[] keys) | Get array multiple strings, null when key missing
MSET | MSet(params string[] keyValuePairs) | keys and values are interlaced
STRLEN | StrLength(string key) |
APPEND | Append(string key, string value) |
SET | Set(string key, string value) |
SET NX | SetUnlessExists(string key, string value) |
GET | Get(string key) |
GETRANGE | GetRange(string key, int start, int end) | Substring
BITCOUNT | BitCount(string key, int startByte, int endByte) |
GETSET | GetSet(string key, string value) | set value and return old value
DECR | Decrement(string key)
DECRBY | DecrementBy(string key, long delta)
INCR | Increment(string key)
INCRBY | IncrementBy(string key, long delta)

### Hashes
Redis hashes are implemented with Dictionary<string,string>

*Redis command* | *Method* | *Notes*
----------------|----------|----------
HSET | HSet(string key, string field, string value) | set a hash key/value pair
HDEL | HDelete(string key, params string[] fields) | return number of fields removed
HEXIST | HExists(string key, string field) |
HGET | | HGet(string key, string field) |
HGETALL | HGetAll(string key) | Get all keys and values in a single array
HINCRBY | HIncrementBy(string key, string field, long delta) |
HLEN | HLen(string key) | number of fields in a hash
HKEYS | HKeys(string key) | get all hash keys
HVALUES | HValues(string key) | get all the hash values
HMSET | HMSet(string key, params string[] keyValuePairs) | Set hash key/values
HMGET | HMGet(string key, params string[] fields) | Get multiple values from hash

### Geospatial sorted sets

*Redis command* | *Method* | *Notes*
----------------|----------|----------
GEOADD | GeoAdd(string key, params NamedGeoPoint[] points) | add named lat/lon coords to a geospatial index
GEODIST | GeoDist(string key, string member1, string member2) | returns an ArcDistance object representing the distance between two named geo points
GEOPOS | GeoPos(string key, params string[] fields) | get positions of fields
GEORADIUS | GeoRadius(string key, GeoPoint center, double radiusKm, int maxItems) | get points within a given radiusKm
GEORADIUSBYMEMBER | GeoRadiusByMember(string key, string member, double radiusKm, int maxItems) get points within given distance of an existing points

### Lists

*Redis command* | *Method* | *Notes*
----------------|----------|----------
LINSERT | LInsert(string key, string pivot, string value, bool before = true) | insert item in list before or after item with value of pivot
LLEN | LLength(string key) | number of items in list
LPOP | LPop(string key) | remove and return first element of list
RPOP | RPop(string key) | remove and return last element of list
LSET | LSet(string key, int index, string value) | set list item at given index
LINDEX | LIndex(string key, int index) | get list item by index
LPUSH | LPush(string key, params string[] items) | prepend items to list
RPUSH | RPush(string key, params string[] items) | append items to list

### Sets
A set of string members.

*Redis command* | *Method* | *Notes*
----------------|----------|----------
SADD | SAdd(string key, params string[] member) | add members to set, return number added
SCARD | SCard(string key) | Number of set members
SDIFF | SDiff(string key, params string[] sets) | set difference of key minus each set in sets
SDIFFSTORE | SDiffStore(string destination, string key, params string[] sets) | same as SDiff but create a new set with the result, returns size of created set
SINTER | SInter(string key, params string[] sets) | Set intersection
SINTERSTORE | SInterStore(string key, params string[] sets) | Same as SInter but create new set with the result, returns size of created set
SISMEMBER | SIsMember(string key, string member) | test for membership
SMEMBERS | SMembers(string key) | Get all members
SMOVE | SMove(string source, string destination, string value) | move member from source set to destination set
SPOP | SPop(string key) | remove and return a random set member
SRANDMEMBER | SRandMember(string key, int count = 1) | get one or more random set menbers
SREM | SRemove(string key, params string[] members) | remove set members, return number removed
SUNION | SUnion(string key, params string[] keys) | set union
SUNIONSTORE | SUnionStore(string destination, string key, params string[] keys) | same as SUnion, but creates a new set


### Sorted sets
A sorted set is a set where members have an associated score.

*Redis command* | *Method* | *Notes*
----------------|----------|----------
ZADD | ZAdd(string key, string member, double score) | add member/score to sorted set
ZADD | ZAdd(string key, params KeyValuePair<string, double>[] items) | add multiple member/score pairs to sorted set
ZADD | ZAdd(string key, IDictionary<string, double> membersAndScores) | overload
ZADD | ZAdd(string key, params string[] scoreAndMembersInterlaced) | another overload
ZCARD | ZCard(string key) | cardinality of sorted set
ZCOUNT | ZCount(string key, double min, double max) | number of items within range
ZINCRBY | ZIncrementBy(string key, double increment, string member) | increment sorted set score
ZINTERSTORE | ZInterStore(string destination, string[] keys, double[] weights = null, AggregateType aggregateType = AggregateType.Sum) | yikes!
ZRANGE | ZRange(string key, int start  = 0, int stop = -1) | returns range of elements
ZRANGE | ZRangeWithScores(string key, int start = 0, int stop = -1) | same as ZRange but include scores
ZRANGEBYSCORE | ZRangeByScore(string key, double min, double max, int skip = 0, int take = Int32.MaxValue) | return sorted set members within given range
ZRANGEBYSCORE | ZRangeByScoreWithScores(string key, double min, double max, int skip = 0, int take = Int32.MaxValue) | same as ZRangeByScore but include scores
ZRANK | ZRank(string key, string member) | zero-based rank of sorted set item
ZREM | ZRemove(string key, params string[] members) | remove members from sorted set, return number removed
ZREMBYRANK | ZRemoveRangeByRank(string key, int first, int last) | remove members with rank within range
ZREMBYSCORE | ZRemoveRangeByScore(string key, double min, double max) | remove members with score within range
ZSCORE | ZScore(string key, string member) | get score or null if member doesn't exist
ZREVRANGE | ZReverseRange(string key, int start, int stop) | retrieve a range of members in reverse order
ZREVRANGE | ZReverseRangeWithScores(string key, int start = 0, int stop = 0) | overload including scores
ZREVRANGEBYSCORE | ZReverseRangeByScore(string key, double min = double.MinValue, double max = double.MaxValue, int skip = 0, int take = int.MaxValue) |
ZREVRANGEBYSCORE |  ZReverseRangeByScoreWithScores(string key, double min = double.MinValue,double max = double.MaxValue, int skip = 0, int take = Int32.MaxValue) |
ZREVRANK | ZReverseRank(string key, string member) | zero-based rank of sorted set item counting from end of set
ZUNIONSTORE | ZUnionStore(string destination, string[] keys, double[] weights = null, AggregateType aggregateType = AggregateType.Sum) | create sorted set union

### Expiration
*Redis command* | *Method* | *Notes*
----------------|----------|----------
EXPIRE | Expire(string key, DateTime at) | Set an expiration for a given key
EXPIRES | Expires(string key) | returns DateTime? when key expires or null
PERSIST | Persist(string key) | Cancel expiration
N/A | PurgeExpired() | removes keys scheduled for expiration, called by timer
