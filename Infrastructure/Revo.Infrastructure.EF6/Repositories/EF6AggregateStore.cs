﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.EF6.Model;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.EF6.Repositories
{
    public class EF6AggregateStore : IQueryableAggregateStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly IModelMetadataExplorer modelMetadataExplorer;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;

        public EF6AggregateStore(ICrudRepository crudRepository,
            IModelMetadataExplorer modelMetadataExplorer,
            IEntityTypeManager entityTypeManager,
            IPublishEventBuffer eventQueue,
            IEventMessageFactory eventMessageFactory)
        {
            this.crudRepository = crudRepository;
            this.modelMetadataExplorer = modelMetadataExplorer;
            this.entityTypeManager = entityTypeManager;
            this.publishEventBuffer = eventQueue;
            this.eventMessageFactory = eventMessageFactory;
        }

        public void Add<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Add(aggregate);
        }

        public T Get<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.Get<T>(id);
        }

        public Task<T> GetAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.GetAsync<T>(id);
        }

        public T Find<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.Find<T>(id);
        }

        public Task<T> FindAsync<T>(Guid id) where T : class, IAggregateRoot
        {
            return crudRepository.FindAsync<T>(id);
        }

        public IEnumerable<IAggregateRoot> GetTrackedAggregates()
        {
            return crudRepository.GetEntities<IAggregateRoot>();
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return modelMetadataExplorer.IsTypeMapped(aggregateType);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefault(predicate);
        }

        public T First<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.First(predicate);
        }

        public Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstOrDefaultAsync(predicate);
        }

        public Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FirstAsync(predicate);
        }

        public IQueryable<T> FindAll<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAll<T>();
        }

        public Task<IList<T>> FindAllAsync<T>() where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.FindAllAsync<T>();
        }

        public IQueryable<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IAggregateRoot, IQueryableEntity
        {
            return crudRepository.Where(predicate);
        }

        public void Remove<T>(T aggregate) where T : class, IAggregateRoot
        {
            crudRepository.Remove(aggregate);
        }

        public void SaveChanges()
        {
            InjectClassIds();
            crudRepository.SaveChanges();
            CommitAggregates();
        }

        public async Task SaveChangesAsync()
        {
            InjectClassIds();
            await crudRepository.SaveChangesAsync();
            await CommitAggregatesAsync();
        }

        private void InjectClassIds()
        {
            var addedClassEntitites =
                crudRepository.GetEntities<IBasicClassIdEntity>(Revo.DataAccess.Entities.EntityState.Added, Revo.DataAccess.Entities.EntityState.Modified);

            foreach (IBasicClassIdEntity entity in addedClassEntitites)
            {
                if (entity.ClassId == Guid.Empty)
                {
                    entity.ClassId = entityTypeManager.GetClassIdByClrType(entity.GetType());
                }
            }
        }
        private void CommitAggregates()
        {
            foreach (var aggregate in GetTrackedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    var eventMessages = Task.Run(async () => await CreateEventMessagesAsync(aggregate, aggregate.UncommittedEvents)).Result; // TODO
                    eventMessages.ForEach(publishEventBuffer.PushEvent);
                    aggregate.Commit();
                }
            }
        }

        private async Task CommitAggregatesAsync()
        {
            foreach (var aggregate in GetTrackedAggregates())
            {
                if (aggregate.IsChanged)
                {
                    var eventMessages = await CreateEventMessagesAsync(aggregate, aggregate.UncommittedEvents);
                    eventMessages.ForEach(publishEventBuffer.PushEvent);
                    aggregate.Commit();
                }
            }
        }

        private async Task<List<IEventMessageDraft>> CreateEventMessagesAsync(IAggregateRoot aggregate, IReadOnlyCollection<DomainAggregateEvent> events)
        {
            var messages = new List<IEventMessageDraft>();
            Guid? aggregateClassId = entityTypeManager.TryGetClassIdByClrType(aggregate.GetType());

            foreach (DomainAggregateEvent ev in events)
            {
                IEventMessageDraft message = await eventMessageFactory.CreateMessageAsync(ev);
                if (aggregateClassId != null)
                {
                    message.SetMetadata(BasicEventMetadataNames.AggregateClassId, aggregateClassId.Value.ToString());
                }

                messages.Add(message);
            }

            return messages;
        }
    }
}