﻿using CapstoneProject_SP25_IPAS_BussinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_Repository.IRepository
{
    public interface IResourceRepository
    {
        public Task<List<Resource>> GetListResourceByGraftedNoteId(int graftedNoteId);
    }
}
