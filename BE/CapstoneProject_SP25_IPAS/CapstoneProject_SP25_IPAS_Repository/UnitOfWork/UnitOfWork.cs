﻿using CapstoneProject_SP25_IPAS_BussinessObject.Entities;
using CapstoneProject_SP25_IPAS_Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IpasContext _context;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction _transaction;


        // Repository
        private UserRepository _userRepo;
        private RoleRepository _roleRepo;
        private RefreshTokenRepository _refreshRepo;
        private ChatRoomRepository _chatRoomRepo;
        private FarmRepository _farmRepo;
        private TaskFeedbackRepository _taskFeedbackRepo;
        private UserWorkLogRepository _userWorkLogRepo;
        private PlanRepository _planRepo;
        private NotificationRepository _notificationRepo;
        private UserFarmRepository _userFarmRepo;
        private PlantLotRepository _plantLotRepo;
        private PlantRepository _plantRepo;
        private MasterTypeRepository _masterTypeRepo;
        private CriteriaRepository _criteriaRepo;
        private PartnerRepository _partnerRepo;
        private GrowthStageRepository _growthStageRepo;
        private ProcessRepository _processRepo;
        private SubProcessRepository _subProcessRepo;
        private LandPlotRepository _landPlotRepo;
        private LandPlotCoordinationRepository _landPlotCoordinationRepo;
        private CriteriaTargetRepository _criteriaTargetRepo;
        private LandRowRepository _landRowRepo;
        public PlantGrowthHistoryRepository _plantGrowthHistoryRepo;
        public CarePlanScheduleRepository _carePlanScheduleRepo;
        public WorkLogRepository _workLogRepo;
        public ResourceRepository _resourceRepo;
        public LegalDocumentRepository _legalDocumentRepo;
        public CropRepository _cropRepo;
        public HarvestHistoryRepository _harvestHistoryRepo;
        public HarvestTypeHistoryRepository _harvestTypeHistoryRepo;
        public LandPlotCropRepository _landPlotCropRepo;
        public Type_TypeRepository _type_TypeRepo;
        public PackageRepository _packageRepo;
        public OrdesRepository _ordesRepo;
        public PlanNotificationRepository _planNotificationRepository;
        public GraftedPlantRepository _graftedPlantRepo;
        public PlanTargetRepository _planTargetRepo;
        public GraftedPlantNoteRepository _graftedPlantNoteRepo;
        public PaymentRepository _paymentRepo;
        public ChatMessageRepository _chatMessageRepo;
        public ReportRepository _reportRepository;
        public SystemConfigRepository _systemConfigRepo;
        public EmployeeSkillRepository _employeeSkillRepo;
        public PackageDetailRepository _packageDetailRepo;
        public GrowthStagePlanRepository _growthStagePlanRepo;
        public UnitOfWork(IpasContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _userRepo = new UserRepository(context);
            _roleRepo = new RoleRepository(context);
            _refreshRepo = new RefreshTokenRepository(context);
            _chatRoomRepo = new ChatRoomRepository(context);
            _farmRepo = new FarmRepository(context);
            _taskFeedbackRepo = new TaskFeedbackRepository(context);
            _userWorkLogRepo = new UserWorkLogRepository(context);
            _planRepo = new PlanRepository(context);
            _notificationRepo = new NotificationRepository(context);
            _userFarmRepo = new UserFarmRepository(context);
            _plantLotRepo = new PlantLotRepository(context);
            _plantRepo = new PlantRepository(context);
            _masterTypeRepo = new MasterTypeRepository(context);
            _criteriaRepo = new CriteriaRepository(context);
            _partnerRepo = new PartnerRepository(context);
            _growthStageRepo = new GrowthStageRepository(context);
            _processRepo = new ProcessRepository(context);
            _subProcessRepo = new SubProcessRepository(context);
            _configuration = configuration;
            _landPlotRepo = new LandPlotRepository(context);
            _landPlotCoordinationRepo = new LandPlotCoordinationRepository(context);
            _planRepo = new PlanRepository(context);
            _landRowRepo = new LandRowRepository(context);
            _plantGrowthHistoryRepo = new PlantGrowthHistoryRepository(context);
            _carePlanScheduleRepo = new CarePlanScheduleRepository(context);
            _workLogRepo = new WorkLogRepository(context);
            _resourceRepo = new ResourceRepository(context);
            _legalDocumentRepo = new LegalDocumentRepository(context);
            _cropRepo = new CropRepository(context);
            _harvestHistoryRepo = new HarvestHistoryRepository(context);
            _harvestTypeHistoryRepo = new HarvestTypeHistoryRepository(context);
            _landPlotCropRepo = new LandPlotCropRepository(context);
            _type_TypeRepo = new Type_TypeRepository(context);
            _packageRepo = new PackageRepository(context);
            _ordesRepo = new OrdesRepository(context);
            _planNotificationRepository = new PlanNotificationRepository(context);
            _graftedPlantRepo = new GraftedPlantRepository(context);
            _planTargetRepo = new PlanTargetRepository(context);
            _graftedPlantNoteRepo = new GraftedPlantNoteRepository(context);
            _paymentRepo = new PaymentRepository(context);
            _reportRepository = new ReportRepository(context);
            _systemConfigRepo = new SystemConfigRepository(context);
            _employeeSkillRepo = new EmployeeSkillRepository(context);
            _packageDetailRepo = new PackageDetailRepository(context);
            _growthStagePlanRepo = new GrowthStagePlanRepository(context);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
            //throw new NotImplementedException();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null!;
            }
        }

        public async Task RollBackAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null!;
        }

        public UserRepository UserRepository
        {
            get
            {
                if (_userRepo == null)
                {
                    this._userRepo = new UserRepository(_context);
                }
                return _userRepo;
            }
        }

        public RoleRepository RoleRepository
        {
            get
            {
                if (_roleRepo == null)
                {
                    this._roleRepo = new RoleRepository(_context);
                }
                return _roleRepo;
            }
        }

        public RefreshTokenRepository RefreshTokenRepository
        {
            get
            {
                if (_refreshRepo == null)
                {
                    this._refreshRepo = new RefreshTokenRepository(_context);
                }
                return _refreshRepo;
            }
        }

        public ChatRoomRepository ChatRoomRepository
        {
            get
            {
                if (_chatRoomRepo == null)
                {
                    this._chatRoomRepo = new ChatRoomRepository(_context);
                }
                return _chatRoomRepo;
            }
        }

        public TaskFeedbackRepository TaskFeedbackRepository
        {
            get
            {
                if (_taskFeedbackRepo == null)
                {
                    this._taskFeedbackRepo = new TaskFeedbackRepository(_context);
                }
                return _taskFeedbackRepo;
            }
        }

        public UserWorkLogRepository UserWorkLogRepository
        {
            get
            {
                if (_userWorkLogRepo == null)
                {
                    this._userWorkLogRepo = new UserWorkLogRepository(_context);
                }
                return _userWorkLogRepo;
            }
        }

        public PlanRepository PlanRepository
        {
            get
            {
                if (_planRepo == null)
                {
                    this._planRepo = new PlanRepository(_context);
                }
                return _planRepo;
            }
        }

        public NotificationRepository NotificationRepository
        {
            get
            {
                if (_notificationRepo == null)
                {
                    this._notificationRepo = new NotificationRepository(_context);
                }
                return _notificationRepo;
            }
        }

        public UserFarmRepository UserFarmRepository
        {
            get
            {
                if (_userFarmRepo == null)
                {
                    this._userFarmRepo = new UserFarmRepository(_context);
                }
                return _userFarmRepo;
            }
        }

        public PlantLotRepository PlantLotRepository
        {
            get
            {
                if (_plantLotRepo == null)
                {
                    this._plantLotRepo = new PlantLotRepository(_context);
                }
                return _plantLotRepo;
            }
        }

        public PlantRepository PlantRepository
        {
            get
            {
                if (_plantRepo == null)
                {
                    this._plantRepo = new PlantRepository(_context);
                }
                return _plantRepo;
            }
        }
        public FarmRepository FarmRepository
        {
            get
            {
                if (_farmRepo == null)
                {
                    this._farmRepo = new FarmRepository(_context);
                }
                return _farmRepo;
            }
        }
        //public FarmCoordinationRepository FarmCoordinationRepository
        //{
        //    get
        //    {
        //        if (_farmCoordinationRepo == null)
        //        {
        //            this._farmCoordinationRepo = new FarmCoordinationRepository(_context);
        //        }
        //        return _farmCoordinationRepo;
        //    }
        //}
        public LandPlotRepository LandPlotRepository
        {
            get
            {
                if (_landPlotRepo == null)
                {
                    this._landPlotRepo = new LandPlotRepository(_context);
                }
                return _landPlotRepo;
            }
        }

        public LandPlotCoordinationRepository LandPlotCoordinationRepository
        {
            get
            {
                if(_landPlotCoordinationRepo == null)
                {
                    this._landPlotCoordinationRepo = new LandPlotCoordinationRepository(_context);
                }
                return _landPlotCoordinationRepo;
            }
        }

        public MasterTypeRepository MasterTypeRepository
        {
            get
            {
                if (_masterTypeRepo == null)
                {
                    this._masterTypeRepo = new MasterTypeRepository(_context);
                }
                return _masterTypeRepo;
            }
        }

        public PartnerRepository PartnerRepository
        {
            get
            {
                if (_partnerRepo == null)
                {
                    this._partnerRepo = new PartnerRepository(_context);
                }
                return _partnerRepo;
            }
        }

        public CriteriaRepository CriteriaRepository
        {
            get
            {
                if (_criteriaRepo == null)
                {
                    this._criteriaRepo = new CriteriaRepository(_context);
                }
                return _criteriaRepo;
            }
        }

        public GrowthStageRepository GrowthStageRepository
        {
            get
            {
                if (_growthStageRepo == null)
                {
                    this._growthStageRepo = new GrowthStageRepository(_context);
                }
                return _growthStageRepo;
            }
        }

        public CriteriaTargetRepository CriteriaTargetRepository
        {
            get
            {
                if (_criteriaTargetRepo == null)
                {
                    this._criteriaTargetRepo = new CriteriaTargetRepository(_context);
                }
                return _criteriaTargetRepo;
            }
        }

        public ProcessRepository ProcessRepository
        {
            get
            {
                if (_processRepo == null)
                {
                    this._processRepo = new ProcessRepository(_context);
                }
                return _processRepo;
            }
        }


        public SubProcessRepository SubProcessRepository
        {
            get
            {
                if (_subProcessRepo == null)
                {
                    this._subProcessRepo = new SubProcessRepository(_context);
                }
                return _subProcessRepo;
            }
        }


        public LandRowRepository LandRowRepository
        {
            get
            {
                if (_landRowRepo == null)
                {
                    this._landRowRepo = new LandRowRepository(_context);
                }
                return _landRowRepo;
            }
        }

        
        public PlantGrowthHistoryRepository PlantGrowthHistoryRepository
        {
            get
            {
                if (_plantGrowthHistoryRepo == null)
                {
                    this._plantGrowthHistoryRepo = new PlantGrowthHistoryRepository(_context);
                }
                return _plantGrowthHistoryRepo;
            }
        }

        public CarePlanScheduleRepository CarePlanScheduleRepository
        {
            get
            {
                if (_carePlanScheduleRepo == null)
                {
                    this._carePlanScheduleRepo = new CarePlanScheduleRepository(_context);
                }
                return _carePlanScheduleRepo;
            }
        }
        public WorkLogRepository WorkLogRepository
        {
            get
            {
                if (_workLogRepo == null)
                {
                    this._workLogRepo = new WorkLogRepository(_context);
                }
                return _workLogRepo;
            }
        }

        public ResourceRepository ResourceRepository
        {
            get
            {
                if (_resourceRepo == null)
                {
                    this._resourceRepo = new ResourceRepository(_context);
                }
                return _resourceRepo;
            }
        }

        public LegalDocumentRepository LegalDocumentRepository
        {
            get
            {
                if (_legalDocumentRepo == null)
                {
                    this._legalDocumentRepo = new LegalDocumentRepository(_context);
                }
                return _legalDocumentRepo;
            }
        }

        public CropRepository CropRepository
        {
            get
            {
                if (_cropRepo == null)
                {
                    this._cropRepo = new CropRepository(_context);
                }
                return _cropRepo;
            }
        }

        public HarvestHistoryRepository HarvestHistoryRepository
        {
            get
            {
                if (_harvestHistoryRepo == null)
                {
                    this._harvestHistoryRepo = new HarvestHistoryRepository(_context);
                }
                return _harvestHistoryRepo;
            }
        }

        public HarvestTypeHistoryRepository ProductHarvestHistoryRepository
        {
            get
            {
                if (_harvestTypeHistoryRepo == null)
                {
                    this._harvestTypeHistoryRepo = new HarvestTypeHistoryRepository(_context);
                }
                return _harvestTypeHistoryRepo;
            }
        }

        public LandPlotCropRepository LandPlotCropRepository
        {
            get
            {
                if (_landPlotCropRepo == null)
                {
                    this._landPlotCropRepo= new LandPlotCropRepository(_context);
                }
                return _landPlotCropRepo;
            }
        }

        public Type_TypeRepository Type_TypeRepository
        {
            get
            {
                if (_type_TypeRepo == null)
                {
                    this._type_TypeRepo = new Type_TypeRepository(_context);
                }
                return _type_TypeRepo;
            }
        }

        public PackageRepository PackageRepository
        {
            get
            {
                if (_packageRepo == null)
                {
                    this._packageRepo = new PackageRepository(_context);
                }
                return _packageRepo;
            }
        }

        public OrdesRepository OrdersRepository
        {
            get
            {
                if (_ordesRepo == null)
                {
                    this._ordesRepo= new OrdesRepository(_context);
                }
                return _ordesRepo;
            }
        }

        public PlanNotificationRepository PlanNotificationRepository
        {
            get
            {
                if (_planNotificationRepository == null)
                {
                    this._planNotificationRepository = new PlanNotificationRepository(_context);
                }
                return _planNotificationRepository;
            }
        }

        public GraftedPlantRepository GraftedPlantRepository
        {
            get
            {
                if (_graftedPlantRepo == null)
                {
                    this._graftedPlantRepo = new GraftedPlantRepository(_context);
                }
                return _graftedPlantRepo;
            }
        }

        public PlanTargetRepository PlanTargetRepository
        {
            get
            {
                if (_planTargetRepo == null)
                {
                    this._planTargetRepo = new PlanTargetRepository(_context);
                }
                return _planTargetRepo;
            }
        }

        public GraftedPlantNoteRepository GraftedPlantNoteRepository
        {
            get
            {
                if(_graftedPlantNoteRepo == null)
                {
                    this._graftedPlantNoteRepo = new GraftedPlantNoteRepository(_context);
                }
                return _graftedPlantNoteRepo;
            }
        }

        public PaymentRepository PaymentRepository
        {
            get
            {
                if (_paymentRepo == null)
                {
                    this._paymentRepo = new PaymentRepository(_context);
                }
                return _paymentRepo;
            }
        }

        public ChatMessageRepository ChatMessageRepository
        {
            get
            {
                if (_chatMessageRepo == null)
                {
                    this._chatMessageRepo = new ChatMessageRepository(_context);
                }
                return _chatMessageRepo;
            }
        }

        public ReportRepository ReportRepository
        {
            get
            {
                if (_reportRepository == null)
                {
                    this._reportRepository = new ReportRepository(_context);
                }
                return _reportRepository;
            }
        }

        public SystemConfigRepository SystemConfigRepository
        {
            get
            {
                if (_systemConfigRepo == null)
                {
                    this._systemConfigRepo = new SystemConfigRepository(_context);
                }
                return _systemConfigRepo;
            }
        }

        public EmployeeSkillRepository EmployeeSkillRepository
        {
            get
            {
                if (_employeeSkillRepo == null)
                {
                    this._employeeSkillRepo = new EmployeeSkillRepository(_context);
                }
                return _employeeSkillRepo;
            }
        }

        public PackageDetailRepository PackageDetailRepository
        {
            get
            {
                if (_packageDetailRepo == null)
                {
                    this._packageDetailRepo = new PackageDetailRepository(_context);
                }
                return _packageDetailRepo;
            }
        }

        public GrowthStagePlanRepository GrowthStagePlanRepository
        {
            get
            {
                if (_growthStagePlanRepo == null)
                {
                    this._growthStagePlanRepo = new GrowthStagePlanRepository(_context);
                }
                return _growthStagePlanRepo;
            }
        }
    }
}
