﻿using CapstoneProject_SP25_IPAS_BussinessObject.Entities;
using CapstoneProject_SP25_IPAS_Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_Repository.Repository
{
    public class PlanTargetRepository : GenericRepository<PlanTarget>, IPlanTargetRepository
    {
        private readonly IpasContext _context;

        public PlanTargetRepository(IpasContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PlanTarget>> GetPlanTargetsByPlanId(int planId)
        {
           var result = await _context.PlanTargets.Where(x => x.PlanID == planId).ToListAsync();
           return result;
        }
    }
}
