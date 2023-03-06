﻿using Dss.Application.Interfaces;
using Dss.Domain.Models;
using Dss.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dss.Application.Services
{
    public class AddDocumentService : RepositoryBase<AddDocument>, IAddDocumentService
    {
        public AddDocumentService(ApplicationDBContext context) : base(context)
        {
        }
        public async Task<string> AddDocumentAsync(AddDocument model)
        {
            Create(model);
            await SaveAsync();
            return model.correlationid.ToString();
        }
        public async Task<bool> UpdateAddDocumentAsync(AddDocument model)
        {
            Update(model);
            int rowsAffected = await Save();
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<IList<AddDocument>> GetAddDocumentAsync()
        {
            var res = await FindAllAsync();
            return res.OrderBy(x => x.correlationid).ToList();
        }

        public async Task<AddDocument?> GetAddDocumentAsync(Guid id)
        {
            var res = await FindByConditionAync(o => o.correlationid.Equals(id));
            return res.FirstOrDefault();
        }

        public async Task<bool> DeleteAddDocumentAsync(AddDocument model)
        {
            Delete(model);
            int rowsAffected = await Save();
            if (rowsAffected > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
