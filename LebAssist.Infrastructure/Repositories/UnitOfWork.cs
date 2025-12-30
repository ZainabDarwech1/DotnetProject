using Domain.Interfaces;
using LebAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LebAssist.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories;

        // Lazy-loaded repository fields
        private IClientRepository? _clients;
        private ICategoryRepository? _categories;
        private IServiceRepository? _services;
        private IBookingRepository? _bookings;
        private IEmergencyRequestRepository? _emergencyRequests;
        private IReviewRepository? _reviews;
        private IReportRepository? _reports;
        private INotificationRepository? _notifications;
        private IProviderServiceRepository? _providerServices;
        private IProviderWorkingHoursRepository? _providerWorkingHours;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        // Lazy-loaded repository properties
        public IClientRepository Clients =>
            _clients ??= new ClientRepository(_context);

        public ICategoryRepository Categories =>
            _categories ??= new CategoryRepository(_context);

        public IServiceRepository Services =>
            _services ??= new ServiceRepository(_context);


        public IBookingRepository Bookings =>
            _bookings ??= new BookingRepository(_context);

        public IEmergencyRequestRepository EmergencyRequests =>
            _emergencyRequests ??= new EmergencyRequestRepository(_context);

        public IReviewRepository Reviews =>
            _reviews ??= new ReviewRepository(_context);

        public IReportRepository Reports =>
            _reports ??= new ReportRepository(_context);

        public INotificationRepository Notifications =>
            _notifications ??= new NotificationRepository(_context);

        public IProviderServiceRepository ProviderServices =>
            _providerServices ??= new ProviderServiceRepository(_context);

        public IProviderWorkingHoursRepository ProviderWorkingHours =>
            _providerWorkingHours ??= new ProviderWorkingHoursRepository(_context);

        public IRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<T>(_context);
            }

            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
