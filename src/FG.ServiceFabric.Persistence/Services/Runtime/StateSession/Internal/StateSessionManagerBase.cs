using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FG.Common.Utils;
using FG.ServiceFabric.Services.Runtime.StateSession.Metadata;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Data;

namespace FG.ServiceFabric.Services.Runtime.StateSession.Internal
{
	public abstract class StateSessionManagerBase<TStateSession> : IStateSessionManager, IStateSessionWritableManager, IStateSessionManagerInternals
		where TStateSession : class, IStateSession
	{
		public class StateChange
		{
			public StateChange(StateChangeType changeType, StateWrapper value, Type valueType)
			{
				ChangeType = changeType;
				Value = value;
				ValueType = valueType;
			}

			public StateChangeType ChangeType { get; set; }
			public StateWrapper Value { get; set; }
			public Type ValueType { get; set; }
		}

		public enum StateChangeType
		{
			None = 0,
			AddOrUpdate = 1,
			Remove = 2,
			Enqueue = 3,
			Dequeue = 4,
		}

		private readonly IDictionary<string, QueueInfo> _openQueues = new ConcurrentDictionary<string, QueueInfo>();

		private readonly Nito.AsyncEx.AsyncReaderWriterLock _lock = new Nito.AsyncEx.AsyncReaderWriterLock();

		Nito.AsyncEx.AsyncReaderWriterLock IStateSessionManagerInternals.Lock => _lock;

		protected StateSessionManagerBase(
			string serviceName,
			Guid partitionId,
			string partitionKey)
		{
			ServiceName = serviceName;
			PartitionId = partitionId;
			PartitionKey = partitionKey;
		}

		protected Guid PartitionId { get; }
		protected string PartitionKey { get; }
		protected string ServiceName { get; }

		#region Internals

		private IStateSessionManagerInternals Internals => (IStateSessionManagerInternals) this;

		IDictionary<string, QueueInfo> IStateSessionManagerInternals.OpenQueues => _openQueues;

		SchemaStateKey IStateSessionManagerInternals.GetKey(ISchemaKey key)
		{
			return new SchemaStateKey(ServiceName, PartitionKey, key?.Schema, key?.Key);
		}

		IServiceMetadata IStateSessionManagerInternals.GetMetadata()
		{
			return new ServiceMetadata() { ServiceName = ServiceName, PartitionKey = PartitionKey };
		}

		IValueMetadata IStateSessionManagerInternals.GetOrCreateMetadata(IValueMetadata metadata, StateWrapperType type)
		{
			if (metadata == null)
			{
				metadata = new ValueMetadata(type);
			}
			else
			{
				metadata.SetType(type);
			}
			return metadata;
		}

		StateWrapper IStateSessionManagerInternals.BuildWrapper(IValueMetadata valueMetadata, SchemaStateKey key)
		{
			var serviceMetadata = Internals.GetMetadata();
			if (valueMetadata == null) valueMetadata = new ValueMetadata(StateWrapperType.Unknown);
			valueMetadata.Key = key.Key;
			valueMetadata.Schema = key.Schema;
			var wrapper = valueMetadata.BuildStateWrapper(key, serviceMetadata);
			return wrapper;
		}

		StateWrapper IStateSessionManagerInternals.BuildWrapper(IValueMetadata metadata, SchemaStateKey key,
			Type valueType, object value)
		{
			return (StateWrapper)this.CallGenericMethod(
				$"{typeof(IStateSessionManagerInternals).FullName}.{nameof(IStateSessionManagerInternals.BuildWrapperGeneric)}",
				new Type[] { valueType },
				Internals.GetOrCreateMetadata(metadata, StateWrapperType.ReliableDictionaryItem), key, value);
		}

		StateWrapper<T> IStateSessionManagerInternals.BuildWrapperGeneric<T>(IValueMetadata valueMetadata, SchemaStateKey key, T value)
		{
			var serviceMetadata = Internals.GetMetadata();
			if (valueMetadata == null) valueMetadata = new ValueMetadata(StateWrapperType.Unknown);
			valueMetadata.Key = key.Key;
			valueMetadata.Schema = key.Schema;
			var wrapper = valueMetadata.BuildStateWrapper(key, value, serviceMetadata);
			return wrapper;
		}

		string IStateSessionManagerInternals.GetEscapedKey(string key)
		{
			return GetEscapedKeyInternal(key);
		}

		string IStateSessionManagerInternals.GetUnescapedKey(string key)
		{
			return GetUnescapedKeyInternal(key);
		}

		#endregion

		public Task<IStateSessionReadOnlyDictionary<T>> OpenDictionary<T>(string schema,
			CancellationToken cancellationToken = default(CancellationToken))
		{
			var dictionary =
				(IStateSessionReadOnlyDictionary<T>)
				new StateSessionBaseReadOnlyDictionary<StateSessionManagerBase<TStateSession>, TStateSession, T>(this, schema, readOnly: false);
			return Task.FromResult(dictionary);
		}
		
		Task<IStateSessionDictionary<T>> IStateSessionWritableManager.OpenDictionary<T>(
			string schema, 
			CancellationToken cancellationToken = new CancellationToken())
		{
			var dictionary =
				(IStateSessionDictionary<T>)
				new StateSessionBaseDictionary<StateSessionManagerBase<TStateSession>, TStateSession, T>(this, schema, readOnly: false);
			return Task.FromResult(dictionary);
		}

		Task<IStateSessionQueue<T>> IStateSessionWritableManager.OpenQueue<T>(
			string schema,
			CancellationToken cancellationToken = new CancellationToken())
		{
			if (!_openQueues.ContainsKey(schema))
			{
				_openQueues.Add(schema, null);
			}
			var queue = (IStateSessionQueue<T>)new StateSessionBaseQueue<TStateSession, T>(this, schema, readOnly: false);
			return Task.FromResult(queue);
		}

		public Task<IStateSessionReadOnlyQueue<T>> OpenQueue<T>(
			string schema,
			CancellationToken cancellationToken = new CancellationToken())
		{
			if (!_openQueues.ContainsKey(schema))
			{
				_openQueues.Add(schema, null);
			}
			var queue = (IStateSessionReadOnlyQueue<T>) new StateSessionBaseReadOnlyQueue<TStateSession, T>(this, schema, readOnly: false);
			return Task.FromResult(queue);
		}

		#region  Writable
		public IStateSessionWritableManager Writable => (IStateSessionWritableManager)this;
		IStateSession IStateSessionWritableManager.CreateSession(params IStateSessionObject[] stateSessionObjects)
		{
			var session = CreateSessionInternal(this, stateSessionObjects);
			return session;
		}

		#endregion

		public IStateSessionReader CreateSession(params IStateSessionReadOnlyObject[] stateSessionObjects)
		{		
			var session = CreateSessionInternal(this, stateSessionObjects);
			return session;
		}

		protected virtual string GetEscapedKeyInternal(string key)
		{
			return key;
		}

		protected virtual string GetUnescapedKeyInternal(string key)
		{
			return key;
		}

		protected abstract TStateSession CreateSessionInternal(StateSessionManagerBase<TStateSession> manager,
			IStateSessionReadOnlyObject[] stateSessionObjects);
		protected abstract TStateSession CreateSessionInternal(StateSessionManagerBase<TStateSession> manager,
			IStateSessionObject[] stateSessionObjects);


		public abstract class StateSessionBase<TStateSessionManager> : IStateSession
			where TStateSessionManager : class, IStateSessionManagerInternals, IStateSessionManager
		{
			private readonly object _lock = new object();
			private readonly TStateSessionManager _manager;

			private readonly IDictionary<string, StateChange> _transactedChanges =
				new ConcurrentDictionary<string, StateChange>();

			private IEnumerable<IStateSessionReadOnlyObject> _attachedObjects;

			private IDisposable _rwLock;

			protected StateSessionBase(TStateSessionManager manager, IEnumerable<IStateSessionReadOnlyObject> stateSessionObjects)
			{
				AttachObjects(stateSessionObjects);
				_manager = manager;

				IsReadOnly = true;
				AquireLock().GetAwaiter().GetResult();
			}

			protected StateSessionBase(TStateSessionManager manager, IEnumerable<IStateSessionObject> stateSessionObjects)
			{
				AttachObjects(stateSessionObjects);
				_manager = manager;

				IsReadOnly = false;
				AquireLock().GetAwaiter().GetResult();				
			}

			private async Task AquireLock()
			{
				_rwLock = IsReadOnly ? await _managerInternals.Lock.ReaderLockAsync() : await _managerInternals.Lock.WriterLockAsync();
			}

			public bool IsReadOnly { get; }

			private void CheckIsNotReadOnly()
			{
				if (IsReadOnly)
				{
					throw new StateSessionException($"Tried to modify a StateSession that is in ReadOnly mode");
				}
			}

			private IStateSessionManagerInternals _managerInternals => _manager;

			public Task<bool> Contains<T>(string schema, string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				return Contains(schema, key, cancellationToken);
			}

			public Task<bool> Contains(string schema, string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, key));

				if (_transactedChanges.ContainsKey(id))
				{
					// Check if session contains it, or if it has been removed from session
					switch (_transactedChanges[id].ChangeType)
					{
						case StateChangeType.AddOrUpdate:
							return Task.FromResult(true);
						case StateChangeType.Remove:
							return Task.FromResult(false);
						case StateChangeType.None:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;
				return ContainsInternal(id, cancellationToken);
			}

			public Task<FindByKeyPrefixResult<T>> FindByKeyPrefixAsync<T>(string schema, string keyPrefix,
				int maxNumResults = 100000,
				ContinuationToken continuationToken = null,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				throw new NotImplementedException();
			}

			public async Task<FindByKeyPrefixResult> FindByKeyPrefixAsync(string schema, string keyPrefix,
				int maxNumResults = 100000,
				ContinuationToken continuationToken = null,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				var schemaKeyPrefix = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(keyPrefix)));
				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var result =
					await FindByKeyPrefixInternalAsync(schemaKeyPrefix, maxNumResults, continuationToken, cancellationToken);
				return result;
			}

			public async Task<IEnumerable<string>> EnumerateSchemaNamesAsync(string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				var schemaKeyPrefix = _manager.GetKey(null);
				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;
				var result = await EnumerateSchemaNamesInternalAsync(schemaKeyPrefix, key, cancellationToken);
				return result;
			}

			public async Task<ConditionalValue<T>> TryGetValueAsync<T>(string schema, string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(key)));

				// Check if session contains it, or if it has been removed from session
				if (_transactedChanges.ContainsKey(id))
				{
					var transactedChange = _transactedChanges[id];
					switch (transactedChange.ChangeType)
					{
						case StateChangeType.AddOrUpdate:
							return new ConditionalValue<T>(true, ((StateWrapper<T>) transactedChange.Value).State);
						case StateChangeType.Remove:
							return new ConditionalValue<T>(false, default(T));
						case StateChangeType.None:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;
				var value = await TryGetValueInternalAsync<T>(id, cancellationToken);
				if (value.HasValue)
				{
					return new ConditionalValue<T>(true, value.Value.State);
				}
				return new ConditionalValue<T>(false, default(T));
			}

			public async Task<T> GetValueAsync<T>(string schema, string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(key)));

				// Check if session contains it, or if it has been removed from session
				if (_transactedChanges.ContainsKey(id))
				{
					var transactedChange = _transactedChanges[id];
					switch (transactedChange.ChangeType)
					{
						case StateChangeType.AddOrUpdate:
							return ((StateWrapper<T>) transactedChange.Value).State;
						case StateChangeType.Remove:
							throw new KeyNotFoundException($"State with {id} does not exist");
						case StateChangeType.None:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;
				var value = await GetValueInternalAsync<T>(id, cancellationToken);
				if (value != null)
				{
					return value.State;
				}
				throw new StateSessionException($"Failed to GetValueAsync for {schema}:{key}");
			}

			public Task SetValueAsync<T>(string schema, string key, T value, IValueMetadata metadata,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				CheckIsNotReadOnly();

				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(key)));

				var valueType = typeof(T);
				var document = _managerInternals.BuildWrapperGeneric<T>(
					_managerInternals.GetOrCreateMetadata(metadata, StateWrapperType.ReliableDictionaryItem), id, value);

				_transactedChanges[id] = new StateChange(StateChangeType.AddOrUpdate, document, valueType);
				return Task.FromResult(true);
			}

			public Task SetValueAsync(string schema, string key, Type valueType, object value, IValueMetadata metadata,
				CancellationToken cancellationToken = new CancellationToken())
			{
				CheckIsNotReadOnly();

				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(key)));
				var document = _managerInternals.BuildWrapper(
					_managerInternals.GetOrCreateMetadata(metadata, StateWrapperType.ReliableDictionaryItem), id,
					valueType, value);

				_transactedChanges[id] = new StateChange(StateChangeType.AddOrUpdate, document, valueType);
				return Task.FromResult(true);
			}

			public Task RemoveAsync<T>(string schema, string key, CancellationToken cancellationToken = new CancellationToken())
			{
				return RemoveAsync(schema, key, cancellationToken);
			}

			public Task RemoveAsync(string schema, string key,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				CheckIsNotReadOnly();

				var id = _managerInternals.GetKey(new DictionaryStateKey(schema, _manager.GetEscapedKey(key)));
				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var stateWrapper = _managerInternals.BuildWrapper(
					_managerInternals.GetOrCreateMetadata(null, StateWrapperType.ReliableDictionaryItem), id);

				_transactedChanges[id] = new StateChange(StateChangeType.Remove, stateWrapper, null);
				return Task.FromResult(true);
			}

			public async Task EnqueueAsync<T>(string schema, T value, IValueMetadata metadata,
				CancellationToken cancellationToken = new CancellationToken())
			{
				CheckIsNotReadOnly();

				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var stateQueueInfo = await GetOrAddQueueInfo(schema, cancellationToken);
				var head = stateQueueInfo.HeadKey;
				head++;
				stateQueueInfo.HeadKey = head;
				var id = _managerInternals.GetKey(new QueueItemStateKey(schema, head));
				var document = _managerInternals.BuildWrapperGeneric(
					_managerInternals.GetOrCreateMetadata(metadata, StateWrapperType.ReliableQueueInfo), id, value);

				await SetValueInternalAsync(id, document, typeof(T), cancellationToken);
				await SetQueueInfo(schema, stateQueueInfo, cancellationToken);
			}

			public async Task<ConditionalValue<T>> DequeueAsync<T>(string schema,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				CheckIsNotReadOnly();

				var stateQueueInfo = await GetOrAddQueueInfo(schema, cancellationToken);
				var tail = stateQueueInfo.TailKey;
				var head = stateQueueInfo.HeadKey;

				if ((tail - head) == 1)
				{
					return new ConditionalValue<T>(false, default(T));
				}
				var id = _managerInternals.GetKey(new QueueItemStateKey(schema, tail));
				var value = await TryGetValueInternalAsync<T>(id, cancellationToken);
				if (!value.HasValue)
				{
					return new ConditionalValue<T>(false, default(T));
				}

				tail++;

				stateQueueInfo.TailKey = tail;
				await SetQueueInfo(schema, stateQueueInfo, cancellationToken);

				await RemoveInternalAsync(id, cancellationToken);

				return new ConditionalValue<T>(true, value.Value.State);
			}

			public async Task<ConditionalValue<T>> PeekAsync<T>(string schema,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var stateQueueInfo = await GetOrAddQueueInfo(schema, cancellationToken);
				var tail = stateQueueInfo.TailKey;
				var head = stateQueueInfo.HeadKey;

				if ((tail - head) == 1)
				{
					return new ConditionalValue<T>(false, default(T));
				}
				var id = _managerInternals.GetKey(new QueueItemStateKey(schema, tail));
				var value = await TryGetValueInternalAsync<T>(id, cancellationToken);
				if (!value.HasValue)
				{
					return new ConditionalValue<T>(false, default(T));
				}
				return new ConditionalValue<T>(value.HasValue, value.Value.State);
			}

			public Task<long> GetDictionaryCountAsync<T>(string schema, CancellationToken cancellationToken)
			{
				return GetCountInternalAsync(schema, cancellationToken);
			}

			public async Task<long> GetEnqueuedCountAsync<T>(string schema, CancellationToken cancellationToken)
			{
				var stateQueueInfo = await GetOrAddQueueInfo(schema, cancellationToken);
				var tail = stateQueueInfo.TailKey;
				var head = stateQueueInfo.HeadKey;

				return head - tail + 1;
			}

			public void Dispose()
			{
				DetachObjects();
				CommitAsync();
				_rwLock.Dispose();
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public Task CommitAsync()
			{
				var changes = _transactedChanges.Values.ToArray();
				_transactedChanges.Clear();
				return CommitinternalAsync(changes);
			}

			public Task AbortAsync()
			{
				_transactedChanges.Clear();
				return Task.FromResult(true);
			}

			private void AttachObjects(IEnumerable<IStateSessionReadOnlyObject> stateSessionObjects)
			{
				_attachedObjects = stateSessionObjects;
				var stateSession = this as TStateSession;
				foreach (var stateSessionObject in _attachedObjects)
				{
					if (!(stateSessionObject is StateSessionBaseObject<TStateSession> stateSessionBaseObject))
					{
						throw new StateSessionException(
							$"Can only attach object that have been created by the owning StateSessionManager");
					}
					stateSessionBaseObject.AttachToSession(stateSession);
				}
			}

			private void DetachObjects()
			{
				var stateSession = this as TStateSession;
				foreach (var stateSessionObject in _attachedObjects)
				{
					if (!(stateSessionObject is StateSessionBaseObject<TStateSession> stateSessionBaseObject))
					{
						throw new StateSessionException(
							$"Can only detach object that have been created by the owning StateSessionManager");
					}
					stateSessionBaseObject.DetachFromSession(stateSession);
				}
				_attachedObjects = new IStateSessionObject[0];
			}

			protected abstract Task<bool> ContainsInternal(SchemaStateKey id,
				CancellationToken cancellationToken = default(CancellationToken));

			protected abstract Task<FindByKeyPrefixResult> FindByKeyPrefixInternalAsync(string schemaKeyPrefix,
				int maxNumResults = 100000,
				ContinuationToken continuationToken = null,
				CancellationToken cancellationToken = new CancellationToken());

			protected abstract Task<IEnumerable<string>> EnumerateSchemaNamesInternalAsync(string schemaKeyPrefix, string key,
				CancellationToken cancellationToken = default(CancellationToken));

			protected abstract Task<ConditionalValue<StateWrapper<T>>> TryGetValueInternalAsync<T>(SchemaStateKey id,
				CancellationToken cancellationToken = default(CancellationToken));

			protected abstract Task<StateWrapper<T>> GetValueInternalAsync<T>(string id,
				CancellationToken cancellationToken = default(CancellationToken));

			protected abstract Task SetValueInternalAsync(SchemaStateKey id, StateWrapper value,
				Type valueType,
				CancellationToken cancellationToken = default(CancellationToken));

			protected abstract Task RemoveInternalAsync(SchemaStateKey id,
				CancellationToken cancellationToken = default(CancellationToken));

			private async Task<QueueInfo> GetOrAddQueueInfo(string schema,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				if (!_managerInternals.OpenQueues.ContainsKey(schema))
				{
					throw new StateSessionException($"Queue {schema} must be open before starting a session that uses it");
				}

				QueueInfo queueInfo;
				lock (_lock)
				{
					queueInfo = _managerInternals.OpenQueues[schema];
					if (queueInfo != null)
					{
						return queueInfo;
					}
				}

				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var id = _managerInternals.GetKey(new QueueInfoStateKey(schema));

				var value = await TryGetValueInternalAsync<QueueInfo>(id, cancellationToken);
				if (value.HasValue)
				{
					lock (_lock)
					{
						queueInfo = value.Value.State;
						if (queueInfo != null)
						{
							_managerInternals.OpenQueues[schema] = queueInfo;
							return queueInfo;
						}
					}
				}

				queueInfo = new QueueInfo()
				{
					HeadKey = -1L,
					TailKey = 0L,
				};
				var metadata = new ValueMetadata(StateWrapperType.ReliableQueueItem);
				var document = _managerInternals.BuildWrapperGeneric(metadata, id, queueInfo);

				await SetValueInternalAsync(id, document, typeof(QueueInfo), cancellationToken);

				lock (_lock)
				{
					_managerInternals.OpenQueues[schema] = queueInfo;
					return queueInfo;
				}
			}


			private async Task SetQueueInfo(string schema, QueueInfo value,
				CancellationToken cancellationToken = default(CancellationToken))
			{
				cancellationToken = cancellationToken == default(CancellationToken) ? CancellationToken.None : cancellationToken;

				var id = _managerInternals.GetKey(new QueueInfoStateKey(schema));

				var metadata = new ValueMetadata(StateWrapperType.ReliableQueueItem);
				var document = _managerInternals.BuildWrapperGeneric(metadata, id, value);

				await SetValueInternalAsync(id, document, typeof(QueueInfo), cancellationToken);

				lock (_lock)
				{
					_managerInternals.OpenQueues[schema] = value;
				}
			}

			protected abstract Task<long> GetCountInternalAsync(string schema, CancellationToken cancellationToken);

			protected abstract void Dispose(bool disposing);

			protected virtual async Task CommitinternalAsync(IEnumerable<StateChange> stateChanges)
			{
				foreach (var stateChange in stateChanges)
				{
					switch (stateChange.ChangeType)
					{
						case StateChangeType.None:
							break;
						case StateChangeType.AddOrUpdate:
							await SetValueInternalAsync(SchemaStateKey.Parse(stateChange.Value.Id),
								stateChange.Value, stateChange.ValueType, CancellationToken.None);
							break;
						case StateChangeType.Remove:
							await RemoveInternalAsync(SchemaStateKey.Parse(stateChange.Value.Id),
								CancellationToken.None);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}
	}
}