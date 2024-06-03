using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoBogus;
using DomainLayer.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Repository.Realization;
namespace RepositoryLayer.Tests
{
    public class TestEntity : BaseEntity<int>
    {
        public string Name { get; set; }
    }

    public class TestEntityRepository : GenericRepository<TestEntity, int>
    {
        public TestEntityRepository(ApplicationDbContext context) : base(context)
        {
        }
    }

    [TestFixture]
    public class GenericRepositoryTests
    {
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;

        [SetUp]
        public void SetUp()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [TearDown]
        public void TearDown()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();
            }
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entities = AutoFaker.Generate<TestEntity>(3);
                context.TestEntities.AddRange(entities);
                await context.SaveChangesAsync();

                var repository = new TestEntityRepository(context);
                var result = await repository.GetAllAsync();

                result.Should().BeEquivalentTo(entities);
            }
        }

        [Test]
        public void Query_ShouldReturnQueryable()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entities = AutoFaker.Generate<TestEntity>(3);
                context.TestEntities.AddRange(entities);
                context.SaveChanges();

                var repository = new TestEntityRepository(context);
                var result = repository.Query();

                result.Should().BeAssignableTo<IQueryable<TestEntity>>();
                result.Should().BeEquivalentTo(entities.AsQueryable());
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entity = AutoFaker.Generate<TestEntity>();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                var repository = new TestEntityRepository(context);
                var result = await repository.GetByIdAsync(entity.Id);

                result.Should().BeEquivalentTo(entity);
            }
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var id = AutoFaker.Generate<int>();

                var repository = new TestEntityRepository(context);
                var result = await repository.GetByIdAsync(id);

                result.Should().BeNull();
            }
        }

        [Test]
        public async Task CreateAsync_ShouldAddEntity()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entity = AutoFaker.Generate<TestEntity>();

                var repository = new TestEntityRepository(context);
                await repository.CreateAsync(entity);

                var savedEntity = await context.TestEntities.FindAsync(entity.Id);
                savedEntity.Should().BeEquivalentTo(entity);
            }
        }

        [Test]
        public void CreateAsync_ShouldThrowArgumentNullException_WhenEntityIsNull()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var repository = new TestEntityRepository(context);

                Func<Task> act = async () => await repository.CreateAsync(null);
                act.Should().ThrowAsync<ArgumentNullException>();
            }
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entity = AutoFaker.Generate<TestEntity>();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                entity.Name = AutoFaker.Generate<string>();

                var repository = new TestEntityRepository(context);
                await repository.UpdateAsync(entity);

                var updatedEntity = await context.TestEntities.FindAsync(entity.Id);
                updatedEntity.Should().BeEquivalentTo(entity);
            }
        }

        [Test]
        public void UpdateAsync_ShouldThrowArgumentNullException_WhenEntityIsNull()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var repository = new TestEntityRepository(context);

                Func<Task> act = async () => await repository.UpdateAsync(null);
                act.Should().ThrowAsync<ArgumentNullException>();
            }
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveEntity()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entity = AutoFaker.Generate<TestEntity>();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                var repository = new TestEntityRepository(context);
                await repository.DeleteAsync(entity);

                var deletedEntity = await context.TestEntities.FindAsync(entity.Id);
                deletedEntity.Should().BeNull();
            }
        }

        [Test]
        public void DeleteAsync_ShouldThrowArgumentNullException_WhenEntityIsNull()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var repository = new TestEntityRepository(context);

                Func<Task> act = async () => await repository.DeleteAsync(null);
                act.Should().ThrowAsync<ArgumentNullException>();
            }
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldRemoveEntity_WhenEntityExists()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var entity = AutoFaker.Generate<TestEntity>();
                context.TestEntities.Add(entity);
                await context.SaveChangesAsync();

                var repository = new TestEntityRepository(context);
                var result = await repository.DeleteByIdAsync(entity.Id);

                result.Should().BeTrue();

                var deletedEntity = await context.TestEntities.FindAsync(entity.Id);
                deletedEntity.Should().BeNull();
            }
        }

        [Test]
        public async Task DeleteByIdAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var id = AutoFaker.Generate<int>();

                var repository = new TestEntityRepository(context);
                var result = await repository.DeleteByIdAsync(id);

                result.Should().BeFalse();
            }
        }

        [Test]
        public async Task SaveChangesAsync_ShouldSaveChanges()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var repository = new TestEntityRepository(context);

                // Adding a test entity to track changes
                var entity = AutoFaker.Generate<TestEntity>();
                await repository.CreateAsync(entity);

                // Making some changes to ensure SaveChangesAsync is necessary
                entity.Name = AutoFaker.Generate<string>();

                await repository.SaveChangesAsync();

                // Verify changes were saved
                var savedEntity = await context.TestEntities.FindAsync(entity.Id);
                savedEntity.Name.Should().Be(entity.Name);
            }
        }

        [Test]
        public void SaveChanges_ShouldSaveChanges()
        {
            using (var context = new TestApplicationDbContext(_dbContextOptions))
            {
                var repository = new TestEntityRepository(context);

                // Adding a test entity to track changes
                var entity = AutoFaker.Generate<TestEntity>();
                repository.CreateAsync(entity).Wait();

                // Making some changes to ensure SaveChanges is necessary
                entity.Name = AutoFaker.Generate<string>();

                repository.SaveChanges();

                // Verify changes were saved
                var savedEntity = context.TestEntities.Find(entity.Id);
                savedEntity.Name.Should().Be(entity.Name);
            }
        }
    }
}
