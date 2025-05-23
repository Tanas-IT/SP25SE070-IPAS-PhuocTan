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
    public class GraftedPlantNoteRepository : GenericRepository<GraftedPlantNote>, IGraftedPlantNoteRepository
    {
        private readonly IpasContext _context;

        public GraftedPlantNoteRepository(IpasContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<GraftedPlantNote>> GetListGraftedPlantNoteByGraftedId(int graftedPlantId)
        {
            var getListGraftedPlant = await _context.GraftedPlantNotes
                .Include(x => x.Resources)
                .Where(x => x.GraftedPlantId == graftedPlantId).ToListAsync();
            return getListGraftedPlant; 
        }
    }
}
